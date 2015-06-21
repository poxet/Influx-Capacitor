using System.Collections.Generic;

namespace InfluxDB.Net.Collector.Console.Entities
{
    public class CounterGroup
    {
        private readonly List<Counter> _counters;

        public CounterGroup(List<Counter> counters)
        {
            _counters = counters;
        }

        public IEnumerable<Counter> Counters { get { return _counters; } }
    }
}