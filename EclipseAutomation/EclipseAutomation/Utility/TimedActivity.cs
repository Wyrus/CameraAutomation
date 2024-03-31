using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace EclipseAutomation.Utility
{
    public class TimedActivity : IDisposable
    {
        String _activityName;
        Logger _logger;
        Stopwatch _stopwatch;
        Statistic _statistic;

        public TimedActivity(String activityName, Logger logger)
            : this(activityName, logger, null)
        {}

        public TimedActivity(String activityName, Statistic stats)
            : this(activityName, null, stats)
        { }

        public TimedActivity(String activityName, Logger logger, Statistic stats)
        {
            _activityName = activityName;
            _logger = logger;
            _statistic = stats;

            _stopwatch = Stopwatch.StartNew();
            _logger?.Debug($"BEGIN {_activityName}");
        }

        public void Dispose()
        {
            _stopwatch.Stop();

            _statistic?.AddSample(_stopwatch.Elapsed.TotalSeconds);

            _logger?.Debug($"END {_activityName} after {_stopwatch.Elapsed.TotalSeconds:0.000} seconds");
        }
    }
}
