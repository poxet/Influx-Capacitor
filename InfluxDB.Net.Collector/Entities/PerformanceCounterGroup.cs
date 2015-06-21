using System.Collections.Generic;
using System.Diagnostics;
using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector.Entities
{
    public class PerformanceCounterGroup : IPerformanceCounterGroup
    {
        private readonly string _name;
        private readonly int _secondsInterval;
        private readonly IReadOnlyCollection<PerformanceCounter> _performanceCounters;

        public PerformanceCounterGroup(string name, int secondsInterval, IReadOnlyCollection<PerformanceCounter> performanceCounters)
        {
            _name = name;
            _secondsInterval = secondsInterval;
            _performanceCounters = performanceCounters;
        }

        public string Name { get { return _name; } }
        public int SecondsInterval { get { return _secondsInterval; } }
        public IEnumerable<PerformanceCounter> PerformanceCounters { get { return _performanceCounters; } }
    }
}