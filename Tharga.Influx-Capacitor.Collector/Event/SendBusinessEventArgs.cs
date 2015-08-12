using System;

namespace Tharga.InfluxCapacitor.Collector.Event
{
    public class SendBusinessEventArgs : EventArgs
    {
        private readonly string _message;
        private readonly bool _warning;
        private readonly Exception _exception;

        public SendBusinessEventArgs(Exception exception)
        {
            _exception = exception;
        }

        public SendBusinessEventArgs(string message, int count, bool warning)
        {
            _message = message;
            _warning = warning;
        }

        public string Message
        {
            get
            {
                return _exception != null ? _exception.Message : _message;
            }
        }

        public bool Success
        {
            get
            {
                return _exception == null && !_warning;
            }
        }

        public bool Warning
        {
            get
            {
                return _warning;
            }
        }
    }
}