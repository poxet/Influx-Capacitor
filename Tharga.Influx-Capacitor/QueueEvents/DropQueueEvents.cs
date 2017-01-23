using System;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.QueueEvents
{
    public class DropQueueEvents : IQueueEvents
    {
        public void DebugMessageEvent(string message)
        {
        }

        public void ExceptionEvent(Exception exception)
        {
        }

        public void SendEvent(ISendEventInfo sendCompleteEventArgs)
        {
        }

        public void QueueChangedEvent(IQueueChangeEventInfo queueChangeEventInfo)
        {
        }

        public void TimerEvent(ISendResponse sendResponse)
        {
        }
    }
}