using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class CounterGroup : ICounterGroup
    {
        private readonly string _name;
        private readonly List<ICounter> _counters;
        private readonly int _secondsInterval;

        public CounterGroup(string name, int secondsInterval, List<ICounter> counters)
        {
            _name = name;
            _counters = counters;
            _secondsInterval = secondsInterval;
        }

        public string Name { get { return _name; } }
        public int SecondsInterval { get { return _secondsInterval; } }
        public IEnumerable<ICounter> Counters { get { return _counters; } }
    }
}