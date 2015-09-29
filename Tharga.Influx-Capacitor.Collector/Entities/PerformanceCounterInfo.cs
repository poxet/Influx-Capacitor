using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class PerformanceCounterInfo : IPerformanceCounterInfo
    {
        private readonly string _name;
        private readonly PerformanceCounter _performanceCounters;
        private readonly string _alias;
        private readonly List<ITag> _tags;

        public PerformanceCounterInfo(string name, PerformanceCounter performanceCounters, string alias, IEnumerable<ITag> tags)
        {
            _name = name;
            _performanceCounters = performanceCounters;
            _alias = alias;
            _tags = tags.ToList();
        }

        public string Name { get { return _name; } }
        public PerformanceCounter PerformanceCounter { get { return _performanceCounters; } }
        public string Alias { get { return _alias; } }
        public IEnumerable<ITag> Tags { get { return _tags; } }
    }
}