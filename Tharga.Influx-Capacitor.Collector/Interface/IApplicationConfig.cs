namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IApplicationConfig
    {
        int FlushSecondsInterval { get; }
        bool DebugMode { get; }
        bool Metadata { get; }
        int MaxQueueSize { get; }
    }
}