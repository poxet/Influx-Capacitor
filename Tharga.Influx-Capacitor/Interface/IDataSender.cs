using System;
using InfluxDB.Net.Models;
using Tharga.Influx_Capacitor.Entities;

namespace Tharga.Influx_Capacitor.Interface
{
    public interface IDataSender
    {
        event EventHandler<SendCompleteEventArgs> SendCompleteEvent;
        SendResponse Send();
        void Enqueue(Point[] points);
        string TargetServer { get; }
        string TargetDatabase { get; }
        int QueueCount { get; }
    }
}