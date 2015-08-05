using System.Collections.Generic;

namespace InfluxDB.Net.Collector.Interface
{
    public interface IPerformanceCounterGroup
    {
        string Name { get; }
        int SecondsInterval { get; }
        IEnumerable<IPerformanceCounterInfo> PerformanceCounterInfos { get; }
    }
}