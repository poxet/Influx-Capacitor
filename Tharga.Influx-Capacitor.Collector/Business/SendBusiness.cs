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
        private readonly List<DataSender> _dataSenders;

        public SendBusiness(IConfigBusiness configBusiness, IInfluxDbAgentLoader influxDbAgentLoader)
        {
            var config = configBusiness.LoadFiles();

            var flushMilliSecondsInterval = 1000 * config.Application.FlushSecondsInterval;
            if (flushMilliSecondsInterval > 0)
            {
                _timer = new Timer(flushMilliSecondsInterval);
                _timer.Elapsed += Elapsed;
            }

            _dataSenders = config.Databases.Select(x => new DataSender(influxDbAgentLoader, x)).ToList();
            foreach (var dataSender in _dataSenders)
            {
                dataSender.SendBusinessEvent += OnSendBusinessEvent;
            }
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach(var dataSender in _dataSenders)
            {
                dataSender.Send();
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