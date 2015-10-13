using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class SendBusiness : ISendBusiness
    {
        public event EventHandler<SendBusinessEventArgs> SendBusinessEvent;

        private readonly Timer _timer;
        private readonly List<IDataSender> _dataSenders;
        private readonly bool _metadata;

        public SendBusiness(IConfigBusiness configBusiness, IInfluxDbAgentLoader influxDbAgentLoader)
        {
            var config = configBusiness.LoadFiles();

            var flushMilliSecondsInterval = 1000 * config.Application.FlushSecondsInterval;
            if (flushMilliSecondsInterval > 0)
            {
                _timer = new Timer(flushMilliSecondsInterval);
                _timer.Elapsed += Elapsed;
            }

            _dataSenders = config.Databases.Select(x => x.GetDataSender(influxDbAgentLoader)).ToList();
            foreach (var dataSender in _dataSenders)
            {
                dataSender.SendBusinessEvent += OnSendBusinessEvent;
            }

            _metadata = config.Application.Metadata;
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            var metaPoints = new List<Point>();

            foreach(var dataSender in _dataSenders)
            {
                var previousQueueCount = dataSender.QueueCount;
                dataSender.Send();
                var postQueueCount = dataSender.QueueCount;

                if (_metadata)
                {
                    metaPoints.Add(MetaDataBusiness.GetQueueCountPoints("Send", dataSender.TargetServer, dataSender.TargetDatabase, previousQueueCount, postQueueCount - previousQueueCount + _dataSenders.Count));
                }
            }

            if (_metadata)
            {
                foreach (var dataSender in _dataSenders)
                {
                    dataSender.Enqueue(metaPoints.ToArray());
                }
            }
        }

        public void Enqueue(Point[] points)
        {
            if (!points.Any())
            {
                return;
            }

            if (_timer == null)
            {
                OnSendBusinessEvent(this, new SendBusinessEventArgs(null, "The engine that sends data to the database is not enabled. Probably becuse the FlushSecondsInterval has not been configured.", points.Length, OutputLevel.Error));
                return;
            }

            if (!_timer.Enabled)
            {
                _timer.Start();
            }

            //Prepare metadata information about the queue and add that to the points to be sent.
            if (_metadata)
            {
                var metaPoints = _dataSenders.Select(x => MetaDataBusiness.GetQueueCountPoints("Enqueue", x.TargetServer, x.TargetDatabase, x.QueueCount, points.Length + _dataSenders.Count)).ToArray();
                points = points.Union(metaPoints).ToArray();
            }

            foreach (var dataSender in _dataSenders)
            {
                dataSender.Enqueue(points);
            }
        }

        protected virtual void OnSendBusinessEvent(object sender, SendBusinessEventArgs e)
        {
            var handler = SendBusinessEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}