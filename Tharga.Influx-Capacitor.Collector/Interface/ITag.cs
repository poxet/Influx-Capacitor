namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ITag
    {
        string Name { get; }
        string Value { get; }
    }
}