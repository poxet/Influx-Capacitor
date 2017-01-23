namespace Tharga.InfluxCapacitor.Interface
{
    public interface IQueueSettings
    {
        int FlushSecondsInterval { get; }
        bool DropOnFail { get; }
        int MaxQueueSize { get; }
    }
}