using System.Collections.Generic;
using System.Linq;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class Counter : ICounter
    {
        private readonly string _categoryName;
        private readonly string _counterName;
        private readonly string _instanceName;
        private readonly string _alias;
        private readonly List<ITag> _tags;

        public Counter(string categoryName, string counterName)
            : this(categoryName, counterName, string.Empty, null, null)
        {
        }

        public Counter(string categoryName, string counterName, string instanceName, string alias, IEnumerable<ITag> tags)
        {
            _categoryName = categoryName;
            _counterName = counterName;
            _instanceName = instanceName;
            _alias = alias;
            _tags = (tags ?? new List<ITag>()).ToList();
        }

        public string Name { get { return _instanceName ?? CategoryName; } }
        public string CategoryName { get { return _categoryName; } }
        public string CounterName { get { return _counterName; } }
        public string InstanceName { get { return _instanceName; } }
        public string Alias { get { return _alias; } }
        public IEnumerable<ITag> Tags { get { return _tags; } }
    }
}