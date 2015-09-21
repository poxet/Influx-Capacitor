using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IPerformanceCounterGroup
    {
        string Name { get; }
        int SecondsInterval { get; }
        IEnumerable<ITag> Tags { get; }
        IEnumerable<IPerformanceCounterInfo> GetFreshCounters();
    }
}