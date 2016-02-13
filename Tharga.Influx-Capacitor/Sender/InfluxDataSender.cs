using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Net.Helpers;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Agents;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Sender
{
    public class InfluxDataSender : IDataSender
    {
        private readonly object _syncRoot = new object();
        private readonly ISenderConfiguration _senderConfiguration;
        private readonly Queue<Point[]> _queue = new Queue<Point[]>();
        private readonly Queue<Tuple<int, Point[]>> _failQueue = new Queue<Tuple<int, Point[]>>();
        private readonly Lazy<IInfluxDbAgent> _client;
        private readonly bool _dropOnFail;
        private bool _canSucceed;

        public event EventHandler<SendCompleteEventArgs> SendCompleteEvent;

        public InfluxDataSender(ISenderConfiguration senderConfiguration)
        {
            _senderConfiguration = senderConfiguration;
            _dropOnFail = false;
            _client = new Lazy<IInfluxDbAgent>(() => new InfluxDbAgent(senderConfiguration.Properties.Url, senderConfiguration.Properties.DatabaseName, senderConfiguration.Properties.UserName, senderConfiguration.Properties.Password));
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

                        var client = _client.Value;
                        if (client != null)
                        {
                            //TODO: Possible to log what is sent. To an output file or similar.
                            var response = client.WriteAsync(points).Result;
                            _canSucceed = true;
                            OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, string.Format("Sending {0} points to server.", points.Length), points.Length, SendCompleteEventArgs.OutputLevel.Information));
                        }
                        else
                        {
                            responseMessage = "There is no client configured for sending data to the database, or the client has invalid settings.";
                            OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, responseMessage, points.Length, SendCompleteEventArgs.OutputLevel.Error));
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (exception is AggregateException)
                    {
                        exception = exception.InnerException;
                    }

                    responseMessage = exception.Message;
                    OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, exception));
                    if (points != null)
                    {
                        if (!_dropOnFail)
                        {
                            var invalidExceptionType = exception.IsExceptionValidForPutBack();

                            if (invalidExceptionType != null)
                            {
                                OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, String.Format("Dropping {0} since the exception type {1} is not allowed for resend.", points.Length, invalidExceptionType), points.Length, SendCompleteEventArgs.OutputLevel.Warning));
                            }
                            else if (!_canSucceed)
                            {
                                OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, String.Format("Dropping {0} points because there have never yet been a successful send.", points.Length), points.Length, SendCompleteEventArgs.OutputLevel.Warning));
                            }
                            else if (retryCount > 5)
                            {
                                OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, String.Format("Dropping {0} points after {1} retries.", points.Length, retryCount), points.Length, SendCompleteEventArgs.OutputLevel.Warning));
                            }
                            else
                            {
                                OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, String.Format("Putting {0} points back in the queue.", points.Length), points.Length, SendCompleteEventArgs.OutputLevel.Warning));
                                retryCount++;
                                _failQueue.Enqueue(new Tuple<int, Point[]>(retryCount, points));
                            }
                        }
                        else
                        {
                            OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, String.Format("Dropping {0} points.", points.Length), points.Length, SendCompleteEventArgs.OutputLevel.Warning));
                        }
                    }
                }
            }

            return new SendResponse(responseMessage, stopWatch.Elapsed.TotalMilliseconds);
        }

        public void Enqueue(Point[] points)
        {
            lock (_syncRoot)
            {
                if (_senderConfiguration.MaxQueueSize - QueueCount < points.Length)
                {
                    OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, String.Format("Queue will reach max limit, cannot add more points. Have {0} points, want to add {1} more. The limit is {2}.", QueueCount, points.Length, _senderConfiguration.MaxQueueSize), QueueCount, SendCompleteEventArgs.OutputLevel.Error));
                    return;
                }

                _queue.Enqueue(points);
            }
        }

        public string TargetServer
        {
            get { return _senderConfiguration.Properties.Url; }
        }

        public string TargetDatabase
        {
            get { return _senderConfiguration.Properties.DatabaseName; }
        }

        public int QueueCount { get { return _queue.Sum(x => x.Length) + _failQueue.Sum(x => x.Item2.Length); } }

        protected virtual void OnSendBusinessEvent(SendCompleteEventArgs e)
        {
            var handler = SendCompleteEvent;
            if (handler != null) handler.Invoke(this, e);
        }
    }
}