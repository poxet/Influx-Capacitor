using System;
using System.Collections.Generic;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector.Event
{
    public class CollectRegisterCounterValuesEventArgs : EventArgs
    {
        private readonly string _engineName;
        private readonly string _message;
        private readonly OutputLevel _outputLevel;

        public CollectRegisterCounterValuesEventArgs(string engineName, Exception exception)
        {
            _engineName = engineName;
            _message = exception.Message;
            _outputLevel = OutputLevel.Error;
        }

        public CollectRegisterCounterValuesEventArgs(string engineName, string message, OutputLevel outputLevel)
        {
            _engineName = engineName;
            _message = message;
            _outputLevel = outputLevel;
        }

        public CollectRegisterCounterValuesEventArgs(string engineName, int counters, Dictionary<string, long> timeInfo, double elapseOffset, OutputLevel outputLevel)
        {
            _engineName = engineName;
            _message = string.Format("Read {0} metrics in {1}ms. ElapseOffset: {2}ms", counters, new TimeSpan(timeInfo["read"]).TotalMilliseconds.ToString("0.####"), elapseOffset.ToString("0.####"));
            _outputLevel = outputLevel;
        }

        public string EngineName { get { return _engineName; } }
        public string Message { get { return _message; } }
        public OutputLevel OutputLevel { get { return _outputLevel; } }
    }
}