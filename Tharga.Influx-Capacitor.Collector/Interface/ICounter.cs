namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ICounter
    {
        string Name { get; }
        string CategoryName { get; }
        string CounterName { get; }
        string InstanceName { get; }
        string Alias { get; }
    }
}