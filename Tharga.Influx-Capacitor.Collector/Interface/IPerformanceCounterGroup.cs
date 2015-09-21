using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IPerformanceCounterGroup
    {
        string Name { get; }
        int SecondsInterval { get; }
        int RefreshInstanceInterval { get; }
        IEnumerable<ITag> Tags { get; }
        IEnumerable<IPerformanceCounterInfo> GetCounters();
        IEnumerable<IPerformanceCounterInfo> GetFreshCounters();
        void RemoveCounter(IPerformanceCounterInfo performanceCounterInfo);
    }
}