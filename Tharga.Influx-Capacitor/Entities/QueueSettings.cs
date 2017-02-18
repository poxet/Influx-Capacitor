using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class QueueSettings : IQueueSettings
    {
        public QueueSettings(int flushSecondsInterval = 30, bool dropOnFail = false, int maxQueueSize = 20000, bool metadata = true)
        {
            FlushSecondsInterval = flushSecondsInterval;
            DropOnFail = dropOnFail;
            MaxQueueSize = maxQueueSize;
            Metadata = metadata;
        }

        public int FlushSecondsInterval { get; }
        public bool DropOnFail { get; }
        public int MaxQueueSize { get; }
        public bool Metadata { get; }
    }
}