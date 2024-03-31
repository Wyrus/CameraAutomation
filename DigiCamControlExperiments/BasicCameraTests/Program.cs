using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using NLog;

namespace BasicCameraTests
{
    internal class Program
    {
        private static Logger _logger = LogManager.GetLogger("MAIN");

        private static string MY_CAMERA = "D7000";
        private static string DOWNLOAD_PATH = @"C:\temp\DigiCamControlExperiments";

        // Some notes
        // https://www.bhphotovideo.com/explora/photography/tips-and-solutions/how-to-photograph-a-solar-eclipse

        static void Main(string[] args)
        {
            try
            {
                _logger.Debug("Initializing");
                Log.LogInfo += DM_LogInfo;
                Log.LogDebug += DM_LogDebug;
                Log.LogError += DM_LogError;

                CameraDeviceManager devMgr = new CameraDeviceManager();
                devMgr.StartInNewThread = false;
                devMgr.DetectWebcams = false;
                devMgr.UseExperimentalDrivers = false;
                devMgr.LoadWiaDevices = false;
                devMgr.CameraConnected += DevMgr_CameraConnected;
                devMgr.CameraDisconnected += DevMgr_CameraDisconnected;
                devMgr.PhotoCaptured += DevMgr_PhotoCaptured;

                // start looking for cameras
                _logger.Debug("Start listening for cameras");
                devMgr.ConnectToCamera();

                Console.WriteLine("Press X to exit...");

                while (true)
                {
                    try
                    {
                        var key = Console.ReadKey();
                        if (key.Key == ConsoleKey.X)
                        {
                            _logger.Info("Shutting down");
                            return;
                        }
                        if (key.Key == ConsoleKey.L)
                        {
                            ListCameras(devMgr);
                        }
                        if (key.Key == ConsoleKey.P)
                        {
                            TakePhoto(devMgr);
                        }
                        if (key.Key == ConsoleKey.D)
                        {
                            DownloadPhotos(devMgr);
                        }
                        if (key.Key == ConsoleKey.E)
                        {
                            TakeExposureStack(devMgr);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                Console.Write("Press return to exit.");
                Console.ReadLine();
            }
            finally
            {
                _logger.Debug("Final finally");
            }
        }

        private static void TakeExposureStack(CameraDeviceManager devMgr)
        {
            _logger.Info("Taking exposure stack");

            ICameraDevice myCam = GetMyCamera(devMgr);
            if (myCam == null)
            {
                _logger.Error($"My camera {MY_CAMERA} not found.");
                return;
            }


            WaitForNotBusy(myCam); // do nothing until ready
            myCam.LockCamera(); // mirror lock?
            myCam.WhiteBalance.Value = "Daylight";
            myCam.ExposureCompensation.Value = "-0.7";
            TakePhoto(myCam, 3);
            myCam.ExposureCompensation.Value = "+0.7";
            TakePhoto(myCam, 3);
            myCam.ExposureCompensation.Value = "0.0";
            TakePhoto(myCam, 3);

            myCam.UnLockCamera();// mirror lock?

        }

        private static void TakePhoto(CameraDeviceManager devMgr)
        {
            _logger.Info("Taking photo");
            ICameraDevice myCam = GetMyCamera(devMgr);
            if (myCam == null)
            {
                _logger.Error($"My camera {MY_CAMERA} not found.");
                return;
            }

            // myCam.IsoNumber.Value = "2000";
            // FYI: myCam.ExposureCompensation.Values of "0.1", "+0.1" appear to do nothing (+ is more light, - is less light)
            // Nikon WB values appear to be Auto,Daylight,Fluorescent,Incandescent,Flash,Cloudy,Shade,Kelvin,Custom
            //myCam.ExposureCompensation.Value = "+0.3";

            _logger.Info("----- Incandescent -----");
            WaitForNotBusy(myCam); // do nothing until ready
            myCam.WhiteBalance.Value = "Incandescent";
            TakePhoto(myCam);

            _logger.Info("----- Cloudy -----");
            for (int i = 0; i < 5; ++i)
            {
                WaitForNotBusy(myCam); // do nothing until ready
                myCam.WhiteBalance.Value = "Cloudy";
                TakePhoto(myCam);
            }
        }

        private static void WaitForNotBusy(ICameraDevice cam)
        {
            if (!cam.IsBusy)
                return;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            _logger.Debug("Busy...");

            while (cam.IsBusy)
            {
                System.Threading.Thread.Sleep(25);
            }
            sw.Stop();

            _logger.Info($"Waited for camera ~{sw.ElapsedMilliseconds} ms");
        }

        private static void TakePhoto(ICameraDevice cam, int tries = 3)
        {
            if (tries < 1)
            {
                _logger.Warn("TakePhoto tries must be >= 1 (set to 1)");
                tries = 1;
            }
            int attemptsRemaining = tries;
            while (attemptsRemaining > 0)
            {
                attemptsRemaining--;
                try
                {
                    cam.CapturePhoto();
                    return;
                }
                catch (Exception e)
                {
                    if (attemptsRemaining > 0)
                    {
                        _logger.Warn(e, "While taking photo - will retry");
                        WaitForNotBusy(cam);
                    }
                    else
                        _logger.Error(e, "While taking photo - failed");
                }
            }
        }

        private static void DownloadPhotos(CameraDeviceManager devMgr)
        {
            _logger.Info("Download photos");
            ICameraDevice myCam = GetMyCamera(devMgr);
            if (myCam == null)
            {
                _logger.Error($"My camera {MY_CAMERA} not found.");
                return;
            }
            if (myCam.IsBusy)
            {
                _logger.Warn("Camera is busy");
                return;
            }

            DownloadPhotos(myCam);
        }

        private static void DownloadPhotos(ICameraDevice myCam)
        {
            while (_capturedPhotos.Count > 0)
            {
                var cap = _capturedPhotos.Dequeue();
                WaitForNotBusy(myCam);
                _logger.Info($"   Download {cap.FileName} ({cap.Handle})");
                myCam.TransferFile(cap.Handle, Path.Combine(DOWNLOAD_PATH, cap.FileName));
            }
        }

        private static ICameraDevice GetMyCamera(CameraDeviceManager devMgr)
        {
            foreach (var dev in devMgr.ConnectedDevices)
            {
                if (dev.DeviceName == MY_CAMERA)
                    return dev;
            }
            return null;
        }

        private static void ListCameras(CameraDeviceManager devMgr)
        {
            _logger.Info("Connected Cameras");
            foreach (var dev in devMgr.ConnectedDevices)
            {
                _logger.Info($"{dev.DeviceName}");
                if (dev.DeviceName != MY_CAMERA)
                    continue;
                _logger.Info($"   IsBusy = {dev.IsBusy}");
                _logger.Info($"   Battery = {dev.Battery}%");
                _logger.Info($"   Whitebalance = {dev.WhiteBalance.Value}");
                _logger.Info($"   LiveView? = {dev.GetCapability(CapabilityEnum.LiveView)}");
                _logger.Info($"   LiveViewStream? = {dev.GetCapability(CapabilityEnum.LiveViewStream)}");
                _logger.Info($"   CanLockFocus? = {dev.GetCapability(CapabilityEnum.CanLockFocus)}");
                _logger.Info($"   CaptureNoAf? = {dev.GetCapability(CapabilityEnum.CaptureNoAf)}");
                _logger.Info($"   SimpleManualFocus? = {dev.GetCapability(CapabilityEnum.SimpleManualFocus)}");
                _logger.Info($"   Zoom? = {dev.GetCapability(CapabilityEnum.Zoom)}");
                foreach (var prop in dev.AdvancedProperties)
                {
                    _logger.Info($"   {prop.Name} ({prop.Code}) = {prop.Value}");
                }
            }
        }

        #region DeviceManager log message mapping
        private static void DM_LogError(CameraControl.Devices.Classes.LogEventArgs e)
        {
            _logger.Error(e.Exception, $"DM: {e.Message}");
        }

        private static void DM_LogDebug(CameraControl.Devices.Classes.LogEventArgs e)
        {
            _logger.Debug(e.Exception, $"DM: {e.Message}");
        }

        private static void DM_LogInfo(CameraControl.Devices.Classes.LogEventArgs e)
        {
            _logger.Info(e.Exception, $"DM: {e.Message}");
        }
        #endregion

        static Queue<CapturedPhoto> _capturedPhotos = new Queue<CapturedPhoto>();

        private static void DevMgr_PhotoCaptured(object sender, CameraControl.Devices.Classes.PhotoCapturedEventArgs eventArgs)
        {
            _logger.Info($"Photo Captured {eventArgs.FileName}, handle={eventArgs.Handle}");
            _capturedPhotos.Enqueue(new CapturedPhoto(eventArgs));

            // indicate we don't want it right now
            eventArgs.CameraDevice.IsBusy = false;
            eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);
        }

        private static void DevMgr_CameraDisconnected(ICameraDevice cameraDevice)
        {
            _logger.Info($"Disconnected: {cameraDevice.DeviceName}");
        }

        private static void DevMgr_CameraConnected(ICameraDevice cameraDevice)
        {
            _logger.Info($"Connected DeviceName={cameraDevice.DeviceName}; DisplayName={cameraDevice.DisplayName}");
            if (cameraDevice.DeviceName == MY_CAMERA)
            {
                cameraDevice.CaptureInSdRam = false; // forces camera to save in internal storage (and name the files)
            }
        }
    }
}
