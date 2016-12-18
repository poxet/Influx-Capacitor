namespace Tharga.InfluxCapacitor
{
    public interface IQueueSettings
    {
        int FlushSecondsInterval { get; }
        bool DropOnFail { get; }
        int MaxQueueSize { get; }
    }
}