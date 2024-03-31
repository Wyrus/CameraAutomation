using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Diagnostics;
using WebSocketSharp;

namespace LiveViewTests
{
    public partial class MainForm : Form
    {
        private static string MY_CAMERA = "D7000";
        private static string DOWNLOAD_PATH = @"C:\temp\LiveViewTests";

        ICameraDevice _captureCamera = null;
        Timer _liveViewTimer = null;
        bool _timerBusy = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _liveViewTimer = new Timer();
            _liveViewTimer.Enabled = false;
            _liveViewTimer.Interval = 1000 / 20; // 20/sec
            _liveViewTimer.Tick += _liveViewTimer_Tick;


            Log.LogInfo += DM_LogInfo;
            Log.LogDebug += DM_LogDebug;
            Log.LogError += DM_LogError;

            CameraDeviceManager devMgr = new CameraDeviceManager();
            devMgr.StartInNewThread = true;
            devMgr.DetectWebcams = false;
            devMgr.UseExperimentalDrivers = false;
            devMgr.LoadWiaDevices = false;
            devMgr.CameraConnected += DevMgr_CameraConnected;
            devMgr.CameraDisconnected += DevMgr_CameraDisconnected;
            devMgr.PhotoCaptured += DevMgr_PhotoCaptured;

            // start looking for cameras
            devMgr.ConnectToCamera();
        }

        private void btnTakePhoto_Click(object sender, EventArgs e)
        {
            if (_captureCamera == null)
                return;

            WaitForNotBusy();
            _captureCamera.WhiteBalance.Value = "Auto";
            _captureCamera.ExposureCompensation.Value = "0.0";
            _captureCamera.CapturePhoto();
        }

        private void WaitForNotBusy(int timeoutMS = 1000)
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
        }

        private void DevMgr_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
        {
            System.Diagnostics.Debug.WriteLine($"DevMgr_PhotoCaptured ({eventArgs.FileName})");

            // Ensure DOWNLOAD_PATH exists or else TransferFile will fail!
            string targetFilepath = Path.Combine(DOWNLOAD_PATH, eventArgs.FileName);
            System.Diagnostics.Debug.WriteLine($"DevMgr_PhotoCaptured downloading to {targetFilepath}");
            eventArgs.CameraDevice.TransferFile(eventArgs.Handle, targetFilepath);
            System.Diagnostics.Debug.WriteLine($"DevMgr_PhotoCaptured after Transfer before release");
            eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);

            System.Diagnostics.Debug.WriteLine($"Photo saved to {targetFilepath}");
        }

        private void _liveViewTimer_Tick(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("_liveViewTimer_Tick ENTER");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                _timerBusy = true;

                var lvImage = _captureCamera.GetLiveViewImage();
                if (lvImage?.ImageData == null)
                {
                    System.Diagnostics.Debug.WriteLine("_liveViewTimer_Tick No Image");
                    return;
                }
                System.Diagnostics.Debug.WriteLine("_liveViewTimer_Tick LiveViewData");


                MemoryStream stream = new MemoryStream(
                    lvImage.ImageData,
                    lvImage.ImageDataPosition,
                    lvImage.ImageData.Length - lvImage.ImageDataPosition);

                using (var res = new Bitmap(stream))
                {
                    System.Diagnostics.Debug.WriteLine("_liveViewTimer_Tick Got Bitmap from stream");

                    //res.Save("C:\\temp\\liveviewimage.jpg", ImageFormat.Jpeg);

                    Image clone = new Bitmap(res.Width, res.Height, PixelFormat.Format32bppArgb);
                    using (var copy = Graphics.FromImage(clone))
                    {
                        copy.DrawImage(res, 0, 0);
                    }

                    lvDisplay.Invoke(
                        (Action<PictureBox, Bitmap>)(
                        (ctrl, img) => { ctrl.Image = img; }),
                        lvDisplay, clone);
                    //pnlLiveView.Invoke(
                    //    (Action<Panel, Bitmap>)(
                    //    (pnl, img) => { pnl.BackgroundImage = img; }),
                    //    pnlLiveView, clone);

                    //var writeableBitmap =
                    //    BitmapFactory.ConvertToPbgra32Format(
                    //        BitmapSourceConvert.ToBitmapSource(res));
                    //writeableBitmap.Freeze();
                    //Bitmap = writeableBitmap;
                    //IsMovieRecording = LiveViewData.MovieIsRecording;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}({ex.GetType()})");
                System.Diagnostics.Debug.WriteLine($"{ex.StackTrace}");
            }
            finally
            {
                sw.Stop();
                System.Diagnostics.Debug.WriteLine($"_liveViewTimer_Tick elapsed {sw.Elapsed.TotalMilliseconds} ms");
                _timerBusy = false;
            }
        }

        private void DevMgr_CameraDisconnected(ICameraDevice cameraDevice)
        {
            if (cameraDevice.DeviceName == MY_CAMERA)
            {
                _liveViewTimer.Enabled = false;
                _captureCamera = null;
                SetMode(LiveViewMode.NoCamera);
            }
        }

        private void DevMgr_CameraConnected(ICameraDevice cameraDevice)
        {
            if (cameraDevice.DeviceName == MY_CAMERA)
            {
                _captureCamera = cameraDevice;
                _captureCamera.CaptureInSdRam = false; // Not required and won't interfere with photos
                SetMode(LiveViewMode.Ready);
            }
        }

        private void DM_LogError(LogEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"DM Err: {e.Message}");
        }

        private void DM_LogDebug(LogEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"DM Dbg: {e.Message}");
        }

        private void DM_LogInfo(LogEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"DM Info: {e.Message}");
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_captureCamera == null)
                return;

            WaitForNotBusy();
            _captureCamera.WhiteBalance.Value = "Auto";
            _captureCamera.ExposureCompensation.Value = "0.0";

            _captureCamera.StartLiveView();
            _liveViewTimer.Enabled = true;
            SetMode(LiveViewMode.Running);
            System.Diagnostics.Debug.WriteLine("btnStart_Click complete");
        }

        public enum LiveViewMode { NoCamera, Ready, Running };

        public void SetMode(LiveViewMode mode)
        {
            // very often fires from another thread
            this.Invoke(new Action(() =>
            {
                switch (mode)
                {
                    case LiveViewMode.NoCamera:
                        btnStart.Enabled = false;
                        btnStop.Enabled = false;
                        btnTakePhoto.Enabled = false;
                        return;
                    case LiveViewMode.Ready:
                        btnStart.Enabled = true;
                        btnStop.Enabled = false;
                        btnTakePhoto.Enabled = true;
                        return;
                    case LiveViewMode.Running:
                        btnStart.Enabled = false;
                        btnStop.Enabled = true;
                        btnTakePhoto.Enabled = false;
                        return;

                }
            }));
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_captureCamera == null)
                return;

            _captureCamera.StopLiveView();
            _liveViewTimer.Enabled = false;
            lvDisplay.Image = null;
            SetMode(LiveViewMode.Ready);
        }
    }
}
