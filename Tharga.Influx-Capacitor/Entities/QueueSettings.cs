using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class QueueSettings : IQueueSettings
    {
        public QueueSettings(int flushSecondsInterval = 30, bool dropOnFail = false, int maxQueueSize = 20000, bool metaCounter = true)
        {
            FlushSecondsInterval = flushSecondsInterval;
            DropOnFail = dropOnFail;
            MaxQueueSize = maxQueueSize;
            MetaCounter = metaCounter;
        }

        public int FlushSecondsInterval { get; }
        public bool DropOnFail { get; }
        public int MaxQueueSize { get; }
        public bool MetaCounter { get; }
    }
}