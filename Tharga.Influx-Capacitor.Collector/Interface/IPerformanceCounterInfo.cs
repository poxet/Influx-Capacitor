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
        /// Gets the system performance counter associated with this informations.
        /// <c>null</c> if the counter does not exists or is not available.
        /// </summary>

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
        /// Gets the read instance name for this counter, based on system instance name (without filtering).
        /// </summary>
        string InstanceName { get; }

        /// <summary>
        /// Gets the name to use as a field for this counter. Can be the value of <see cref="CounterName"/> or <see cref="InstanceName"/> with an eventual filter applied.
        /// </summary>
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