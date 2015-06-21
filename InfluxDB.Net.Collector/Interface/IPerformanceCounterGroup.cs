using System.Collections.Generic;
using System.Diagnostics;

namespace InfluxDB.Net.Collector.Interface
{
    public interface IPerformanceCounterGroup
    {
        string Name { get; }
        int SecondsInterval { get; }
        IEnumerable<PerformanceCounter> PerformanceCounters { get; }
    }
}