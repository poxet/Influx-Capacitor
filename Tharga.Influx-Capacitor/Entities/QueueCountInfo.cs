namespace Tharga.InfluxCapacitor.Entities
{
    public class QueueCountInfo : IQueueCountInfo
    {
        public QueueCountInfo(int queueCount, int failQueueCount)
        {
            QueueCount = queueCount;
            FailQueueCount = failQueueCount;
        }

        public int QueueCount { get; }
        public int FailQueueCount { get; }
        public int TotalQueueCount => QueueCount + FailQueueCount;
    }
}