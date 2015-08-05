using System.Diagnostics;

namespace InfluxDB.Net.Collector.Interface
{
    public interface IPerformanceCounterInfo
    {
        string Name { get; }
        PerformanceCounter PerformanceCounter { get; }
    }
}