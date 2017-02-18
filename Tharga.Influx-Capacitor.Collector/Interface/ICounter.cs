using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Entities;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ICounter
    {
        string Name { get; }
        string MachineName { get; }
        string CategoryName { get; }
        Naming CounterName { get; }
        Naming InstanceName { get; }
        string FieldName { get; }
        IEnumerable<ITag> Tags { get; }

        /// <summary>
        /// Gets the maximum value authorized for this counter.
        /// Useful if the counter is sometimes reporting more than a limit value.
        /// </summary>
        /// <seealso cref="IPerformanceCounterInfo.Max"/>
        /// <seealso href="https://support.microsoft.com/en-us/kb/310067"/>
        float? Max { get; }
        float? Min { get; }

        float? Reverse { get; }
    }
}