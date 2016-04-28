using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Handlers;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ICounterGroup
    {
        string Name { get; }
        string Provider { get; }
        int SecondsInterval { get; }
        int RefreshInstanceInterval { get; }
        IEnumerable<ICounter> Counters { get; }
        IEnumerable<ITag> Tags { get; }
        CollectorEngineType CollectorEngineType { get; }
        bool AllowDuplicateInstanceNames { get; }

        /// <summary>
        /// Gets all filters defined on this group that should be applied on instance names.
        /// </summary>
        IEnumerable<ICounterInstanceFilter> Filters { get; }
    }
}