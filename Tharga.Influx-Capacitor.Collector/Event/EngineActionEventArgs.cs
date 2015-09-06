using System;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector.Event
{
    public class EngineActionEventArgs : EventArgs
    {
        private readonly string _engineName;
        private readonly string _message;
        private readonly OutputLevel _outputLevel;

        public EngineActionEventArgs(string engineName, string message, OutputLevel outputLevel)
        {
            _engineName = engineName;
            _message = message;
            _outputLevel = outputLevel;
        }

        public string Message { get { return string.Format("Engine {0}: {1}", _engineName, _message); } }
        public OutputLevel OutputLevel { get { return _outputLevel; } }
    }
}