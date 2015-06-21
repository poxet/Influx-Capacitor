using System.Collections.Generic;

namespace InfluxDB.Net.Collector.Console.Entities
{
    public class CounterGroup
    {
        private readonly string _name;
        private readonly List<Counter> _counters;

        public CounterGroup(string name, List<Counter> counters)
        {
            _name = name;
            _counters = counters;
        }

        public string Name { get { return _name; } }
        public IEnumerable<Counter> Counters { get { return _counters; } }
    }
}