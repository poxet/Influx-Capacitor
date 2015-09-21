using System.Diagnostics;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IPerformanceCounterInfo
    {
        string Name { get; }
        PerformanceCounter PerformanceCounter { get; }
    }
}