namespace InfluxDB.Net.Collector.Interface
{
    public interface ICounter
    {
        string CategoryName { get; }
        string InstanceName { get; }
        string CounterName { get; }
    }
}