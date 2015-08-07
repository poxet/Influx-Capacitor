namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ICounter
    {
        string Name { get; }
        string CategoryName { get; }
        string InstanceName { get; }
        string CounterName { get; }
    }
}