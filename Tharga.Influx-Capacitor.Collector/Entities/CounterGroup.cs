using System.Collections.Generic;
using System.Linq;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class CounterGroup : ICounterGroup
    {
        private readonly string _name;
        private readonly int _secondsInterval;
        private readonly int _refreshInstanceInterval;
        private readonly List<ICounter> _counters;
        private readonly List<ITag> _tags;
        private readonly CollectorEngineType _collectorEngineType;

        public CounterGroup(string name, int secondsInterval, int refreshInstanceInterval, List<ICounter> counters, IEnumerable<ITag> tags, CollectorEngineType collectorEngineType)
        {
            _name = name;
            _secondsInterval = secondsInterval;
            _refreshInstanceInterval = refreshInstanceInterval;
            _counters = counters;
            _tags = (tags ?? new List<ITag>()).ToList();
            _collectorEngineType = collectorEngineType;
        }

        public string Name { get { return _name; } }
        public int SecondsInterval { get { return _secondsInterval; } }
        public int RefreshInstanceInterval { get { return _refreshInstanceInterval; } }
        public IEnumerable<ICounter> Counters { get { return _counters; } }
        public IEnumerable<ITag> Tags { get { return _tags; } }
        public CollectorEngineType CollectorEngineType { get { return _collectorEngineType; } }
    }
}