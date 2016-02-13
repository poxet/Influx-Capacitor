using System;
using System.Collections.Generic;
using InfluxDB.Net.Models;
using Tharga.Influx_Capacitor.Entities;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ISendBusiness
    {
        void Enqueue(Point[] points);
        event EventHandler<SendCompleteEventArgs> SendBusinessEvent;
        IEnumerable<Tuple<string, int>> GetQueueInfo();
    }
}