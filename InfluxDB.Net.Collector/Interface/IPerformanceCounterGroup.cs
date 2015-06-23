using System.Collections.Generic;
using System.Diagnostics;

namespace InfluxDB.Net.Collector.Interface
{
    public interface IPerformanceCounterGroup
    {
        string Name { get; }
        int SecondsInterval { get; }
        IEnumerable<IPerformanceCounterInfo> PerformanceCounterInfos { get; }
    }

    public interface IPerformanceCounterInfo
    {
        string Name { get; }
        PerformanceCounter PerformanceCounter { get; }
    }
}