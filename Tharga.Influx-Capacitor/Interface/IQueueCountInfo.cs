namespace Tharga.InfluxCapacitor
{
    public interface IQueueCountInfo
    {
        int QueueCount { get; }
        int FailQueueCount { get; }
        int TotalQueueCount { get; }
    }
}