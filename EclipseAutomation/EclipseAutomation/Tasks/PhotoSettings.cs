using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace EclipseAutomation.Tasks
{
    public class PhotoSettings
    {
        private static Logger _logger = LogManager.GetLogger(nameof(PhotoSettings));

        // These come experimentally but looking at ICameraHandler.TakePhotoAverage and AwaitCaptureAverage

        // On my D7000
        // Manual focus saves time
        // Setting the camera in full manual mode saves time (after all, we're driving it)
        // When recording RAW, Compressed is a bit faster than Lossless Compressed
        readonly TimeSpan AVG_PHOTO_TIME = TimeSpan.FromSeconds(1.015);
        // Average time to capture to in camera SD
        readonly TimeSpan AVG_AWAIT_CAPTURE_TIME = TimeSpan.FromSeconds(1.170);
        // Average time to download to PC (without SD capture)
        readonly TimeSpan AVG_AWAIT_DOWNLOAD_TIME = TimeSpan.FromSeconds(0.79);

        public string IsoNumber { get; set; } = "200";

        private string _shutterSpeed = "1/1600";
        public string ShutterSpeed
        {
            get => _shutterSpeed;
            set
            {
                _shutterSpeed = value;
                ComputeShutterDuration();
            }
        }

        public string FNumber { get; set; } = "8.0";
        public string ExposureCompensation { get; set; } = "0.0";

        public PhotoSettings() { }

        public PhotoSettings(PhotoSettings source)
        {
            IsoNumber = source.IsoNumber;
            ShutterSpeed = source.ShutterSpeed;
            FNumber = source.FNumber;
            ExposureCompensation = source.ExposureCompensation;
        }

        private void ComputeShutterDuration()
        {
            if (String.IsNullOrEmpty(ShutterSpeed))
            {
                ShutterDuration = TimeSpan.Zero;
                return;
            }
            try
            {
                var trimmed = ShutterSpeed.TrimEnd('s');
                var parts = trimmed.Split('/');
                if (parts.Length == 1)
                {
                    double n = double.Parse(parts[0]);
                    ShutterDuration = TimeSpan.FromSeconds(1 / n);
                }
                else if (parts.Length == 2)
                {
                    double n = double.Parse(parts[0]);
                    double d = double.Parse(parts[1]);
                    ShutterDuration = TimeSpan.FromSeconds(n / d);
                }
                else
                {
                    _logger.Error($"Cannot parse ShutterSpeed of {ShutterSpeed}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "While computing shutter duration");
            }
        }

        public TimeSpan ShutterDuration { get; private set; } = TimeSpan.FromSeconds(1 / 500);

        /// <summary>
        /// Expected time to complete photo capture
        /// </summary>
        public TimeSpan ExpectedCaptureDuration => AVG_PHOTO_TIME + AVG_AWAIT_CAPTURE_TIME + ShutterDuration;
        public TimeSpan ExpectedDownloadDuration => AVG_PHOTO_TIME + AVG_AWAIT_DOWNLOAD_TIME + ShutterDuration;
    }
}
