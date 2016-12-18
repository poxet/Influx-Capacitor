namespace Tharga.InfluxCapacitor.Interface
{
    public interface IQueueChangeEventInfo
    {
        IQueueCountInfo Before { get; }
        IQueueCountInfo After { get; }
        string Message { get; }
    }
}