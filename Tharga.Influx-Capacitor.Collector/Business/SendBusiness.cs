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
        private static readonly object _syncRoot = new object();
        public event EventHandler<SendBusinessEventArgs> SendBusinessEvent;

        private readonly Timer _timer;
        private readonly Queue<Point[]> _queue = new Queue<Point[]>();
        private readonly Lazy<IInfluxDbAgent> _client;

        public SendBusiness(IConfigBusiness configBusiness, IInfluxDbAgentLoader influxDbAgentLoader)
        {
            var config = configBusiness.LoadFiles();

            var flushMilliSecondsInterval = 1000 * config.Application.FlushSecondsInterval;
            if (flushMilliSecondsInterval > 0)
            {
                _timer = new Timer(flushMilliSecondsInterval);
                _timer.Elapsed += Elapsed;
            }

            _client = new Lazy<IInfluxDbAgent>(() => influxDbAgentLoader.GetAgent(config.Database));
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_syncRoot)
            {
                if (_timer == null || !_timer.Enabled)
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
                    var client = _client.Value;
                    if (client != null)
                    {
                        client.WriteAsync(pts.ToArray());
                        OnSendBusinessEvent(new SendBusinessEventArgs(string.Format("Sending {0} points to server.", pts.Count), pts.Count, OutputLevel.Information));
                    }
                    else
                    {
                        OnSendBusinessEvent(new SendBusinessEventArgs("There is no client configured for sending data to the database, or the client has invalid settings.", pts.Count, OutputLevel.Error));
                    }
                }
                catch (Exception exception)
                {
                    OnSendBusinessEvent(new SendBusinessEventArgs(exception));
                    OnSendBusinessEvent(new SendBusinessEventArgs(string.Format("Putting {0} points back in the queue.", pts.Count), pts.Count, OutputLevel.Warning));
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

            if (_timer == null)
            {
                OnSendBusinessEvent(new SendBusinessEventArgs("The engine that sends data to the database is not enabled. Probably becuse the FlushSecondsInterval has not been configured.", points.Length, OutputLevel.Error));
                return;
            }

            if (!_timer.Enabled)
            {
                _timer.Start();
            }

            lock (_syncRoot)
            {
                _queue.Enqueue(points);
            }
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