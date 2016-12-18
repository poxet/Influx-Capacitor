using System;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface ISendResponse
    {
        bool IsSuccess { get; }
        string Message { get; }
        int PointCount { get; }
        TimeSpan Elapsed { get; }
    }
}