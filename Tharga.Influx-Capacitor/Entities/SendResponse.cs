using System;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class SendResponse : ISendResponse
    {
        internal SendResponse(bool isSuccess, string message, int pointCount, TimeSpan elapsed)
        {
            IsSuccess = isSuccess;
            Message = message;
            PointCount = pointCount;
            Elapsed = elapsed;
        }

        public bool IsSuccess { get; }
        public string Message { get; }
        public int PointCount { get; }
        public TimeSpan Elapsed { get; }
    }
}