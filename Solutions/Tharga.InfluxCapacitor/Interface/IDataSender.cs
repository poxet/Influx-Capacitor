using System;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Entities;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface IDataSender
    {
        event EventHandler<SendEventArgs> SendBusinessEvent;
        SendResponse Send();
        void Enqueue(Point[] points);
        string TargetServer { get; }
        string TargetDatabase { get; }
        int QueueCount { get; }
    }
}