using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EclipseAutomation.Tasks
{
    [TestClass]
    public class TaskQueueTests
    {
        private TestScheduledTask NewTask(String name, DateTime at, TimeSpan? untilOffset = null)
        {
            TimeSpan offset = untilOffset ?? TimeSpan.Zero;
            return new TestScheduledTask(name, name, at, at + offset, false, TimeSpan.Zero);
        }

        [TestMethod]
        public void StripPastTests_InConstructor()
        {
            DateTime refTime = new DateTime(2001, 1, 1, 12, 0, 0);
            var t1 = NewTask("t1", refTime.AddMinutes(-60));
            var t2 = NewTask("t2", refTime.AddMinutes(-2));
            var t3 = NewTask("t3", refTime.AddMinutes(-1), TimeSpan.FromMinutes(2));
            var t4 = NewTask("t4", refTime.AddMinutes(5));

            var queue = new TaskQueue(new[] { t1, t2, t3, t4 }, refTime);

            Assert.AreEqual(2, queue.Count, "wrong count");
            Assert.AreEqual(t3.Name, queue.NextTask(refTime).Name, "wrong first item");
        }

        [TestMethod]
        public void InstantTasks()
        {
            DateTime refTime = new DateTime(2001, 1, 1, 12, 0, 0);
            var t1 = NewTask("t1", refTime.AddMinutes(5));
            var t2 = NewTask("t2", refTime.AddMinutes(10));
            var t3 = NewTask("t3", refTime.AddMinutes(15));

            var queue = new TaskQueue(new[] { t1, t2, t3 }, refTime);
            Assert.AreEqual(3, queue.Count, "wrong count");

            var task = queue.NextTask(refTime);
            Assert.AreEqual(t1.Name, task.Name);

            // still future
            task = queue.NextTask(refTime.AddSeconds(5 * 60 - 1));
            Assert.AreEqual(t1.Name, task.Name);

            // the guaranteed at or after ExecuteAt
            task = queue.NextTask(refTime.AddSeconds(5 * 60));
            Assert.AreEqual(t1.Name, task.Name);
            Assert.AreEqual(2, queue.Count, "t1 should be gone");

            // even outside the window, t2 should always be returned once
            task = queue.NextTask(refTime.AddSeconds(11 * 60));
            Assert.AreEqual(t2.Name, task.Name);

            // should get it at the exact time
            task = queue.NextTask(refTime.AddSeconds(15 * 60));
            Assert.AreEqual(t3.Name, task.Name);

            Assert.AreEqual(0, queue.Count, "All should be gone");
        }

        [TestMethod]
        public void InstantAndDurationTasks()
        {
            DateTime refTime = new DateTime(2001, 1, 1, 12, 0, 0);
            var t1 = NewTask("t1", refTime.AddMinutes(5));
            var t2 = NewTask("t2", refTime.AddMinutes(10), TimeSpan.FromMinutes(1));
            var t3 = NewTask("t3", refTime.AddMinutes(15), TimeSpan.FromMinutes(1));
            var t4 = NewTask("t4", refTime.AddMinutes(20), TimeSpan.FromMinutes(1));

            var queue = new TaskQueue(new[] { t1, t2, t3, t4 }, refTime);
            Assert.AreEqual(4, queue.Count, "wrong count");

            // slightly in the future
            var task = queue.NextTask(refTime.AddSeconds(5 * 60 + 1));
            Assert.AreEqual(t1.Name, task.Name);
            Assert.AreEqual(3, queue.Count, "t1 should be gone");

            // before t2
            task = queue.NextTask(refTime.AddSeconds(10 * 60 - 1));
            Assert.AreEqual(t2.Name, task.Name);

            // at t2
            task = queue.NextTask(refTime.AddSeconds(10 * 60));
            Assert.AreEqual(t2.Name, task.Name);

            task = queue.NextTask(refTime.AddSeconds(10 * 60 + 1));
            Assert.AreEqual(t2.Name, task.Name);

            task = queue.NextTask(refTime.AddSeconds(10 * 60 + 59));
            Assert.AreEqual(t2.Name, task.Name);

            // t2 has been returned when active, so it should have been removed
            task = queue.NextTask(refTime.AddSeconds(11 * 60));
            Assert.AreEqual(t3.Name, task.Name);

            // before t3 is active
            task = queue.NextTask(refTime.AddSeconds(13 * 60));
            Assert.AreEqual(t3.Name, task.Name);

            // t3 not returned yet, but run outside of window
            task = queue.NextTask(refTime.AddSeconds(17 * 60));
            Assert.AreEqual(t3.Name, task.Name);
            Assert.AreEqual(1, queue.Count, "t3 should be gone");

            // t4 once in window
            task = queue.NextTask(refTime.AddSeconds(20 * 60 + 30));
            Assert.AreEqual(t4.Name, task.Name);

            // t4 is no longer in the window
            task = queue.NextTask(refTime.AddSeconds(21 * 60));
            Assert.IsNull(task);
        }
    }
}
