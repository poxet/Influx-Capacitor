using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Agents;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class SendBusiness : ISendBusiness
    {
        private readonly List<IQueue> _queues = new List<IQueue>();

        public event EventHandler<SendCompleteEventArgs> SendBusinessEvent;

        public SendBusiness(IConfigBusiness configBusiness, IInfluxDbAgentLoader influxDbAgentLoader, IQueueEvents queueEvents)
        {
            var config = configBusiness.LoadFiles();
            foreach (var databaseConfig in config.Databases)
            {
                ISenderAgent senderAgent = null;
                try
                {
                    if (databaseConfig.IsEnabled)
                    {
                        senderAgent = databaseConfig.GetSenderAgent();
                    }
                }
                catch (Exception exception)
                {
                    queueEvents.ExceptionEvent(exception);
                }

                if (senderAgent != null)
                {
                    var queueSettings = new QueueSettings(config.Application.FlushSecondsInterval, false, config.Application.MaxQueueSize);
                    _queues.Add(new Queue(senderAgent, queueEvents, queueSettings));
                }
            }
        }

        public void Enqueue(Point[] points)
        {
            foreach (var queue in _queues)
            {
                queue.Enqueue(points);
            }
        }

        public IEnumerable<Tuple<string, int>> GetQueueInfo()
        {
            throw new NotImplementedException();
        }

        protected virtual void OnSendBusinessEvent(object sender, SendCompleteEventArgs e)
        {
            var handler = SendBusinessEvent;
            if (handler != null) handler.Invoke(this, e);
        }
    }
}