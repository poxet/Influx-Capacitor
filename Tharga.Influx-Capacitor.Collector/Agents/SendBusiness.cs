using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Agents
{
    public class SendBusiness : ISendBusiness
    {
        private readonly Timer _timer;
        private readonly Queue<Point[]> _queue = new Queue<Point[]>();
        private readonly IInfluxDbAgent _client;

        public SendBusiness(IConfigBusiness configBusiness, IInfluxDbAgentLoader influxDbAgentLoader)
        {
            var config = configBusiness.LoadFiles();

            _timer = new Timer(1000 * config.Database.FlushSecondsInterval);
            _timer.Elapsed += Elapsed;

            _client = influxDbAgentLoader.GetAgent(config.Database);
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var pts = new List<Point>();
                while (_queue.Count > 0)
                {
                    var points = _queue.Dequeue();
                    pts.AddRange(points);
                }

                //TODO: Possible to log what is sent

                //Send all theese points to influx
                _client.WriteAsync(pts.ToArray());
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry(Constants.ServiceName, exception.Message, EventLogEntryType.Error);
            }
        }

        public void Enqueue(Point[] points)
        {
            if (!_timer.Enabled)
            {
                _timer.Start();
            }

            _queue.Enqueue(points);
        }
    }
}