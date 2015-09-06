using System;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector.Event
{
    public class SendBusinessEventArgs : EventArgs
    {
        private readonly string _message;
        private readonly OutputLevel _outputLevel;
        private readonly Exception _exception;

        public SendBusinessEventArgs(Exception exception)
        {
            _exception = exception;
            _outputLevel = OutputLevel.Error;
        }

        public SendBusinessEventArgs(string message, int count, OutputLevel outputLevel)
        {
            _message = message;
            _outputLevel = outputLevel;
        }

        public string Message { get { return _exception != null ? _exception.Message : _message; } }
        public OutputLevel OutputLevel { get { return _outputLevel; } }
    }
}