using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EclipseAutomation.Camera;
using EclipseAutomation.Config;
using EclipseAutomation.Tasks;
using EclipseAutomation.Utility;
using NLog;

namespace EclipseAutomation
{
   public partial class MainForm : Form, ILiveViewDisplay
   {
      // High-level test flag
      private readonly bool TEST_MODE = false;
      private bool _showHistogramOnLiveView = true;

      // Just in case you forget to leave TEST_MODE enabled
      // (Wouldn't that just be the worst!)
      // IsTesting will not return true near the day of the eclipse
      private bool IsTesting =>
          TEST_MODE
          && Math.Abs((_settings.EclipseTimes.EclipseDate - DateTime.Now).TotalDays) > 2;

      private static Logger _logger = LogManager.GetLogger(nameof(MainForm));

      System.Threading.Timer _runTimer;
      bool _timerBusy = false;

      ICameraHandler _cameraHandler = null;
      TaskQueue _scheduledTasks = null;
      EclipseImagingSettings _settings;

      private const int RUNTIMER_PERIOD = 500;
      private readonly TimeSpan LIVE_VIEW_WINDOW = TimeSpan.FromSeconds(2);

      public MainForm()
      {
         InitializeComponent();

         lblCountdown.Text = string.Empty;
         lblNextStep.Text = string.Empty;
      }

      private bool LiveViewing => (_cameraHandler?.CameraState ?? CameraStates.Idle) == CameraStates.LiveViewing;

      protected override void OnLoad(EventArgs e)
      {
         base.OnLoad(e);

         _cameraHandler = new NikonD7000Handler(this, Properties.Settings.Default.ImageSavePath);

         _settings = (EclipseImagingSettings)ConfigurationManager.GetSection("eclipseSettings");
         EclipseTaskFactory taskFactory;

         DateTime now = DateTime.Now;
         if (IsTesting)
         {
            _logger.Warn("\n\nNOT USING CONFIGURED SETTINGS!!!\n\n");
            _logger.Warn("\n\nUSING TEST SETTINGS!!!\n\n");

            // adjust settings as needed for testing

            // The below causes the tasks from C2 to C3 to run in real time.
            // However, C2 is adjusted to be very soon
            TimeSpan offset = (now.Date + _settings.EclipseTimes.C2) - now - TimeSpan.FromSeconds(60);

            EclipseTimes times = new EclipseTimes(
                now.Date,
                _settings.EclipseTimes.C1 - offset,
                _settings.EclipseTimes.C2 - offset,
                _settings.EclipseTimes.C3 - offset,
                _settings.EclipseTimes.C4 - offset
            );

            taskFactory = new EclipseTaskFactory(
                times,
                _settings.TaskParameters,
                _settings.AnnularStack,
                _settings.SecondContactStack,
                _settings.TotalityStack);
         }
         else
         {
            taskFactory = new EclipseTaskFactory(
                _settings.EclipseTimes,
                _settings.TaskParameters,
                _settings.AnnularStack,
                _settings.SecondContactStack,
                _settings.TotalityStack);
         }

         _scheduledTasks = new TaskQueue(taskFactory.CreateScheduledTasks());

         _runTimer = new System.Threading.Timer(RunTimer_Tick, null, RUNTIMER_PERIOD, RUNTIMER_PERIOD);
      }

      private string StatString(Statistic stat)
      {
         return $"{stat.Average:0.000} (Min={stat.Min:0.000}, Max={stat.Max:0.000})";
      }

      protected override void OnClosing(CancelEventArgs e)
      {
         _runTimer.Change(Timeout.Infinite, 0);

         // recording and logging these to help figure out the fastest camera settings
         _logger.Info($"TakePhoto average: {StatString(_cameraHandler.TakePhotoStatistics)}");
         _logger.Info($"Awaiting Capture: {StatString(_cameraHandler.AwaitCaptureStatistics)}");
         _logger.Info($"Awaiting Download: {StatString(_cameraHandler.AwaitDownloadStatistics)}");
         double lvFPS = 0;
         if (_cameraHandler.LiveViewFrameCaptureStatistics.Average > 0)
            lvFPS = 1 / _cameraHandler.LiveViewFrameCaptureStatistics.Average;

         _logger.Info($"LiveView Frame Capture: {StatString(_cameraHandler.LiveViewFrameCaptureStatistics)}; {lvFPS:0.0}fps");

         DisableLiveView();
         base.OnClosing(e);
      }

      private void RunTimer_Tick(object state)
      {
         if (_timerBusy)
            return;

         try
         {
            _timerBusy = true;
            var now = DateTime.Now;
            var task = _scheduledTasks.NextTask(now);
            UpdateDisplay(task);

            // run the task
            if (task.ExecuteAt <= now)
            {
               // scheduled for more than 2 seconds ago, and not a long running task...
               if ((now - task.ExecuteAt).TotalSeconds > 2 && task.ExecuteUntil < now)
               {
                  _logger.Warn($"POSSIBLE SLIPPAGE, Execution was scheduled for {task.ExecuteAt:HH:mm:ss}");
               }
               _logger.Info($"EXECUTE {task}");
               task.ExecuteTask(_cameraHandler);
               return;
            }

            // if task doesn't use the camera, or the task is not soon, enable liveview
            if (!task.UsesCamera || task.ExecuteAt > (now + LIVE_VIEW_WINDOW))
               EnableLiveView();
            else
               DisableLiveView();
         }
         catch (Exception ex)
         {
            _logger.Error(ex, "During main window timer tick");
         }
         finally
         {
            _timerBusy = false;
         }

      }

      private void UpdateDisplay(ScheduledTask task)
      {
         this.Invoke((MethodInvoker)(
             () =>
             {
                lblNextStep.Text = task.Description;

                TimeSpan eta = task.ExecuteAt - DateTime.Now;
                if (eta.TotalMilliseconds < 0)
                   eta = TimeSpan.Zero;

                lblCountdown.Text = eta.ToString(@"hh\:mm\:ss");
                if (task.WarningFlag)
                   lblNextStep.BackColor = Color.Orange;
                else
                   lblNextStep.BackColor = Color.FromKnownColor(KnownColor.Control);

                lblClock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
             }
         ));
      }

      private void EnableLiveView()
      {
         if (LiveViewing)
            return;

         if (_cameraHandler.StartLiveView(null))
         {
            UpdateLiveViewStateIndication();
         }
      }

      private void DisableLiveView()
      {
         if (!LiveViewing)
            return;

         _cameraHandler.StopLiveView();
         UpdateLiveViewStateIndication();
      }

      private void MainForm_KeyUp(object sender, KeyEventArgs e)
      {
         // The only commands available near/during the eclipse
         switch (e.KeyCode)
         {
            case Keys.L:
               var tasks = _scheduledTasks.AllTasks;
               _logger.Info("Task List");
               DateTime lastCompletion = tasks.First().ExecuteAt.AddSeconds(-30);

               foreach (var task in tasks)
               {
                  var gap = task.ExecuteAt - lastCompletion;
                  _logger.Info($"GAP of {gap:mm\\:ss\\.fff}");

                  bool tooSoon = gap.TotalSeconds < 1.0;
                  if (tooSoon)
                     _logger.Error($"{task} - {task.Description}");
                  else
                     _logger.Info($"{task} - {task.Description}");

                  lastCompletion = task.EstimatedCompletion;
               }
               return;

            case Keys.H:
               _logger.Info("Toggle live view histogram");
               _showHistogramOnLiveView = !_showHistogramOnLiveView;
               return;

            default:
               _logger.Info($"Keypress {e.KeyCode} ignored");
               break;
         }

         // During the eclipse, IGNORE everything below this
         if (!IsTesting)
         {
            return;
         }

         if (_timerBusy)
         {
            _logger.Warn($"Keypress {e.KeyCode} - Ignored while timer.busy");
            return;
         }
         if (_cameraHandler.CameraState == CameraStates.NoCamera)
         {
            _logger.Warn($"Keypress {e.KeyCode} - No camera connected.");
            return;
         }

         bool liveViewWasEnabled = LiveViewing;
         try
         {
            _timerBusy = true;
            DisableLiveView();
            if (liveViewWasEnabled)
            {
               // give live view a moment to finish
               System.Threading.Thread.Sleep(250);
            }

            PhotoSettingsStackFactory stackFactory;
            List<PhotoSettings> stack;

            switch (e.KeyCode)
            {
               case Keys.A:
                  stackFactory = new PhotoSettingsStackFactory(_settings.AnnularStack);
                  stack = stackFactory.GetShutterStack(_settings.AnnularStack.ShutterSpeeds);
                  using (new TimedActivity($"==== Annular Stack ({stack.Count()}) =====", _logger))
                  {
                     // set saveToPCNotCameraSD same as done by EclipseTaskFactory
                     _cameraHandler.TakePhotos(stack, false);
                  }
                  _logger.Info($"Expected annular stack duration is {stack.Sum(s => s.ExpectedCaptureDuration):0.000} sec");
                  break;
               case Keys.S:
                  stackFactory = new PhotoSettingsStackFactory(_settings.SecondContactStack);
                  stack = stackFactory.GetShutterStack(_settings.SecondContactStack.ShutterSpeeds);
                  using (new TimedActivity($"===== Second Contact Stack ({stack.Count()}) =====", _logger))
                  {
                     // set saveToPCNotCameraSD same as done by EclipseTaskFactory
                     _cameraHandler.TakePhotos(stack, true);
                  }
                  _logger.Info($"Expected second contact stack duration is {stack.Sum(s => s.ExpectedCaptureDuration):0.000} sec");
                  break;
               case Keys.T:
                  stackFactory = new PhotoSettingsStackFactory(_settings.TotalityStack);
                  stack = stackFactory.GetShutterStack(_settings.TotalityStack.ShutterSpeeds);
                  using (new TimedActivity($"===== Totality Stack ({stack.Count()}) =====", _logger))
                  {
                     // set saveToPCNotCameraSD same as done by EclipseTaskFactory
                     _cameraHandler.TakePhotos(stack, false);
                  }
                  _logger.Info($"Expected totality stack duration is {stack.Sum(s => s.ExpectedCaptureDuration):0.000} sec");
                  break;
               case Keys.P:
                  stack = new List<PhotoSettings>();
                  stack.Add(new PhotoSettings
                  {
                     IsoNumber = "800",
                     FNumber = "8",
                     ExposureCompensation = "0.0",
                     ShutterSpeed = "1/400"
                  });
                  using (new TimedActivity("Single Photo", _logger))
                  {
                     // set saveToPCNotCameraSD same as done by EclipseTaskFactory
                     _cameraHandler.TakePhotos(stack, false);
                  }
                  break;
               default:
                  _logger.Info($"Keypress {e.KeyCode}, NOP");
                  break;
            }
         }
         catch (Exception ex)
         {
            _logger.Error(ex, "During key handler");
         }
         finally
         {
            if (liveViewWasEnabled)
               EnableLiveView();

            _timerBusy = false;
         }
      }

      bool _liveViewDisplayBusy = false;

      public void RenderLiveViewImage(Bitmap image)
      {
         if (_liveViewDisplayBusy)
         {
            _logger.Warn("Live view display was busy");
            return;
         }

         try
         {
            _liveViewDisplayBusy = true;
            if (_showHistogramOnLiveView)
            {
               image = new HistogramCalc(0.5f).OverlayHistogram(image);
            }
            pbLiveView.Invoke(
                (Action<Bitmap>)(
                (img) => { pbLiveView.Image = image; }),
                image);
         }
         finally
         {
            _liveViewDisplayBusy = false;
         }
      }

      public void UpdateLiveViewStateIndication()
      {
         Color bkColor = Color.FromKnownColor(KnownColor.Control);
         if (!LiveViewing)
            bkColor = Color.Red;

         pbLiveView.Invoke(
             (Action<Color>)(
             (color) => { pbLiveView.BackColor = color; }),
             bkColor);
      }

      private void exitToolStripMenuItem_Click(object sender, EventArgs e)
      {
         this.Close();
      }
   }
}
