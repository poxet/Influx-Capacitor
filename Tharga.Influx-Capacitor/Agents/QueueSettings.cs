namespace Tharga.InfluxCapacitor.Agents
{
    public class QueueSettings : IQueueSettings
    {
        public QueueSettings(int flushSecondsInterval, bool dropOnFail, int maxQueueSize)
        {
            FlushSecondsInterval = flushSecondsInterval;
            DropOnFail = dropOnFail;
            MaxQueueSize = maxQueueSize;
        }

        public int FlushSecondsInterval { get; }
        public bool DropOnFail { get; }
        public int MaxQueueSize { get; }
    }
}