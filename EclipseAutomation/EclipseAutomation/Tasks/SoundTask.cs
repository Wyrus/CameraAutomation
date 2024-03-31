using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using EclipseAutomation.Camera;
using EclipseAutomation.Properties;
using EclipseAutomation.Utility;
using NLog;

namespace EclipseAutomation.Tasks
{
    public class SoundTask : ScheduledTask
    {
        private static Logger _logger = LogManager.GetLogger(nameof(SoundTask));
        protected override Logger Logger => _logger;

        public SoundTask(String name, String description, DateTime executeAt, DateTime? executeUtil, bool warningFlag)
            : base(name, description, executeAt, executeUtil, warningFlag, false)
        {
        }

        public override void ExecuteTask(ICameraHandler cameraHandler)
        {
            SystemSounds.Exclamation.Play();
        }
    }
}
