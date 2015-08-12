using System;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Event;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ISendBusiness
    {
        void Enqueue(Point[] points);
        event EventHandler<SendBusinessEventArgs> SendBusinessEvent;
    }
}