using System;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    internal class NullDataSender : IDataSender
    {
        private int _count;
        public event EventHandler<SendBusinessEventArgs> SendBusinessEvent;

        public void Send()
        {
            _count = 0;
        }

        public void Enqueue(Point[] points)
        {
            _count += points.Length;
        }

        public string TargetServer { get { return "null"; } }
        public string TargetDatabase { get { return "null"; } }
        public int QueueCount { get { return _count; } }
    }
}