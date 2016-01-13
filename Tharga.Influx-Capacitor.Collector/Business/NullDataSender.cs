using System;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    internal class NullDataSender : IDataSender
    {
        private readonly int _maxQueueSize;
        private int _count;

        public NullDataSender(int maxQueueSize)
        {
            _maxQueueSize = maxQueueSize;
        }

        public event EventHandler<SendBusinessEventArgs> SendBusinessEvent;

        public SendResponse Send()
        {
            _count = 0;
            return new SendResponse("null", null);
        }

        public void Enqueue(Point[] points)
        {
            if (_maxQueueSize - _count < points.Length)
            {
                //OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Queue will reach max limit, cannot add more points. Have {0} points, want to add {1} more. The limit is {2}.", _queue.Count, points.Length, _maxQueueSize), _queue.Count, OutputLevel.Error));
                return;
            }

            _count += points.Length;
        }

        public string TargetServer { get { return "null"; } }
        public string TargetDatabase { get { return "null"; } }
        public int QueueCount { get { return _count; } }
    }
}