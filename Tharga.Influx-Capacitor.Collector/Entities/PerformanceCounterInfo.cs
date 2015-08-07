using System.Diagnostics;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class PerformanceCounterInfo : IPerformanceCounterInfo
    {
        private readonly string _name;
        private readonly PerformanceCounter _performanceCounters;

        public PerformanceCounterInfo(string name, PerformanceCounter performanceCounters)
        {
            _name = name;
            _performanceCounters = performanceCounters;
        }

        public string Name { get { return _name; } }
        public PerformanceCounter PerformanceCounter { get { return _performanceCounters; } }
    }
}