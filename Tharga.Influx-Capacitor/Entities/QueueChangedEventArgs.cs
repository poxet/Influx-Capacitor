using System;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class QueueChangedEventArgs : EventArgs
    {
        internal QueueChangedEventArgs(IQueueChangeEventInfo queueChangeEventInfo)
        {
            QueueChangeEventInfo = queueChangeEventInfo;
        }

        public IQueueChangeEventInfo QueueChangeEventInfo { get; }
    }
}