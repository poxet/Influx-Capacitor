using System.Diagnostics;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ICounterPublisher
    {
        string CounterName { get; }
        int SecondsInterval { get; }
        string CategoryName { get; }
        string CategoryHelp { get; }
        PerformanceCounterCategoryType PerformanceCounterCategoryType { get; }
        PerformanceCounterType CounterType { get; }
        long GetValue();
    }
}