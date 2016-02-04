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
        private readonly string _fieldName;
        private readonly List<ITag> _tags;

        public PerformanceCounterInfo(string name, PerformanceCounter performanceCounters, string fieldName, string alias, IEnumerable<ITag> tags)
        {
            _name = name;
            _performanceCounters = performanceCounters;
            _fieldName = fieldName;
            _alias = alias;
            _tags = (tags ?? new List<ITag>()).ToList();
        }

        public string Name { get { return _name; } }
        public PerformanceCounter PerformanceCounter { get { return _performanceCounters; } }
        public string FieldName {  get { return _fieldName; } }
        public string Alias { get { return _alias; } }
        public IEnumerable<ITag> Tags { get { return _tags; } }
    }
}