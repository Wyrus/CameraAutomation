using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CameraControl.Devices;
using EclipseAutomation.Tasks;
using NLog;
using CameraControl.Devices.Classes;
using EclipseAutomation.Utility;
using System.Threading;

namespace EclipseAutomation.Camera
{
   internal class NikonD7000Handler : ICameraHandler, IDisposable
   {
      private static Logger _logger = LogManager.GetLogger(nameof(NikonD7000Handler));
      private static Logger _dmLogger = LogManager.GetLogger("DM");

      const string MY_CAMERA = "D7000";
      const int LIVE_VIEW_RATE = 1000 / 30;   // ~20fps
      const int MAX_WAIT_FOR_PHOTO_CAPTURE = 2500;

      ILiveViewDisplay _liveViewDisplay;
      ICameraDevice _captureCamera = null;
      CountdownEvent _photoCapturedEvent = null;
      string _imageSavePath = string.Empty;
      System.Threading.Timer _liveViewTimer = null;
      bool _liveViewTimerBusy = false;
      bool _saveToPCNotCameraSD = false;
      CameraDeviceManager _devMgr = null;

      Statistic _takePhotoStatistics = new Statistic();
      Statistic _awaitCaptureStatistics = new Statistic();
      Statistic _awaitDownloadStatistics = new Statistic();
      Statistic _liveViewFrameCaptureStatistics = new Statistic();

      public Statistic TakePhotoStatistics => _takePhotoStatistics;
      public Statistic AwaitCaptureStatistics => _awaitCaptureStatistics;
      public Statistic AwaitDownloadStatistics => _awaitDownloadStatistics;
      public Statistic LiveViewFrameCaptureStatistics => _liveViewFrameCaptureStatistics;

      public CameraStates CameraState { get; private set; } = CameraStates.NoCamera;

      public NikonD7000Handler(ILiveViewDisplay liveViewDisplay, string imageSavePath)
      {
         _devMgr = new CameraDeviceManager();
         _devMgr.StartInNewThread = false;
         _devMgr.DetectWebcams = false;
         _devMgr.UseExperimentalDrivers = false;
         _devMgr.LoadWiaDevices = false;
         _devMgr.CameraConnected += DevMgr_CameraConnected;
         _devMgr.CameraDisconnected += DevMgr_CameraDisconnected;
         _devMgr.PhotoCaptured += DevMgr_PhotoCaptured;

         // Internal camera library log messages
         Log.LogInfo += Log_LogInfo;
         Log.LogDebug += Log_LogDebug;
         Log.LogError += Log_LogError;

         // start looking for cameras
         _devMgr.ConnectToCamera();
         _imageSavePath = imageSavePath;

         if (!String.IsNullOrEmpty(imageSavePath))
         {
            // ensure directory exists
            var dir = new DirectoryInfo(imageSavePath);
            dir.Create();
         }

         _liveViewDisplay = liveViewDisplay;
         if (_liveViewDisplay != null)
         {
            _liveViewTimer = new System.Threading.Timer(LiveViewTimer_Tick, null, System.Threading.Timeout.Infinite, LIVE_VIEW_RATE);
         }
      }

      private void Log_LogError(LogEventArgs e)
      {
         _dmLogger.Error(e.Exception, $"{e.Message}");
      }

      private void Log_LogDebug(LogEventArgs e)
      {
         _dmLogger.Debug(e.Exception, $"{e.Message}");
      }

      private void Log_LogInfo(LogEventArgs e)
      {
         _dmLogger.Info(e.Exception, $"{e.Message}");
      }

      public void Dispose()
      {
         // Let photo stack finish
         while (CameraState == CameraStates.TakingPhotos)
         {
            System.Threading.Thread.Sleep(25);
         }
         StopLiveView();

         _devMgr.CloseAll();
      }

      public void TakePhotos(IEnumerable<PhotoSettings> settings, bool saveToPCNotCameraSD, int tries = 2)
      {
         if (CameraState != CameraStates.Idle)
         {
            _logger.Warn($"Cannot take photos, camera state is {CameraState}");
            return;
         }
         if (tries < 1)
            throw new ArgumentOutOfRangeException(nameof(tries), "Must be >= 1");

         _saveToPCNotCameraSD = saveToPCNotCameraSD;

         // TODO: Determine how many images the camera will report (raw + jpgw)
         var compressionSetting = _captureCamera.CompressionSetting.Value;
         int nImagesPerPhoto = 1;
         if (compressionSetting.Contains("+"))
            nImagesPerPhoto = 2;
         _logger.Info($"CompressionSetting = {compressionSetting} => {nImagesPerPhoto} images per photo");

         if (_saveToPCNotCameraSD)
            _captureCamera.CaptureInSdRam = true; // downloading without writing to camera SD might be faster?
         else
            _captureCamera.CaptureInSdRam = false; // Not required and won't interfere with photos

         _photoCapturedEvent = new CountdownEvent(nImagesPerPhoto);

         try
         {
            WaitForNotBusy("Waiting before taking photo sequence");

            CameraState = CameraStates.TakingPhotos;
            _captureCamera.LockCamera();

            PhotoSettings curSettings = new PhotoSettings();
            curSettings.IsoNumber = _captureCamera.IsoNumber.Value;
            curSettings.ShutterSpeed = _captureCamera.ShutterSpeed.Value;
            curSettings.FNumber = _captureCamera.FNumber.Value;
            curSettings.ExposureCompensation = _captureCamera.ExposureCompensation.Value;

            _captureCamera.WhiteBalance.Value = "Daylight";
            foreach (var setting in settings)
            {
               if (setting.IsoNumber != curSettings.IsoNumber)
                  _captureCamera.IsoNumber.Value = setting.IsoNumber;
               if (setting.ShutterSpeed != curSettings.ShutterSpeed)
                  _captureCamera.ShutterSpeed.Value = setting.ShutterSpeed;
               if (setting.FNumber != curSettings.FNumber)
                  _captureCamera.FNumber.Value = setting.FNumber;
               if (setting.ExposureCompensation != curSettings.ExposureCompensation)
                  _captureCamera.ExposureCompensation.Value = setting.ExposureCompensation;
               curSettings = setting;

               _photoCapturedEvent.Reset();

               using (new TimedActivity("TakePhoto()", _logger, _takePhotoStatistics))
               {
                  _logger.Info($"Taking photo: {setting.ShutterSpeed}s, ISO {setting.IsoNumber}, f{setting.FNumber}. Exposure {setting.ExposureCompensation}");
                  TakePhoto(tries);
               }

               Statistic awaitStat = _awaitCaptureStatistics;
               if (_saveToPCNotCameraSD)
                  awaitStat = _awaitDownloadStatistics;
               using (new TimedActivity("Photo capture wait", _logger, awaitStat))
               {
                  if (!_photoCapturedEvent.Wait(MAX_WAIT_FOR_PHOTO_CAPTURE))
                     _logger.Warn("TIMEOUT while waiting for photo capture to complete.");
               }
            }
         }
         finally
         {
            _captureCamera.UnLockCamera();
            CameraState = CameraStates.Idle;
         }
      }

      private void TakePhoto(int tries)
      {
         if (tries < 1)
            tries = 1;

         int attemptsRemaining = tries;
         while (attemptsRemaining > 0)
         {
            attemptsRemaining--;
            try
            {
               if (_captureCamera == null)
                  _logger.Error("Camera disconnected while taking photos");

               _captureCamera?.CapturePhoto();
               return;
            }
            catch (Exception e)
            {
               if (attemptsRemaining > 0)
               {
                  _logger.Warn(e, "While taking photo - will retry");
                  WaitForNotBusy();
               }
               else
                  _logger.Error(e, "While taking photo - failed");
            }
         }
      }

      public bool StartLiveView(PhotoSettings settings)
      {
         if (CameraState != CameraStates.Idle)
         {
            _logger.Warn($"Cannot start live view, camera state is {CameraState}");
            return false;
         }

         if (_captureCamera.IsBusy)
         {
            _logger.Debug("StartLiveView: Camera busy, wait...");
            return false;
         }

         try
         {
            _logger.Debug("Starting Live View");

            _captureCamera.WhiteBalance.Value = "Daylight";
            _captureCamera.ExposureCompensation.Value = settings?.ExposureCompensation ?? "0.0";
            _captureCamera.IsoNumber.Value = settings?.IsoNumber ?? "800";
            _captureCamera.FNumber.Value = settings?.FNumber ?? "8";
            _captureCamera.ShutterSpeed.Value = settings?.ShutterSpeed ?? "1/800";
            _captureCamera.StartLiveView();
            _liveViewTimer.Change(LIVE_VIEW_RATE, LIVE_VIEW_RATE);
            CameraState = CameraStates.LiveViewing;
            return true;
         }
         catch (Exception ex)
         {
            _logger.Error(ex, "Failed to Start Live View");
            return false;
         }
         finally
         {
            _logger.Debug("Live View Started");
         }
      }

      public void StopLiveView()
      {
         try
         {
            _logger.Debug("Stopping Live View");
            _liveViewTimer.Change(System.Threading.Timeout.Infinite, LIVE_VIEW_RATE);

            if (CameraState == CameraStates.NoCamera)
            {
               _logger.Warn($"Cannot StopLiveView, camera state is {CameraState}");
               return;
            }

            CameraState = CameraStates.Idle;

            _captureCamera?.StopLiveView();

            // capture the last image
            _ = _captureCamera.GetLiveViewImage();
         }
         catch (Exception ex)
         {
            _logger.Error(ex, "Stop Live View");
         }
         finally
         {
            _logger.Debug("Live View Stopped");
         }
      }

      private void LiveViewTimer_Tick(object state)
      {
         if (_liveViewDisplay == null)
         {
            return;
         }
         if (_liveViewTimerBusy)
            return;

         // probably caught a stray tick while stopping
         // LiveView update rate is fast enough we may still catch an exception
         if (CameraState != CameraStates.LiveViewing)
            return;

         try
         {
            _liveViewTimerBusy = true;
            LiveViewData lvData;

            // no logger because we don't want logs at the ~20fps rate, but we still want the average
            using (new TimedActivity("LiveViewImageCapture", _liveViewFrameCaptureStatistics))
            {
               lvData = _captureCamera.GetLiveViewImage();
               if (lvData?.ImageData == null)
               {
                  _logger.Warn("GetLiveViewImage has no image");
                  return;
               }

               MemoryStream stream = new MemoryStream(
                   lvData.ImageData,
                   lvData.ImageDataPosition,
                   lvData.ImageData.Length - lvData.ImageDataPosition);

               using (var res = new Bitmap(stream))
               {
                  Bitmap clone = new Bitmap(res.Width, res.Height, PixelFormat.Format32bppArgb);
                  using (var copy = Graphics.FromImage(clone))
                  {
                     copy.DrawImage(res, 0, 0);
                  }

                  _liveViewDisplay.RenderLiveViewImage(clone);
               }
            }
         }
         catch (Exception ex)
         {
            _logger.Error(ex);
         }
         finally
         {
            _liveViewTimerBusy = false;
         }
      }

      private void WaitForNotBusy(String msg, int timeoutMS = 2000)
      {
         using (new TimedActivity(msg, _logger))
         {
            WaitForNotBusy(timeoutMS);
         }
      }
      private void WaitForNotBusy(int timeoutMS = 2000)
      {
         if (_captureCamera == null)
            return;
         if (!_captureCamera.IsBusy)
            return;

         Stopwatch sw = Stopwatch.StartNew();

         while (_captureCamera.IsBusy && sw.ElapsedMilliseconds < timeoutMS)
         {
            System.Threading.Thread.Sleep(25);
         }
         sw.Stop();
         if (_captureCamera.IsBusy)
            _logger.Warn($"WaitForNotBusy timed out after {sw.Elapsed.TotalSeconds:0.000} sec");
         else
            _logger.Debug($"WaitForNotBusy took {sw.Elapsed.TotalSeconds:0.000} sec");
      }

      private void DevMgr_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
      {
         _logger.Info($"PhotoCaptured: {eventArgs.FileName}");

         if (_saveToPCNotCameraSD)
         {
            var ext = Path.GetExtension(eventArgs.FileName);
            var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filepath = Path.Combine(_imageSavePath, $"{ts}{ext}");
            eventArgs.CameraDevice.TransferFile(eventArgs.Handle, filepath);
            _logger.Info($"...saved to {filepath}");
         }
         eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);
         eventArgs.CameraDevice.IsBusy = false;
         _photoCapturedEvent.Signal();
      }

      private void DevMgr_CameraConnected(ICameraDevice cameraDevice)
      {
         _logger.Info($"Connected: {cameraDevice.DeviceName}");
         if (cameraDevice.DeviceName == MY_CAMERA)
         {
            _captureCamera = cameraDevice;

            CameraState = CameraStates.Idle;
         }
      }

      private void DevMgr_CameraDisconnected(ICameraDevice cameraDevice)
      {
         _logger.Info($"Disconnected: {cameraDevice.DeviceName}");
         if (cameraDevice.DeviceName == MY_CAMERA)
         {
            CameraState = CameraStates.NoCamera;
            _liveViewTimer.Change(System.Threading.Timeout.Infinite, LIVE_VIEW_RATE);
            _captureCamera = null;
         }
      }
   }
}
