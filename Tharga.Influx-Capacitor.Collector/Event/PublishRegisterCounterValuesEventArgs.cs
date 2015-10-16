using System;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector.Event
{
    public class PublishRegisterCounterValuesEventArgs : EventArgs
    {
        private readonly string _engineName;
        private readonly string _message;
        private readonly OutputLevel _outputLevel;

        public PublishRegisterCounterValuesEventArgs(string engineName, Exception exception)
        {
            _engineName = engineName;
            _message = exception.Message;
            _outputLevel = OutputLevel.Error;
        }

        public PublishRegisterCounterValuesEventArgs(string engineName, string message, OutputLevel outputLevel)
        {
            _engineName = engineName;
            _message = message;
            _outputLevel = outputLevel;
        }

        public string EngineName { get { return _engineName; } }
        public string Message { get { return _message; } }
        public OutputLevel OutputLevel { get { return _outputLevel; } }
    }
}