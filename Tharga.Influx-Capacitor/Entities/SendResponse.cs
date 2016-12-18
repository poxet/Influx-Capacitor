using System;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class SendResponse
    {
        public SendResponse(string message, TimeSpan? elapsed)
        {
            Message = message;
            Elapsed = elapsed;
        }

        public string Message { get; }
        public TimeSpan? Elapsed { get; }
    }
}