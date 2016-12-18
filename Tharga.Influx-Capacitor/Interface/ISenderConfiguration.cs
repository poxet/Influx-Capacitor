namespace Tharga.InfluxCapacitor.Interface
{
    public interface ISenderConfiguration
    {
        bool IsEnabled { get; }
        string Type { get; }
        int MaxQueueSize { get; }
        dynamic Properties { get; }
    }
}