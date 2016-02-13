using System;
using System.Collections.Generic;
using InfluxDB.Net.Models;
using Tharga.Influx_Capacitor.Entities;
using Tharga.Influx_Capacitor.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    internal class AccDataSender : IDataSender
    {
        private readonly int _maxQueueSize;
        private readonly List<Point> _points = new List<Point>();
        public event EventHandler<SendCompleteEventArgs> SendCompleteEvent;

        public AccDataSender(int maxQueueSize)
        {
            _maxQueueSize = maxQueueSize;
        }

        public SendResponse Send()
        {
            return new SendResponse("Keeping", null);
        }

        public void Enqueue(Point[] points)
        {
            if (_maxQueueSize - _points.Count < points.Length)
            {
                //OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Queue will reach max limit, cannot add more points. Have {0} points, want to add {1} more. The limit is {2}.", _queue.Count, points.Length, _maxQueueSize), _queue.Count, OutputLevel.Error));
                return;
            }

            _points.AddRange(points);
        }

        public string TargetServer { get { return "acc"; } }
        public string TargetDatabase { get { return "acc"; } }
        public int QueueCount { get { return _points.Count; } }
    }
}