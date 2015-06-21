using System.Collections.Generic;

namespace InfluxDB.Net.Collector.Entities
{
    public class CounterGroup
    {
        private readonly string _name;
        private readonly List<Counter> _counters;
        private readonly int _secondsInterval;

        public CounterGroup(string name, int secondsInterval, List<Counter> counters)
        {
            _name = name;
            _counters = counters;
            _secondsInterval = secondsInterval;
        }

        public string Name { get { return _name; } }
        public int SecondsInterval { get { return _secondsInterval; } }
        public IEnumerable<Counter> Counters { get { return _counters; } }
    }
}