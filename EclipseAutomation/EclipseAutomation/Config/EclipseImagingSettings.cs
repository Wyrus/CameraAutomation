using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EclipseAutomation.Config
{
    public class EclipseImagingSettings : ConfigurationSection
    {
        /* TO EASILY ADJUST!

        Annular
            Iso = "200"
            ShutterStack = "1/2000;1/1000;1/500;1/250"
            FNumber = "8.0"
            Exposure = "0.0"

        SecondContact
            Iso = "200"
            ShutterStack = "1/400;1/200;1/100;"
            FNumber = "8.0"
            Exposure = "0.0"

        Totality
            Iso = "200"
            ShutterStack = "1/6400;1/3200;1/800;1/250;1/100;1/50;1/13;1/6"
            FNumber = "8.0"
            Exposure = "0.0"

        DefaultLocation
            Desc="Plano, TX; April 8, 2024"
            Date="4/8/2024"
            C1=""
            C2=""
            C3=""
            C4=""
         */

        private const string ECLIPSE_TIMES = "eclipseTimes";
        [ConfigurationProperty(ECLIPSE_TIMES)]
        public EclipseTimesConfig EclipseTimes
        {
            get => (EclipseTimesConfig)this[ECLIPSE_TIMES];
            set => this[ECLIPSE_TIMES] = value;
        }

        private const string TASK_PARAMETERS = "taskParameters";
        [ConfigurationProperty(TASK_PARAMETERS)]
        public EclipseTaskParameters TaskParameters
        {
            get => (EclipseTaskParameters)this[TASK_PARAMETERS];
            set => this[TASK_PARAMETERS] = value;
        }

        private const string ANNULAR_STACK = "annularStack";
        [ConfigurationProperty(ANNULAR_STACK)]
        public StackDetails AnnularStack
        {
            get => (StackDetails)this[ANNULAR_STACK];
            set => this[ANNULAR_STACK] = value;
        }

        private const string SECOND_CONTACT_STACK = "secondContactStack";
        [ConfigurationProperty(SECOND_CONTACT_STACK)]
        public StackDetails SecondContactStack
        {
            get => (StackDetails)this[SECOND_CONTACT_STACK];
            set => this[SECOND_CONTACT_STACK] = value;
        }

        private const string TOTALITY_STACK = "totalityStack";
        [ConfigurationProperty(TOTALITY_STACK)]
        public StackDetails TotalityStack
        {
            get => (StackDetails)this[TOTALITY_STACK];
            set => this[TOTALITY_STACK] = value;
        }
    }

    public class EclipseTimesConfig : ConfigurationElement
    {
        // EclipseTaskFactory methods

        private const string ECLIPSE_DATE = "eclipseDate";
        [ConfigurationProperty(ECLIPSE_DATE, IsRequired = true)]
        public DateTime EclipseDate
        {
            get => (DateTime)this[ECLIPSE_DATE];
            set => this[ECLIPSE_DATE] = value;
        }

        private const string C1_TIME = "c1";
        [ConfigurationProperty(C1_TIME, IsRequired = true)]
        [TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "23:59:59", ExcludeRange = false)]
        public TimeSpan C1
        {
            get => (TimeSpan)this[C1_TIME];
            set => this[C1_TIME] = value;
        }

        private const string C2_TIME = "c2";
        [ConfigurationProperty(C2_TIME, IsRequired = true)]
        [TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "23:59:59", ExcludeRange = false)]
        public TimeSpan C2
        {
            get => (TimeSpan)this[C2_TIME];
            set => this[C2_TIME] = value;
        }

        private const string C3_TIME = "c3";
        [ConfigurationProperty(C3_TIME, IsRequired = true)]
        [TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "23:59:59", ExcludeRange = false)]
        public TimeSpan C3
        {
            get => (TimeSpan)this[C3_TIME];
            set => this[C3_TIME] = value;
        }

        private const string C4_TIME = "c4";
        [ConfigurationProperty(C4_TIME, IsRequired = true)]
        [TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "23:59:59", ExcludeRange = false)]
        public TimeSpan C4
        {
            get => (TimeSpan)this[C4_TIME];
            set => this[C4_TIME] = value;
        }
    }

    public class EclipseTaskParameters : ConfigurationElement
    {
        // EclipseTaskFactory constructor

        private const string ANNULAR_OFFSET = "annularOffset";
        [ConfigurationProperty(ANNULAR_OFFSET, IsRequired = true)]
        [TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "2:00:00", ExcludeRange = false)]
        public TimeSpan AnnularOffset
        {
            get => (TimeSpan)this[ANNULAR_OFFSET];
            set => this[ANNULAR_OFFSET] = value;
        }

        private const string ANNULAR_COUNT = "annularCount";
        [ConfigurationProperty(ANNULAR_COUNT, IsRequired = true)]
        public int AnnularCount
        {
            get => (int)this[ANNULAR_COUNT];
            set => this[ANNULAR_COUNT] = value;
        }

        private const string SECOND_CONTACT_WINDOW = "secondContactWindow";
        [ConfigurationProperty(SECOND_CONTACT_WINDOW, IsRequired = true)]
        [TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "0:01:00", ExcludeRange = false)]
        public TimeSpan SecondContactWindow
        {
            get => (TimeSpan)this[SECOND_CONTACT_WINDOW];
            set => this[SECOND_CONTACT_WINDOW] = value;
        }

        private const string TOTALITY_INTERVAL = "totalityInterval";
        [ConfigurationProperty(TOTALITY_INTERVAL, IsRequired = true)]
        [TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "0:03:00", ExcludeRange = false)]
        public TimeSpan TotalityInterval
        {
            get => (TimeSpan)this[TOTALITY_INTERVAL];
            set => this[TOTALITY_INTERVAL] = value;
        }

        protected override void PostDeserialize()
        {
            // Can't use IntegerValidator because it will cause the c-tor to pop when used in test code
            // NOTE: If using in test code like ...new EclipseTaskParameters(), this validation will NOT fire.
            if (AnnularCount < 2 || AnnularCount > 15)
                throw new ArgumentOutOfRangeException(nameof(AnnularCount),"Must be 2 <= AnnularCount <= 15");
        }

    }

    public class StackDetails : ConfigurationElement
    {
        // EclipseTaskFactory internals
        private const string ISO_NUMBER = "isoNumber";
        [ConfigurationProperty(ISO_NUMBER, IsRequired = true)]
        public string IsoNumber
        {
            get => (String)this[ISO_NUMBER];
            set => this[ISO_NUMBER] = value;
        }

        private const string F_NUMBER = "fNumber";
        [ConfigurationProperty(F_NUMBER, IsRequired = true)]
        public string FNumber
        {
            get => (String)this[F_NUMBER];
            set => this[F_NUMBER] = value;
        }

        private const string EXPOSURE_COMPENSATION = "exposureCompensation";
        [ConfigurationProperty(EXPOSURE_COMPENSATION, IsRequired = true)]
        public string ExposureCompensation
        {
            get => (String)this[EXPOSURE_COMPENSATION];
            set => this[EXPOSURE_COMPENSATION] = value;
        }


        private const string SHUTTER_SPEEDS = "shutterSpeeds";
        [ConfigurationProperty(SHUTTER_SPEEDS, IsRequired = true)]
        public string ShutterSpeeds
        {
            get => (String)this[SHUTTER_SPEEDS];
            set => this[SHUTTER_SPEEDS] = value;
        }
    }

}
