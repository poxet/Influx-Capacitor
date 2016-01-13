using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;
using Tharga.Toolkit.Console.Command.Base;
using SendBusinessEventArgs = Tharga.InfluxCapacitor.Collector.Event.SendBusinessEventArgs;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class SendBusiness : ISendBusiness
    {
        public event EventHandler<SendBusinessEventArgs> SendBusinessEvent;

        public IEnumerable<Tuple<string, int>> GetQueueInfo()
        {
            return _dataSenders.Select(x => new Tuple<string, int>(x.TargetDatabase, x.QueueCount));
        }

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

            _dataSenders = config.Databases.Where(x => x.IsEnabled).Select(x => x.GetDataSender(influxDbAgentLoader, config.Application.MaxQueueSize)).ToList();
            foreach (var dataSender in _dataSenders)
            {
                dataSender.SendBusinessEvent += DataSender_SendBusinessEvent;
            }

            _metadata = config.Application.Metadata;
        }

        private void DataSender_SendBusinessEvent(object sender, SendEventArgs e)
        {
            //TODO: Fire!
            throw new NotImplementedException();
            //OnSendBusinessEvent(sender, new SendBusinessEventArgs());
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            var metaPoints = new List<Point>();

            foreach (var dataSender in _dataSenders)
            {
                var previousQueueCount = dataSender.QueueCount;
                var success = dataSender.Send();
                var postQueueCount = dataSender.QueueCount;

                if (_metadata)
                {
                    metaPoints.Add(MetaDataBusiness.GetQueueCountPoints("Send", dataSender.TargetServer, dataSender.TargetDatabase, previousQueueCount, postQueueCount - previousQueueCount + _dataSenders.Count, success));
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
                //TODO: Fire!
                //OnSendBusinessEvent(this, new SendBusinessEventArgs(null, "The engine that sends data to the database is not enabled. Probably becuse the FlushSecondsInterval has not been configured.", points.Length, OutputLevel.Error));
                return;
            }

            if (!_timer.Enabled)
            {
                _timer.Start();
            }

            //Prepare metadata information about the queue and add that to the points to be sent.
            if (_metadata)
            {
                var metaPoints = _dataSenders.Select(x => MetaDataBusiness.GetQueueCountPoints("Enqueue", x.TargetServer, x.TargetDatabase, x.QueueCount, points.Length + _dataSenders.Count, new SendResponse(null, null))).ToArray();
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
            handler?.Invoke(this, e);
        }
    }
}