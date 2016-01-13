using System;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Agents;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    internal class KafkaDataSender : IDataSender
    {
        private KafkaAgent _agent;
        public event EventHandler<SendBusinessEventArgs> SendBusinessEvent;

        public KafkaDataSender()
        {
            _agent = new KafkaAgent();
        }

        public SendResponse Send()
        {
            throw new NotImplementedException();
        }

        public void Enqueue(Point[] points)
        {
            throw new NotImplementedException();
        }

        public string TargetServer { get { throw new NotImplementedException(); } }
        public string TargetDatabase { get { throw new NotImplementedException(); } }
        public int QueueCount { get { throw new NotImplementedException(); } }
    }
}