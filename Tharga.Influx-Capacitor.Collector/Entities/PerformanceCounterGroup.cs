using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class PerformanceCounterGroup : IPerformanceCounterGroup
    {
        private readonly string _name;
        private readonly int _secondsInterval;
        private readonly IReadOnlyCollection<IPerformanceCounterInfo> _performanceCounterInfos;
        private readonly IEnumerable<ITag> _tags;

        public PerformanceCounterGroup(string name, int secondsInterval, IReadOnlyCollection<IPerformanceCounterInfo> performanceCounterInfos, IEnumerable<ITag> tags)
        {
            _name = name;
            _secondsInterval = secondsInterval;
            _performanceCounterInfos = performanceCounterInfos;
            _tags = tags;
        }

        public string Name { get { return _name; } }
        public int SecondsInterval { get { return _secondsInterval; } }
        public IEnumerable<IPerformanceCounterInfo> PerformanceCounterInfos { get { return _performanceCounterInfos; } }
        public IEnumerable<ITag> Tags { get { return _tags; } }
    }
}