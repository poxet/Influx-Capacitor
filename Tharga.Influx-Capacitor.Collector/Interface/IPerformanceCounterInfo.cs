using System.Collections.Generic;
using System.Diagnostics;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IPerformanceCounterInfo
    {
        string Name { get; }
        PerformanceCounter PerformanceCounter { get; }
        string Alias { get; }
        IEnumerable<ITag> Tags { get; }
    }
}