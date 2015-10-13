using System;
using System.Collections.Generic;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    internal class InfluxDataSender : IDataSender
    {
        private readonly IDatabaseConfig _databaseConfig;
        private readonly object _syncRoot = new object();
        public event EventHandler<SendBusinessEventArgs> SendBusinessEvent;

        private readonly Queue<Point[]> _queue = new Queue<Point[]>();
        private readonly Lazy<IInfluxDbAgent> _client;

        public InfluxDataSender(IInfluxDbAgentLoader influxDbAgentLoader, IDatabaseConfig databaseConfig)
        {
            _databaseConfig = databaseConfig;
            _client = new Lazy<IInfluxDbAgent>(() => influxDbAgentLoader.GetAgent(databaseConfig));
        }

        public Tuple<string, double?> Send()
        {
            string responseMessage = null;
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            lock (_syncRoot)
            {
                var pts = new List<Point>();
                try
                {
                    while (_queue.Count > 0)
                    {
                        var points = _queue.Dequeue();
                        pts.AddRange(points);
                    }

                    if (pts.Count != 0)
                    {
                        var client = _client.Value;
                        if (client != null)
                        {
                            //TODO: Possible to log what is sent
                            client.WriteAsync(pts.ToArray());
                            OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Sending {0} points to server.", pts.Count), pts.Count, OutputLevel.Information));
                        }
                        else
                        {
                            responseMessage = "There is no client configured for sending data to the database, or the client has invalid settings.";
                            OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, responseMessage, pts.Count, OutputLevel.Error));
                        }
                    }
                }
                catch (Exception exception)
                {
                    responseMessage = exception.Message;
                    OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, exception));
                    OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Putting {0} points back in the queue.", pts.Count), pts.Count, OutputLevel.Warning));
                    _queue.Enqueue(pts.ToArray()); //Put the points back in the queue to be sent later.
                }
            }

            return new Tuple<string, double?>(responseMessage, stopWatch.Elapsed.TotalMilliseconds);
        }

        public void Enqueue(Point[] points)
        {
            lock (_syncRoot)
            {
                _queue.Enqueue(points);
            }
        }

        public string TargetServer { get { return _databaseConfig.Url; } }
        public string TargetDatabase { get { return _databaseConfig.Name; } }
        public int QueueCount { get { return _queue.Count; } }

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