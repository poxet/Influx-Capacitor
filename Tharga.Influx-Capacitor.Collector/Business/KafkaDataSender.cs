using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Agents;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Influx_Capacitor.Entities;
using Tharga.Influx_Capacitor.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    internal class KafkaDataSender : IDataSender
    {
        private readonly object _syncRoot = new object();
        private readonly int _maxQueueSize;
        private readonly KafkaAgent _agent;
        private readonly Queue<Point[]> _queue = new Queue<Point[]>();
        private readonly Queue<Tuple<int, Point[]>> _failQueue = new Queue<Tuple<int, Point[]>>();
        private readonly IDatabaseConfig _databaseConfig;
        private readonly bool _dropOnFail;
        private bool _canSucceed;

        public event EventHandler<SendCompleteEventArgs> SendCompleteEvent;

        public KafkaDataSender(IDatabaseConfig databaseConfig, int maxQueueSize)
        {
            var kafkaServers = databaseConfig.Url.Split(';').Select(x => new Uri(x)).ToArray();

            _agent = new KafkaAgent(kafkaServers);
            _databaseConfig = databaseConfig;
            _maxQueueSize = maxQueueSize;
            _dropOnFail = false;
        }

        public SendResponse Send()
        {
            string responseMessage = null;
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            lock (_syncRoot)
            {
                Point[] points = null;
                var retryCount = 0;
                try
                {
                    while (_queue.Count + _failQueue.Count > 0)
                    {
                        if (_queue.Count > 0)
                        {
                            var pts = new List<Point>();
                            retryCount = 0;
                            while (_queue.Count > 0)
                            {
                                pts.AddRange(_queue.Dequeue());
                            }
                            points = pts.ToArray();
                        }
                        else
                        {
                            var meta = _failQueue.Dequeue();
                            retryCount = meta.Item1;
                            points = meta.Item2;
                        }

                        //TODO: Possible to log what is sent. To an output file or similar.
                        //var response = client.WriteAsync(points).Result;
                        _agent.Send(points);
                        _canSucceed = true;
                        //TODO: Fire!
                        //OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Sending {0} points to server.", points.Length), points.Length, OutputLevel.Information));
                    }
                }
                catch (Exception exception)
                {
                    if (exception is AggregateException)
                    {
                        exception = exception.InnerException;
                    }

                    responseMessage = exception.Message;
                    //TODO: Fire!
                    //OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, exception));
                    //if (points != null)
                    //{
                    //    if (!_dropOnFail)
                    //    {
                    //        var invalidExceptionType = exception.IsExceptionValidForPutBack();

                    //        if (invalidExceptionType != null)
                    //        {
                    //            OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Dropping {0} since the exception type {1} is not allowed for resend.", points.Length, invalidExceptionType), points.Length, OutputLevel.Warning));
                    //        }
                    //        else if (!_canSucceed)
                    //        {
                    //            OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Dropping {0} points because there have never yet been a successful send.", points.Length), points.Length, OutputLevel.Warning));
                    //        }
                    //        else if (retryCount > 5)
                    //        {
                    //            OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Dropping {0} points after {1} retries.", points.Length, retryCount), points.Length, OutputLevel.Warning));
                    //        }
                    //        else
                    //        {
                    //            OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Putting {0} points back in the queue.", points.Length), points.Length, OutputLevel.Warning));
                    //            retryCount++;
                    //            _failQueue.Enqueue(new Tuple<int, Point[]>(retryCount, points));
                    //        }
                    //    }
                    //    else
                    //    {
                    //        OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Dropping {0} points.", points.Length), points.Length, OutputLevel.Warning));
                    //    }
                    //}
                }
            }

            return new SendResponse(responseMessage, stopWatch.Elapsed.TotalMilliseconds);
        }

        public void Enqueue(Point[] points)
        {
            lock (_syncRoot)
            {
                if (_maxQueueSize - QueueCount < points.Length)
                {
                    //TODO: Fire!
                    //OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Queue will reach max limit, cannot add more points. Have {0} points, want to add {1} more. The limit is {2}.", QueueCount, points.Length, _maxQueueSize), QueueCount, OutputLevel.Error));
                    return;
                }

                _queue.Enqueue(points);
            }
        }

        public string TargetServer
        {
            get { return _databaseConfig.Url; }
        }

        public string TargetDatabase
        {
            get { return "Kafka"; }
        }

        public int QueueCount { get { return _queue.Sum(x => x.Length) + _failQueue.Sum(x => x.Item2.Length); } }

        protected virtual void OnSendBusinessEvent(SendCompleteEventArgs e)
        {
            var handler = SendCompleteEvent;
            if (handler != null) handler.Invoke(this, e);
        }
    }
}