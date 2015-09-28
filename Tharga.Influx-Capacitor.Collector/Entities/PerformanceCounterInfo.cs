using System.Diagnostics;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class PerformanceCounterInfo : IPerformanceCounterInfo
    {
        private readonly string _name;
        private readonly PerformanceCounter _performanceCounters;
        private readonly string _alias;

        public PerformanceCounterInfo(string name, PerformanceCounter performanceCounters, string alias)
        {
            _name = name;
            _performanceCounters = performanceCounters;
            _alias = alias;
        }

        public string Name { get { return _name; } }
        public PerformanceCounter PerformanceCounter { get { return _performanceCounters; } }
        public string Alias { get { return _alias; } }
    }
}