using System;

namespace Tharga.InfluxCapacitor.Collector.Event
{
    public class EngineActionEventArgs : EventArgs
    {
        private readonly string _engineName;
        private readonly string _message;
        private readonly bool _success;

        public EngineActionEventArgs(string engineName, string message, bool success)
        {
            _engineName = engineName;
            _message = message;
            _success = success;
        }

        public string Message
        {
            get
            {
                return string.Format("Engine {0}: {1}", _engineName, _message);
            }
        }

        public bool Success
        {
            get
            {
                return _success;
            }
        }
    }
}