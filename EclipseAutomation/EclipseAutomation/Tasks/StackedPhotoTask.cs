using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EclipseAutomation.Camera;
using EclipseAutomation.Utility;
using NLog;

namespace EclipseAutomation.Tasks
{
    public class StackedPhotoTask : ScheduledTask
    {
        private static Logger _logger = LogManager.GetLogger(nameof(StackedPhotoTask));
        protected override Logger Logger => _logger;

        public IEnumerable<PhotoSettings> Settings { get; }

        public bool SaveToPCNotCameraSD { get; }

        public override DateTime EstimatedCompletion
        {
            get
            {
                if (SaveToPCNotCameraSD)
                    return ExecuteUntil + Settings.Sum(s => s.ExpectedDownloadDuration);
                else
                    return ExecuteUntil + Settings.Sum(s => s.ExpectedCaptureDuration);
            }
        }

        public StackedPhotoTask(String name, String description, DateTime executeAt, DateTime? executeUtil, bool warningFlag, IEnumerable<PhotoSettings> settings, bool saveToPCNotCameraSD)
            : base(name, description, executeAt, executeUtil, warningFlag, true)
        {
            Settings = settings;
            SaveToPCNotCameraSD = saveToPCNotCameraSD;
        }

        public override void ExecuteTask(ICameraHandler cameraHandler)
        {
            using (new TimedActivity($"StackedPhotoTask: {Name}, {ExecuteAt:HH:mm:ss} -> {ExecuteUntil:HH:mm:ss}", _logger))
            {
                cameraHandler.TakePhotos(Settings, SaveToPCNotCameraSD);
            }
            _logger.Info($"Estimated duration was {Settings.Sum(s => s.ExpectedCaptureDuration).TotalSeconds:0.000} sec");
        }

        public override string ToString()
        {
            String taskMsg = base.ToString();
            return $"{taskMsg} (Estimated completion {EstimatedCompletion:HH:mm:ss})";
        }

    }
}
