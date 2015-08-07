using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class Counter : ICounter
    {
        private readonly string _name;
        private readonly string _categoryName;
        private readonly string _counterName;
        private readonly string _instanceName;

        public Counter(string name, string categoryName, string counterName, string instanceName)
        {
            _name = name;
            _categoryName = categoryName;
            _counterName = counterName;
            _instanceName = instanceName;
        }

        public string Name
        {
            get
            {
                if (_name != null) 
                    return _name;

                if (_instanceName != null)
                {
                    //int tmp;
                    //if (int.TryParse(_instanceName, out tmp))
                    //    return CategoryName + _instanceName;
                    return _instanceName;
                }

                return CategoryName;
            }
        }
        public string CategoryName { get { return _categoryName; } }
        public string CounterName { get { return _counterName; } }
        public string InstanceName { get { return _instanceName; } }
    }
}