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
        private readonly string _fieldName;
        private readonly string _alias;
        private readonly List<ITag> _tags;
        private readonly float? _max;
        private readonly string _machineName;

        public Counter(string categoryName, string counterName, string instanceName, string fieldName, string alias, IEnumerable<ITag> tags, float? max, string machineName)
        {
            _categoryName = categoryName;
            _counterName = counterName;
            _instanceName = instanceName;
            _fieldName = fieldName;
            _alias = alias;
            _tags = (tags ?? new List<ITag>()).ToList();
            _max = max;
            _machineName = machineName;
        }

        public string Name { get { return _instanceName ?? CategoryName; } }
        public string MachineName { get { return _machineName; } }
        public string CategoryName { get { return _categoryName; } }
        public string CounterName { get { return _counterName; } }
        public string InstanceName { get { return _instanceName; } }
        public string FieldName { get { return _fieldName; } }
        public string Alias { get { return _alias; } }
        public IEnumerable<ITag> Tags { get { return _tags; } }
        public float? Max {  get { return _max; } }
    }
}