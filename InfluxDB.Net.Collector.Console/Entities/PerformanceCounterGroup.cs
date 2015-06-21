using System.Collections.Generic;
using System.Diagnostics;

namespace InfluxDB.Net.Collector.Console.Entities
{
    public class PerformanceCounterGroup
    {
        private readonly string _name;
        private readonly List<PerformanceCounter> _performanceCounters;

        public PerformanceCounterGroup(string name, List<PerformanceCounter> performanceCounters)
        {
            _name = name;
            _performanceCounters = performanceCounters;
        }

        public string Name { get { return _name; } }
        public List<PerformanceCounter> PerformanceCounters { get { return _performanceCounters; }}
    }
}