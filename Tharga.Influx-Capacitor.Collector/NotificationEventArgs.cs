using System;
using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector
{
    public class NotificationEventArgs : EventArgs
    {
        private readonly string _message;
        private readonly OutputLevel _outputLevel;

        public NotificationEventArgs(string message, OutputLevel outputLevel)
        {
            _message = message;
            _outputLevel = outputLevel;
        }

        public string Message { get { return _message; } }
        public OutputLevel OutputLevel { get { return _outputLevel; } }
    }
}