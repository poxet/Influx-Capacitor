using System;
using System.Collections.Generic;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    internal class AccDataSender : IDataSender
    {
        private readonly List<Point> _points = new List<Point>();
        public event EventHandler<SendBusinessEventArgs> SendBusinessEvent;

        public void Send()
        {            
        }

        public void Enqueue(Point[] points)
        {
            _points.AddRange(points);
        }

        public string TargetServer { get { return "acc"; } }
        public string TargetDatabase { get { return "acc"; } }
        public int QueueCount { get { return _points.Count; } }
    }
}