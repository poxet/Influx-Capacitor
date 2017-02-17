using System;
using System.Collections.Generic;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.QueueEvents
{
    public class RelayQueueEvents : IQueueEvents
    {
        public event EventHandler<DebugMessageEventArgs> DebugMessageEvent;
        public event EventHandler<ExceptionEventArgs> ExceptionEvent;
        public event EventHandler<SendEventArgs> SendEvent;
        public event EventHandler<QueueChangedEventArgs> QueueChangedEvent;
        public event EventHandler<TimerEventArgs> TimerEvent;
        public event EventHandler<EnqueueEventArgs> EnqueueEvent;

        public virtual void OnDebugMessageEvent(string message)
        {
            DebugMessageEvent?.Invoke(this, new DebugMessageEventArgs(message));
        }

        public void OnExceptionEvent(Exception exception)
        {
            ExceptionEvent?.Invoke(this, new ExceptionEventArgs(exception));
        }

        public void OnSendEvent(ISendEventInfo sendCompleteEventArgs)
        {
            SendEvent?.Invoke(this, new SendEventArgs(sendCompleteEventArgs));
        }

        public void OnQueueChangedEvent(IQueueChangeEventInfo queueChangeEventInfo)
        {
            QueueChangedEvent?.Invoke(this, new QueueChangedEventArgs(queueChangeEventInfo));
        }

        public void OnTimerEvent(ISendResponse sendResponse)
        {
            TimerEvent?.Invoke(this, new TimerEventArgs(sendResponse));
        }

        public void OnEnqueueEvent(Point[] enqueuedPoints, Point[] providedPoints, string[] validationErrors)
        {
            EnqueueEvent?.Invoke(this, new EnqueueEventArgs(enqueuedPoints, providedPoints, validationErrors));
        }
    }
}