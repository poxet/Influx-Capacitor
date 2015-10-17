using System;
using System.Collections.Generic;
using System.Net.Http;
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
        private readonly int _maxQueueSize;
        private readonly int _maxSendBatchCount;
        private readonly bool _dropOnFail;
        private readonly object _syncRoot = new object();
        private readonly Queue<Point[]> _queue = new Queue<Point[]>();
        private readonly Queue<Tuple<int, Point[]>> _failQueue = new Queue<Tuple<int, Point[]>>();
        private readonly Lazy<IInfluxDbAgent> _client;
        public bool _canSucceed = false;

        public event EventHandler<SendBusinessEventArgs> SendBusinessEvent;

        public InfluxDataSender(IInfluxDbAgentLoader influxDbAgentLoader, IDatabaseConfig databaseConfig, int maxQueueSize)
        {
            _databaseConfig = databaseConfig;
            _maxQueueSize = maxQueueSize;
            _maxSendBatchCount = 1000;
            _dropOnFail = false;
            _client = new Lazy<IInfluxDbAgent>(() => influxDbAgentLoader.GetAgent(databaseConfig));
        }

        public Tuple<string, double?> Send()
        {
            string responseMessage = null;
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            lock (_syncRoot)
            {
                Point[] points = null;
                int retryCount = 0;
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

                        var client = _client.Value;
                        if (client != null)
                        {
                            //TODO: Possible to log what is sent. To an output file or similar.
                            var response = client.WriteAsync(points).Result;
                            _canSucceed = true;
                            OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Sending {0} points to server.", points.Length), points.Length, OutputLevel.Information));
                        }
                        else
                        {
                            responseMessage = "There is no client configured for sending data to the database, or the client has invalid settings.";
                            OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, responseMessage, points.Length, OutputLevel.Error));
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (exception is AggregateException)
                    {
                        exception = exception.InnerException;
                        //    if (exception.InnerException is InfluxDbApiException)
                        //    {
                        //        if (exception.InnerException.Message.Contains("input field \"value\" on measurement \"Influx-Capacitor\" is type float64, already exists as type integer"))
                        //        {
                        //            //DROP MEASUREMENT "???"
                        //        }
                        //    }
                    }

                    responseMessage = exception.Message;
                    OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, exception));
                    if (points != null)
                    {
                        if (!_dropOnFail)
                        {
                            var invalidExceptionType = IsExceptionValidForPutBack(exception);

                            if (invalidExceptionType != null)
                            {
                                OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Dropping {0} since the exception type {1} is not allowed for resend.", points.Length, invalidExceptionType), points.Length, OutputLevel.Warning));
                            }
                            else if (!_canSucceed)
                            {
                                OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Dropping {0} points because there have never yet been a successful send.", points.Length), points.Length, OutputLevel.Warning));
                            }
                            else if (retryCount > 5)
                            {
                                OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Dropping {0} points after {1} retries.", points.Length, retryCount), points.Length, OutputLevel.Warning));
                            }
                            else
                            {
                                OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Putting {0} points back in the queue.", points.Length), points.Length, OutputLevel.Warning));
                                retryCount++;
                                _failQueue.Enqueue(new Tuple<int, Point[]>(retryCount, points));
                            }
                        }
                        else
                        {
                            OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Dropping {0} points.", points.Length), points.Length, OutputLevel.Warning));
                        }
                    }
                }
            }

            return new Tuple<string, double?>(responseMessage, stopWatch.Elapsed.TotalMilliseconds);
        }

        private string IsExceptionValidForPutBack(Exception exception)
        {
            var exceptionToUse = exception;
            //var exceptionToUse = GetInnerMostException(exception);
            //else
            //{
            //    var agg = exceptionToUse as AggregateException;
            //    if (agg != null)
            //    {
            //        exceptionToUse = agg.InnerException;
            //    }
            //    //return exception.GetType().ToString();
            //}

            //Allowed request types returns null
            if (exceptionToUse is HttpRequestException)
            {
                return null;
            }

            return exceptionToUse.GetType().ToString();
        }

        private static Exception GetInnerMostException(Exception exception)
        {
            var inner = exception;
            while (inner.InnerException != null)
            {
                inner = inner.InnerException;
            }

            return inner;
        }

        public void Enqueue(Point[] points)
        {
            lock (_syncRoot)
            {
                if (_maxQueueSize - QueueCount < points.Length)
                {
                    OnSendBusinessEvent(new SendBusinessEventArgs(_databaseConfig, string.Format("Queue will reach max limit, cannot add more points. Have {0} points, want to add {1} more. The limit is {2}.", QueueCount, points.Length, _maxQueueSize), QueueCount, OutputLevel.Error));
                    return;
                }
                
                _queue.Enqueue(points);
            }
        }

        public string TargetServer { get { return _databaseConfig.Url; } }
        public string TargetDatabase { get { return _databaseConfig.Name; } }
        public int QueueCount { get { return _queue.Count + _failQueue.Count; } }

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