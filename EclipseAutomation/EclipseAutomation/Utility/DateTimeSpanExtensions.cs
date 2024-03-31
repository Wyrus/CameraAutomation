using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord;

namespace EclipseAutomation.Utility
{
    public static class DateTimeSpanExtensions
    {
        public static TimeSpan Multiply(this TimeSpan timeSpan, double factor) => TimeSpan.FromSeconds(timeSpan.TotalSeconds * factor);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="denominator"></param>
        /// <returns></returns>
        /// <remarks>Divide by zero will take care of itself</remarks>
        public static TimeSpan DivideBy(this TimeSpan timeSpan, double denominator) => TimeSpan.FromSeconds(timeSpan.TotalSeconds / denominator);

        public static System.TimeSpan Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, System.TimeSpan> selector)
        {
            if(!source.Any())
                return TimeSpan.Zero;

            return source.Select(selector).Aggregate((x, y) => x + y);
        }
    }
}
