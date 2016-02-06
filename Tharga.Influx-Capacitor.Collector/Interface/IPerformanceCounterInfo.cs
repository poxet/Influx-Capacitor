using System.Collections.Generic;
using System.Diagnostics;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IPerformanceCounterInfo
    {
        string Name { get; }
        PerformanceCounter PerformanceCounter { get; }
        string FieldName { get; }
        string Alias { get; }
        IEnumerable<ITag> Tags { get; }

        /// <summary>
        /// Gets the maximum value authorized for this counter.
        /// Useful if the counter is sometimes reporting more than a limit value.
        /// </summary>
        /// <seealso cref="ICounter.Max"/>
        /// <seealso href="https://support.microsoft.com/en-us/kb/310067"/>
        float? Max { get; }
    }
}