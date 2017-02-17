using System;
using System.Collections.Generic;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.QueueEvents
{
    public class DropQueueEvents : IQueueEvents
    {
        public void OnDebugMessageEvent(string message)
        {
        }

        public void OnExceptionEvent(Exception exception)
        {
        }

        public void OnSendEvent(ISendEventInfo sendCompleteEventArgs)
        {
        }

        public void OnQueueChangedEvent(IQueueChangeEventInfo queueChangeEventInfo)
        {
        }

        public void OnTimerEvent(ISendResponse sendResponse)
        {
        }

        public void OnEnqueueEvent(Point[] enqueuedPoints, Point[] providedPoints, string[] validationErrors)
        {
        }
    }
}