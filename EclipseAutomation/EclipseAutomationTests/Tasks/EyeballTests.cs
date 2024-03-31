using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;

namespace EclipseAutomation.Tasks
{
    [TestClass]
    public class EyeballTests
    {
        private static Logger _logger = LogManager.GetLogger(nameof(EyeballTests));

        [TestMethod]
        public void ReviewTheTaskList()
        {
            var factory = EclipseTaskFactory.GetDefaultEclipseTaskFactory();

            var tasks = factory.CreateScheduledTasks();
            DateTime lastCompletion = tasks.First().ExecuteAt.AddSeconds(-30);

            foreach(var task in tasks)
            {
                var gap = task.ExecuteAt - lastCompletion;
                _logger.Info($"GAP of {gap:mm\\:ss\\.fff}");

                bool tooSoon = gap.TotalSeconds < 1.0;
                if(tooSoon)
                    _logger.Error($"{task} - {task.Description}");
                else
                    _logger.Info($"{task} - {task.Description}");

                Assert.IsFalse(tooSoon, $"This task starts {task.ExecuteAt:HH:mm:ss} too close to the previous' expected completion: {lastCompletion:HH:mm:ss}");

                lastCompletion = task.EstimatedCompletion;
            }
        }
    }
}
