using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using EclipseAutomation.Camera;
using EclipseAutomation.Utility;
using NLog;

namespace EclipseAutomation.Tasks
{
    internal class TestScheduledTask : ScheduledTask
    {
        private static Logger _logger = LogManager.GetLogger(nameof(TestScheduledTask));
        protected override Logger Logger => _logger;

        public TimeSpan ExecutionDuration { get; }

        public TestScheduledTask(String name, String description, DateTime executeAt, DateTime? executeUtil, bool warningFlag, TimeSpan executionDuration)
            : base(name, description, executeAt, executeUtil, warningFlag, false)
        {
            ExecutionDuration = executionDuration;
        }

        public override void ExecuteTask(ICameraHandler cameraHandler)
        {
            using (new TimedActivity(nameof(TestScheduledTask), _logger))
            {
                System.Threading.Thread.Sleep((int)ExecutionDuration.TotalMilliseconds);
            }
        }
    }
}
