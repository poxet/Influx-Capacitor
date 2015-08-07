using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class Counter : ICounter
    {
        private readonly string _categoryName;
        private readonly string _counterName;
        private readonly string _instanceName;

        public Counter(string categoryName, string counterName)
            : this(categoryName, counterName, string.Empty)
        {
        }

        public Counter(string categoryName, string counterName, string instanceName)
        {
            _categoryName = categoryName;
            _counterName = counterName;
            _instanceName = instanceName;
        }

        public string Name { get { return _instanceName ?? CategoryName; } }
        public string CategoryName { get { return _categoryName; } }
        public string CounterName { get { return _counterName; } }
        public string InstanceName { get { return _instanceName; } }
    }
}