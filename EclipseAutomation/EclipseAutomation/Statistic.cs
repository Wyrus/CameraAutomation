using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EclipseAutomation
{
    public class Statistic
    {
        int _samples = 0;
        double _sum = 0.0;
        double _max = double.MinValue;
        double _min = double.MaxValue;

        public Statistic() { }

        public void AddSample(double sample)
        {
            _samples++;
            _sum += sample;
            if (sample > _max)
                _max = sample;
            if (sample < _min)
                _min = sample;
        }

        public double Average
        {
            get
            {
                if (_samples == 0)
                    return 0.0;
                return _sum / _samples;
            }
        }

        public double Max => _samples > 0 ? _max : 0;
        public double Min => _samples > 0 ? _min : 0;
    }
}
