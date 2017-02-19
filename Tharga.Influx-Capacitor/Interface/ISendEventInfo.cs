using System;
using Tharga.InfluxCapacitor.Entities;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface ISendEventInfo
    {
        SendEventInfo.OutputLevel Level { get; }
        string Message { get; }
        Exception Exception { get; }
    }
}