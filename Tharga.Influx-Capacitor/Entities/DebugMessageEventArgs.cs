using System;

namespace Tharga.InfluxCapacitor.Entities
{
    public class DebugMessageEventArgs : EventArgs
    {
        internal DebugMessageEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}