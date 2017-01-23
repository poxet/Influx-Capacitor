using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
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