using System;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
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

        internal SendEventInfo(Exception exception)
        {
            _exception = exception;
            Level = OutputLevel.Error;
        }

        internal SendEventInfo(string message, int count, OutputLevel outputLevel)
        {
            _message = message;
            Level = outputLevel;
        }

        public OutputLevel Level { get; }
        public string Message => _exception?.Message ?? _message;
    }
}