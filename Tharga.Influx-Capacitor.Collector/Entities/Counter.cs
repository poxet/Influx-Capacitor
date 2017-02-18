using System.Collections.Generic;
using System.Linq;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class Counter : ICounter
    {
        private readonly string _categoryName;
        private readonly Naming _counterName;
        private readonly Naming _instanceName;
        private readonly string _fieldName;
        private readonly List<ITag> _tags;
        private readonly float? _max;
        private readonly float? _min;
        private readonly float? _reverse;
        private readonly string _machineName;

        public Counter(string categoryName, Naming counterName, Naming instanceName, string fieldName, IEnumerable<ITag> tags, float? max, float? min, string machineName, float? reverse)
        {
            _categoryName = categoryName;
            _counterName = counterName;
            _instanceName = instanceName;
            _fieldName = fieldName;
            _tags = (tags ?? new List<ITag>()).ToList();
            _max = max;
            _min = min;
            _machineName = machineName;
            _reverse = reverse;
        }

        public string Name { get { return _instanceName.Name ?? CategoryName; } }
        public string MachineName { get { return _machineName; } }
        public string CategoryName { get { return _categoryName; } }
        public Naming CounterName { get { return _counterName; } }
        public Naming InstanceName { get { return _instanceName; } }
        public string FieldName { get { return _fieldName; } }
        public IEnumerable<ITag> Tags { get { return _tags; } }
        public float? Max { get { return _max; } }
        public float? Min { get { return _min; } }
        public float? Reverse {  get { return _reverse; } }
    }
}