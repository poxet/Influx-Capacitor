using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Business;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class SendBusiness : ISendBusiness
    {
        private readonly List<IQueue> _queues = new List<IQueue>();

        //public event EventHandler<SendCompleteEventArgs> SendBusinessEvent;

        public SendBusiness(IConfigBusiness configBusiness, IQueueEvents queueEvents)
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
                    queueEvents.OnExceptionEvent(exception);
                }

                if (senderAgent != null)
                {
                    var queueSettings = new QueueSettings(config.Application.FlushSecondsInterval, false, config.Application.MaxQueueSize);
                    var metaDataBusiness = new MetaDataBusiness();
                    _queues.Add(new Queue(senderAgent, queueEvents, metaDataBusiness, queueSettings));
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

        public IEnumerable<IQueueCountInfo> GetQueueInfo()
        {
            foreach (var queue in _queues)
            {
                yield return queue.GetQueueInfo();
            }
        }
    }
}