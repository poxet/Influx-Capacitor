using System;

namespace InfluxDB.Net.Collector
{
    public class NotificationEventArgs : EventArgs
    {
        private readonly string _message;

        public NotificationEventArgs(string message)
        {
            _message = message;
        }

        public string Message { get { return _message; } }
    }
}