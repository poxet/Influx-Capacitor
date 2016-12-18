using System;

namespace Tharga.InfluxCapacitor
{
    public interface IQueueEvents
    {
        void DebugMessageEvent(string message);
        void ExceptionEvent(Exception exception);
        void SendEvent(ISendEventInfo sendCompleteEventArgs);
        void QueueChangedEvent(IQueueChangeEventInfo queueChangeEventInfo);
    }
}