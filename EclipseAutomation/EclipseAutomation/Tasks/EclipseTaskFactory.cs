using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EclipseAutomation.Config;
using EclipseAutomation.Utility;
using NLog;

namespace EclipseAutomation.Tasks
{
    public class EclipseTaskFactory
    {
        private static Logger _logger = LogManager.GetLogger(nameof(EclipseTaskFactory));

        EclipseTimes _times;

        EclipseTaskParameters _taskParameters = null;
        StackDetails _annularStack = null;
        StackDetails _secondContactStack = null;
        StackDetails _totalityStack = null;

        public EclipseTaskFactory(
            EclipseTimesConfig eclipseTimesConfig,
            EclipseTaskParameters taskParameters,
            StackDetails annularStack,
            StackDetails secondContactStack,
            StackDetails totalityStack)
        {
            _times = new EclipseTimes(eclipseTimesConfig);
            _taskParameters = taskParameters;
            _annularStack = annularStack;
            _secondContactStack = secondContactStack;
            _totalityStack = totalityStack;
        }

        public EclipseTaskFactory(
            EclipseTimes eclipseTimes,
            EclipseTaskParameters taskParameters,
            StackDetails annularStack,
            StackDetails secondContactStack,
            StackDetails totalityStack)
        {
            _times = eclipseTimes;
            _taskParameters = taskParameters;
            _annularStack = annularStack;
            _secondContactStack = secondContactStack;
            _totalityStack = totalityStack;
        }

        public static EclipseTaskFactory GetDefaultEclipseTaskFactory()
        {
            // My original settings for April 8, 2024 in Plano, TX
            EclipseTimesConfig eclipseTimesConfig = new EclipseTimesConfig
            {
                EclipseDate = DateTime.Parse("4/8/2024"),
                C1 = TimeSpan.Parse("12:24:02"),
                C2 = TimeSpan.Parse("13:41:35"),
                C3 = TimeSpan.Parse("13:45:03"),
                C4 = TimeSpan.Parse("15:03:13")
            };

            EclipseTaskParameters taskParameters = new EclipseTaskParameters
            {
                AnnularCount = 7,
                AnnularOffset = TimeSpan.Parse("00:05:00"),
                SecondContactWindow = TimeSpan.Parse("00:00:08"),
                TotalityInterval = TimeSpan.Parse("00:00:30")
            };

            StackDetails annularStack = new StackDetails
            {
                IsoNumber = "200",
                FNumber = "8",
                ExposureCompensation = "0.0",
                ShutterSpeeds = "1/2000;1/1000"
            };

            StackDetails secondContactStack = new StackDetails
            {
                IsoNumber = "200",
                FNumber = "8",
                ExposureCompensation = "0.0",
                ShutterSpeeds = "1/800;1/400;1/200"
            };

            StackDetails totalityStack = new StackDetails
            {
                IsoNumber = "400",
                FNumber = "8",
                ExposureCompensation = "0.0",
                ShutterSpeeds = "1/6400;1/3200;1/800;1/250;1/100;1/50;1/13;1/6"
            };

            return new EclipseTaskFactory(
                eclipseTimesConfig,
                taskParameters,
                annularStack,
                secondContactStack,
                totalityStack);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public virtual List<ScheduledTask> CreateScheduledTasks()
        {
            _logger.Info($"=== C1 at {_times.C1} ===");
            _logger.Info($"=== C2 at {_times.C2} ===");
            _logger.Info($"=== C3 at {_times.C3} ===");
            _logger.Info($"=== C4 at {_times.C4} ===");

            List<ScheduledTask> tasks = new List<ScheduledTask>();

            tasks.AddRange(AnnularTasks());
            tasks.AddRange(BoundaryTasks());
            tasks.AddRange(TotalityTasks());
            tasks.Add(new ScheduledTask("Complete", "Eclipse is over.\nNo more tasks.", _times.C4, _times.C4.AddHours(1), true, false));

            return tasks.OrderBy(t => t.ExecuteAt).ToList();
        }

        private IEnumerable<ScheduledTask> AnnularTasks()
        {
            List<ScheduledTask> tasks = new List<ScheduledTask>();

            // How long do totality exposures take?
            // so we can attempt to 'center' the stack at the scheduled time
            TimeSpan halfStackDuration = HalfStackDuration(AnnularStack());

            // Before C2...

            DateTime firstStack = _times.C1 + _taskParameters.AnnularOffset;
            DateTime lastStack = _times.C2 - _taskParameters.AnnularOffset;
            TimeSpan range = lastStack - firstStack;
            // Count-1 because we always want first and last
            TimeSpan interval = range.DivideBy(_taskParameters.AnnularCount - 1);

            List<ScheduledTask> preTasks = new List<ScheduledTask>();
            for (int i = 0; i < _taskParameters.AnnularCount; ++i)
            {
                DateTime executeAt = firstStack + interval.Multiply(i);

                if (i + 1 == _taskParameters.AnnularCount)
                {
                    // prefering the safety of saveToPCNotCameraSD=false over the speed increase
                    preTasks.Add(new StackedPhotoTask("Pre C2", "Coming soon, Baily's Beads!", executeAt - halfStackDuration, null, true, AnnularStack(), false));
                }
                else
                {
                    // prefering the safety of saveToPCNotCameraSD=false over the speed increase
                    preTasks.Add(new StackedPhotoTask("Pre C2", "Waiting for next annular photo capture", executeAt - halfStackDuration, null, false, AnnularStack(), false));
                }
            }

            //_logger.Debug($"Last annular before C2 target: {lastStack:HH\\:mm\\:ss\\.fff}");
            //_logger.Info($"{_taskParameters.AnnularCount} IMAGE STACKS DURING ANNULAR PERIOD (ON EACH SIDE OF TOTALITY)");

            // After C3
            firstStack = _times.C3 + _taskParameters.AnnularOffset;
            lastStack = _times.C4 - _taskParameters.AnnularOffset;
            range = lastStack - firstStack;
            // Count-1 because we always want first and last
            interval = range.DivideBy(_taskParameters.AnnularCount - 1);

            List<ScheduledTask> postTasks = new List<ScheduledTask>();
            for (int i = 0; i < _taskParameters.AnnularCount; ++i)
            {
                DateTime executeAt = firstStack + interval.Multiply(i);

                // prefering the safety of saveToPCNotCameraSD=false over the speed increase
                postTasks.Add(new StackedPhotoTask("Post C3", "Waiting for next annular photo capture", executeAt - halfStackDuration, null, false, AnnularStack(), false));
            }
            //_logger.Debug($"Last annular before C4 target: {lastStack:HH\\:mm\\:ss\\.fff}");

            tasks.AddRange(preTasks);
            tasks.AddRange(postTasks);
            return tasks;
        }

        private IEnumerable<ScheduledTask> BoundaryTasks()
        {
            List<ScheduledTask> tasks = new List<ScheduledTask>();

            // How long do 2nd contact exposures take?
            TimeSpan halfStackDuration = HalfStackDuration(C2Stack());

            // In the seconds around C2 and C3...
            // Attempting to align the center of the stack at the time
            var preC2 = _times.C2 - _taskParameters.SecondContactWindow - halfStackDuration;
            var postC2 = _times.C2 + _taskParameters.SecondContactWindow - halfStackDuration;

            // At least in my D7000, saveToPCNotCameraSD=true saves about 0.2s per image, giving me a chance at a third stack of these
            var c2Task = new StackedPhotoTask("At C2", $"Baily's beads and diamond ring effect\n{C2Stack().Count()} photos", preC2, postC2, false, C2Stack(), true);

            //_logger.Info($"=== BB Start at {preC2} ===");
            //_logger.Info($"=== C2 at {_times.C2} ===");
            //_logger.Info($"=== BB End at {postC2} ===");

            var filterOffStart = c2Task.EstimatedCompletion.AddSeconds(1);

            // NOTE: The settings for Baily's Beads assume the filter is in place
            // After the C2 events, filter should be removed
            var filterOffWarning = new SoundTask("Filter Off Warning", "!!! REMOVE FILTER NOW !!!", filterOffStart, filterOffStart.AddSeconds(8), true);

            var preC3 = _times.C3 - _taskParameters.SecondContactWindow - halfStackDuration;
            var postC3 = _times.C3 + _taskParameters.SecondContactWindow - halfStackDuration;

            //_logger.Info($"=== BB Start at {preC3} ===");
            //_logger.Info($"=== C3 at {_times.C3} ===");
            //_logger.Info($"=== BB End at {postC3} ===");

            // At least in my D7000, saveToPCNotCameraSD=true saves about 0.2s per image, giving me a chance at a third stack of these
            var c3Task = new StackedPhotoTask("At C3", $"Baily's beads and diamond ring effect\n{C2Stack().Count()} photos", preC3, postC3, false, C2Stack(), true);

            tasks.Add(c2Task);
            tasks.Add(filterOffWarning);
            tasks.Add(c3Task);
            return tasks;
        }

        private IEnumerable<ScheduledTask> TotalityTasks()
        {
            TimeSpan timeToReplaceFilter = TimeSpan.FromSeconds(10);

            // How long do totality exposures take?
            TimeSpan halfStackDuration = HalfStackDuration(TotalityStack());

            // Attempt to center the stack at max totality
            DateTime center = Center(_times.C2, _times.C3) - halfStackDuration;

            // work from the center out
            var postC2 = _times.C2 + _taskParameters.SecondContactWindow + timeToReplaceFilter;
            var preC3 = _times.C3 - _taskParameters.SecondContactWindow - timeToReplaceFilter;

            List<ScheduledTask> tasks = new List<ScheduledTask>();
            // at max totality
            // prefering the safety of saveToPCNotCameraSD=false over the speed increase
            tasks.Add(new StackedPhotoTask("MAX Totality", $"Experience Totality!\nNo Filter - {TotalityStack().Count()} photos", center, null, false, TotalityStack(), false));

            var minOffset = _taskParameters.TotalityInterval;

            var totalitySize = (preC3 - postC2).TotalMilliseconds;
            var halfTotalitySize = totalitySize / 2;
            var maxOffset = TimeSpan.FromMilliseconds(halfTotalitySize);// - _taskParameters.SecondContactWindow.TotalMilliseconds);

            int imageCount = 1; // the above is #1

            for (TimeSpan cur = minOffset;
                cur < maxOffset;
                cur += _taskParameters.TotalityInterval)
            {
                // prefering the safety of saveToPCNotCameraSD=false over the speed increase
                tasks.Add(new StackedPhotoTask("Totality", $"Experience Totality!\nNo Filter - {TotalityStack().Count()} photos", center - cur, null, false, TotalityStack(), false));
                tasks.Add(new StackedPhotoTask("Totality", $"Experience Totality!\nNo Filter - {TotalityStack().Count()} photos", center + cur, null, false, TotalityStack(), false));
                imageCount += 2;
            }

            //_logger.Info($"=== C2 at {_times.C2} ===");
            //_logger.Info($"=== First Totality {tasks.OrderBy(t => t.ExecuteAt).First().ExecuteAt} ===");
            //_logger.Info($"=== Last Totality {tasks.OrderBy(t => t.ExecuteAt).Last().ExecuteAt} ===");
            //_logger.Info($"=== C3 at {_times.C3} ===");

            _logger.Info($"{imageCount} IMAGE STACKS DURING TOTALITY");

            // last added task is closest to C3
            var lastTask = tasks.Last();
            DateTime lastTaskEnd = lastTask.EstimatedCompletion.AddSeconds(1);

            // NOTE: The settings for Baily's Beads assume the filter is in place
            // After the totality events, filter should be replaced
            var filterOnWarning = new SoundTask("Filter On Warning", "!!! REPLACE FILTER NOW !!!", lastTaskEnd, lastTaskEnd.AddSeconds(8), true);
            tasks.Add(filterOnWarning);

            return tasks;
        }

        private TimeSpan HalfStackDuration(IEnumerable<PhotoSettings> photoSettings) => StackDuration(photoSettings).DivideBy(2);
        private TimeSpan StackDuration(IEnumerable<PhotoSettings> photoSettings) => photoSettings.Sum(s => s.ExpectedCaptureDuration);

        public DateTime Center(DateTime first, DateTime second)
        {
            var diff = second - first;
            var half = diff.DivideBy(2);
            var center = first + half;
            return center;
        }

        private IEnumerable<PhotoSettings> AnnularStack()
        {
            PhotoSettings defaultSettings = new PhotoSettings
            {
                IsoNumber = _annularStack.IsoNumber,
                ShutterSpeed = "",
                FNumber = _annularStack.FNumber,
                ExposureCompensation = _annularStack.ExposureCompensation
            };
            // Loosely based on https://www.mreclipse.com/SEphoto/SEphoto.html
            // and http://xjubier.free.fr/en/site_pages/SolarEclipseExposure.html
            // and testing prior to eclipse day
            PhotoSettingsStackFactory settingsFactory = new PhotoSettingsStackFactory(defaultSettings);
            return settingsFactory.GetShutterStack(_annularStack.ShutterSpeeds);
        }

        private IEnumerable<PhotoSettings> C2Stack()
        {
            // Bracketing based on 
            // http://xjubier.free.fr/en/site_pages/SolarEclipseExposure.html
            PhotoSettings defaultSettings = new PhotoSettings
            {
                IsoNumber = _secondContactStack.IsoNumber,
                ShutterSpeed = "",
                FNumber = _secondContactStack.FNumber,
                ExposureCompensation = _secondContactStack.ExposureCompensation
            };
            // Loosely based on https://www.mreclipse.com/SEphoto/SEphoto.html
            // and http://xjubier.free.fr/en/site_pages/SolarEclipseExposure.html
            // and testing prior to eclipse day
            PhotoSettingsStackFactory settingsFactory = new PhotoSettingsStackFactory(defaultSettings);
            return settingsFactory.GetShutterStack(_secondContactStack.ShutterSpeeds);
        }

        private IEnumerable<PhotoSettings> TotalityStack()
        {
            PhotoSettings defaultSettings = new PhotoSettings
            {
                IsoNumber = _totalityStack.IsoNumber,
                ShutterSpeed = "",
                FNumber = _totalityStack.FNumber,
                ExposureCompensation = _totalityStack.ExposureCompensation
            };
            // Loosely based on https://www.mreclipse.com/SEphoto/SEphoto.html
            // and http://xjubier.free.fr/en/site_pages/SolarEclipseExposure.html
            // and testing prior to eclipse day (using a mostly full moon)
            PhotoSettingsStackFactory settingsFactory = new PhotoSettingsStackFactory(defaultSettings);
            return settingsFactory.GetShutterStack(_totalityStack.ShutterSpeeds);
        }
    }
}
