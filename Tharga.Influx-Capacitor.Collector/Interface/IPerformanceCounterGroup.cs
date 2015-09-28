using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Handlers;

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
        CollectorEngineType CollectorEngineType { get; }
    }
}