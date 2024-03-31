using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EclipseAutomation.Tasks
{
    [TestClass]
    public class EclipseTaskFactoryTests
    {
        private static Logger _logger = LogManager.GetLogger(nameof(EclipseTaskFactoryTests));

        // Based on EclipseTaskFactory.AnnularStack() and TotalityStack()
        private const int ANNULAR_PHOTO_COUNT = 4;
        private const int TOTALITY_PHOTO_COUNT = 8;

        [TestMethod]
        public void EyeballCheck()
        {
            TimeSpan c1 = new TimeSpan(6, 0, 0);
            TimeSpan c2 = new TimeSpan(7, 0, 0);
            TimeSpan c3 = new TimeSpan(7, 10, 0);
            TimeSpan c4 = new TimeSpan(8, 10, 0);
            TimeSpan lastAnnularOffset = TimeSpan.FromMinutes(5);
            TimeSpan annularInterval = TimeSpan.FromMinutes(10);
            TimeSpan totalityLeadOffset = TimeSpan.FromSeconds(30);
            TimeSpan totalityInterval = TimeSpan.FromSeconds(60);

            //var taskFactory = new EclipseTaskFactory(c1, c2, c3, c4, lastAnnularOffset, annularInterval, totalityLeadOffset, totalityInterval);
            //var results = taskFactory.CreateScheduledTasks(DateTime.Now);
            //Dump(results);
        }

        private void Dump(List<ScheduledTask> results)
        {
            foreach(var task in results)
            {
                string flag = task.WarningFlag ? "*" : " ";
                var photoTask = task as StackedPhotoTask;
                if (photoTask != null)
                    _logger.Info($"{photoTask.ExecuteAt}{flag} {photoTask.Description} {photoTask.Settings.Count()} photos");
                else
                    _logger.Info($"{task.ExecuteAt}{flag} {task.Description}");
            }
        }
    }
}
