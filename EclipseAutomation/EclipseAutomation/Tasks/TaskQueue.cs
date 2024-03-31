using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EclipseAutomation.Tasks
{
    public class TaskQueue
    {
        Queue<ScheduledTask> _tasks;

        public TaskQueue(IEnumerable<ScheduledTask> tasks, DateTime? currentTime = null)
        {
            DateTime refTime = currentTime ?? DateTime.Now;
            tasks = tasks.OrderBy(t => t.ExecuteAt);
            _tasks = new Queue<ScheduledTask>(tasks);

            // strip past tasks
            while (_tasks.Count > 0)
            {
                var task = _tasks.Peek();
                // if there's still time to run the top task, we're done.
                if (task.ExecuteUntil > refTime)
                    break;
                _ = _tasks.Dequeue();
            }
        }

        public bool QueueEmpty => Count == 0;

        public int Count => _tasks.Count;

        public IEnumerable<ScheduledTask> AllTasks => _tasks.ToList();

        private ScheduledTask _lastReturnedActiveTask = null;

        /// <summary>
        /// Returns the top task until it's time has occurred
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns>
        /// The next scheduled task. Each task is guaranteed to be returned at least once at or after the ExecuteAt time
        /// </returns>
        public ScheduledTask NextTask(DateTime? currentTime = null)
        {
            if (Count == 0)
                return null;

            DateTime refTime = currentTime ?? DateTime.Now;

            while (!QueueEmpty)
            {
                // get the top task
                var task = _tasks.Peek();

                // task starts in the future, no change
                if (task.ExecuteAt > refTime)
                    return task;

                // Here, we know that task.ExecuteAt <= refTime

                if (task.ExecuteAt == task.ExecuteUntil)
                {
                    // single execution task
                    // dequeue and return
                    return _tasks.Dequeue();
                }
                else
                {
                    // duration/multiple execution task
                    // expires in the future
                    if(task.ExecuteUntil > refTime)
                    {
                        // keep track that it's been returned at least once
                        _lastReturnedActiveTask = task;
                        return task;
                    }

                    // Here, we know that task.ExecuteUntil <= refTime
                    // if we haven't returned it, do so
                    if(task != _lastReturnedActiveTask)
                    {
                        // we haven't returned it, but it has expired
                        return _tasks.Dequeue();
                    }
                    // we have returned it, so discard and move on
                    _ = _tasks.Dequeue();
                }
            }
            return null;
        }
    }
}
