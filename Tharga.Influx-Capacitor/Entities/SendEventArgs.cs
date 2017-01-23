using System;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class SendEventArgs : EventArgs
    {
        internal SendEventArgs(ISendEventInfo sendCompleteEventArgs)
        {
            SendCompleteEventArgs = sendCompleteEventArgs;
        }

        public ISendEventInfo SendCompleteEventArgs { get; }
    }
}