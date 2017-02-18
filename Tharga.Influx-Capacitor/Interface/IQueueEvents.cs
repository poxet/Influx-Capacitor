using System;
using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface IQueueEvents
    {
        void OnDebugMessageEvent(string message);
        void OnExceptionEvent(Exception exception);
        void OnSendEvent(ISendEventInfo sendCompleteEventArgs);
        void OnQueueChangedEvent(IQueueChangeEventInfo queueChangeEventInfo);
        void OnTimerEvent(ISendResponse sendResponse);
        void OnEnqueueEvent(Point[] enqueuedPoints, Point[] providedPoints, string[] validationErrors);
    }
}