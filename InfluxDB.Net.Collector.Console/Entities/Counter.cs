namespace InfluxDB.Net.Collector.Console.Entities
{
    public class Counter
    {
        private readonly string _categoryName;
        private readonly string _counterName;
        private readonly string _instanceName;

        public Counter(string categoryName, string counterName, string instanceName)
        {
            _categoryName = categoryName;
            _counterName = counterName;
            _instanceName = instanceName;
        }

        public string CategoryName { get { return _categoryName; } }
        public string CounterName { get { return _counterName; } }
        public string InstanceName { get { return _instanceName; } }
    }
}