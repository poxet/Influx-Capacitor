using System;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class TimerEventArgs : EventArgs
    {
        internal TimerEventArgs(ISendResponse sendResponse)
        {
            SendResponse = sendResponse;
        }

        public ISendResponse SendResponse { get; }
    }
}