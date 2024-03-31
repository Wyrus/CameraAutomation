using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EclipseAutomation.Config;

namespace EclipseAutomation.Tasks
{
    public class EclipseTimes
    {
        public DateTime C1 { get; }
        public DateTime C2 { get; }
        public DateTime C3 { get; }
        public DateTime C4 { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dateOfEclipse">The date of the eclipse (time portion is ignored)</param>
        /// <param name="c1Offset">Time of first contact relative to midnight on the day of the eclipse. (Moon starts occluding the sun)</param>
        /// <param name="c2Offset">Time of second contact relative to midnight on the day of the eclipse. (Totality begins)</param>
        /// <param name="c3Offset">Time of third contact relative to midnight on the day of the eclipse. (Totality ends)</param>
        /// <param name="c4Offset">Time of last contact relative to midnight on the day of the eclipse. (Moon ceases occluding the sun)</param>
        public EclipseTimes(
            DateTime dateOfEclipse,
            TimeSpan c1Offset,
            TimeSpan c2Offset,
            TimeSpan c3Offset,
            TimeSpan c4Offset)
        {
            DateTime midnight = dateOfEclipse.Date;

            C1 = midnight + c1Offset;
            C2 = midnight + c2Offset;
            C3 = midnight + c3Offset;
            C4 = midnight + c4Offset;
        }

        public EclipseTimes(EclipseTimesConfig config):
            this(config.EclipseDate, config.C1, config.C2, config.C3, config.C4)
        { }

        public static EclipseTimes GetApril2024FromPlano()
        {
            return new EclipseTimes(
                new DateTime(2024, 04, 08),
                new TimeSpan(12, 24, 02),
                new TimeSpan(13, 41, 35),
                new TimeSpan(13, 45, 03),
                new TimeSpan(15, 03, 13)
                );
        }
    }
}
