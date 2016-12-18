using System;
using System.Text;
using System.Threading;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class QueueCountInfo : IQueueCountInfo
    {
        public QueueCountInfo(int queueCount, int failQueueCount)
        {
            QueueCount = queueCount;
            FailQueueCount = failQueueCount;
        }

        public int QueueCount { get; }
        public int FailQueueCount { get; }
        public int TotalQueueCount => QueueCount + FailQueueCount;
    }

    public class QueueChangeEventInfo : IQueueChangeEventInfo
    {
        public QueueChangeEventInfo(IQueueCountInfo before, IQueueCountInfo after)
        {
            Before = before;
            After = after;
        }

        public IQueueCountInfo Before { get; }
        public IQueueCountInfo After { get; }

        public string Message
        {
            get
            {
                var sb = new StringBuilder();
                if (Before.QueueCount != After.QueueCount)
                    sb.AppendFormat("Queue changed from {0} to {1} items. ", Before.QueueCount, After.QueueCount);

                if (Before.FailQueueCount != After.FailQueueCount)
                    sb.AppendFormat("Fail queue changed from {0} to {1} items. ", Before.FailQueueCount, After.FailQueueCount);

                var response = sb.ToString().Trim();
                return string.IsNullOrEmpty(response) ? "No queue change" : response;
            }
        }
    }

    public class SendEventInfo : ISendEventInfo
    {
        public enum OutputLevel
        {
            Information,
            Warning,
            Error,
        }

        private readonly string _message;
        private readonly Exception _exception;

        public SendEventInfo(Exception exception)
        {
            _exception = exception;
            Level = OutputLevel.Error;
        }

        public SendEventInfo(string message, int count, OutputLevel outputLevel)
        {
            _message = message;
            Level = outputLevel;
        }

        public OutputLevel Level { get; private set; }
        public string Message { get { return _exception != null ? _exception.Message : _message; } }
    }

    public class SendCompleteEventArgs : EventArgs
    {
        public enum OutputLevel
        {
            Information,
            Warning,
            Error,
        }

        private readonly ISenderConfiguration _senderConfiguration;
        private readonly string _message;
        private readonly Exception _exception;

        public SendCompleteEventArgs(ISenderConfiguration senderConfiguration, Exception exception)
        {
            _senderConfiguration = senderConfiguration;
            _exception = exception;
            Level = OutputLevel.Error;
        }

        public SendCompleteEventArgs(ISenderConfiguration senderConfiguration, string message, int count, OutputLevel outputLevel)
        {
            _senderConfiguration = senderConfiguration;
            _message = message;
            Level = outputLevel;
        }

        public OutputLevel Level { get; private set; }
        public string Message { get { return _exception != null ? _exception.Message : _message; } }
    }
}