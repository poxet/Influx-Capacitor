using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class SendBusiness : ISendBusiness
    {
        private static readonly object _syncRoot = new object();
        public event EventHandler<SendBusinessEventArgs> SendBusinessEvent;

        private readonly Timer _timer;
        private readonly Queue<Point[]> _queue = new Queue<Point[]>();
        private readonly Lazy<IInfluxDbAgent> _client;

        public SendBusiness(IConfigBusiness configBusiness, IInfluxDbAgentLoader influxDbAgentLoader)
        {
            var config = configBusiness.LoadFiles();

            _timer = new Timer(1000 * config.Application.FlushSecondsInterval);
            _timer.Elapsed += Elapsed;

            _client = new Lazy<IInfluxDbAgent>(() => influxDbAgentLoader.GetAgent(config.Database));
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_syncRoot)
            {
                if (!_timer.Enabled)
                {
                    return;
                }
                
                var pts = new List<Point>();
                try
                {
                    while (_queue.Count > 0)
                    {
                        var points = _queue.Dequeue();
                        pts.AddRange(points);
                    }

                    if (pts.Count == 0)
                    {
                        return;
                    }

                    //TODO: Possible to log what is sent

                    //Send all theese points to influx
                    _client.Value.WriteAsync(pts.ToArray());

                    OnSendBusinessEvent(new SendBusinessEventArgs(string.Format("Sending {0} points to server.", pts.Count), pts.Count, false));
                }
                catch (Exception exception)
                {
                    OnSendBusinessEvent(new SendBusinessEventArgs(exception));
                    OnSendBusinessEvent(new SendBusinessEventArgs(string.Format("Putting {0} points back in the queue.", pts.Count), pts.Count, true));
                    _queue.Enqueue(pts.ToArray()); //Put the points back in the queue to be sent later.
                    _timer.Enabled = false;
                }
            }
        }

        public void Enqueue(Point[] points)
        {
            if (!points.Any())
            {
                return;
            }

            if (!_timer.Enabled)
            {
                _timer.Start();
            }

            _queue.Enqueue(points);
        }

        protected virtual void OnSendBusinessEvent(SendBusinessEventArgs e)
        {
            var handler = SendBusinessEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}