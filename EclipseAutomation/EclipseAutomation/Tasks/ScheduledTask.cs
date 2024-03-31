using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EclipseAutomation.Camera;
using NLog;

namespace EclipseAutomation.Tasks
{
    public class ScheduledTask
    {
        private static Logger _logger = LogManager.GetLogger(nameof(ScheduledTask));
        protected virtual Logger Logger => _logger;

        /// <summary>
        /// Task name
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// When should this task fire?
        /// </summary>
        public DateTime ExecuteAt { get; protected set; }
        /// <summary>
        /// Continuously repeat task until this time (defaults to ExecuteAt). At the time, the task will no longer execute.
        /// </summary>
        public DateTime ExecuteUntil { get; protected set; }
        /// <summary>
        /// Text to show while waiting for this task
        /// </summary>
        public String Description { get; protected set; }
        /// <summary>
        /// Indicates an important message, you might want to highlight it somehow
        /// </summary>
        public bool WarningFlag { get; protected set; }

        /// <summary>
        /// Task uses camera. (So camera must be idle when task starts)
        /// </summary>
        public bool UsesCamera { get; protected set; }

        public virtual DateTime EstimatedCompletion => ExecuteUntil;

        public ScheduledTask(String name, String description, DateTime executeAt, DateTime? executeUntil, bool warningFlag, bool usesCamera)
        {
            Name = name;
            Description = description;
            ExecuteAt = executeAt;
            ExecuteUntil = executeUntil ?? ExecuteAt;
            WarningFlag = warningFlag;
            UsesCamera = usesCamera;

            if (ExecuteUntil < ExecuteAt)
                throw new ArgumentOutOfRangeException("executeUntil must be >= executeAt", nameof(executeUntil));
        }

        public virtual void ExecuteTask(ICameraHandler cameraHandler)
        {
            _logger.Info($"ScheduledTask: {Name}, {ExecuteAt:HH:mm:ss} -> {ExecuteUntil:HH:mm:ss}");
        }

        public override string ToString()
        {
            if (ExecuteUntil == ExecuteAt)
                return $"{Name} {ExecuteAt:yyyy-MM-dd HH:mm:ss}";
            else
                return $"{Name} {ExecuteAt:yyyy-MM-dd HH:mm:ss} -> {ExecuteUntil:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
