using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IPerformanceCounterInfo
    {
        /// <summary>
        /// Gets the name of this informational counter.
        /// Can be <see cref="CounterName"/> or <see cref="InstanceName"/>, depending if <see cref="CounterName"/> value is "*".
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating if this informational counter has a real counter or not.
        /// </summary>
        bool HasPerformanceCounter { get; }

        /// <summary>
        /// Gets the category this performance counter is member of.
        /// </summary>
        string CategoryName { get; }

        /// <summary>
        /// Gets the performance counter name.
        /// </summary>
        string CounterName { get; }

        /// <summary>
        /// Gets the instance name for this counter.
        /// </summary>
        string InstanceName { get; }

        /// <summary>
        /// Gets the machine name for this counter.
        /// </summary>
        string MachineName { get; }

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

        /// <summary>
        /// Obtains the next counter value and returns it.
        /// </summary>
        float NextValue();
    }
}