using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Net;
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
                            OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, $"Sending {points.Length} points to server.", points.Length, SendCompleteEventArgs.OutputLevel.Information));
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
                                OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, $"Dropping {points.Length} since the exception type {invalidExceptionType} is not allowed for resend.", points.Length, SendCompleteEventArgs.OutputLevel.Warning));
                            }
                            else if (!_canSucceed)
                            {
                                OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, $"Dropping {points.Length} points because there have never yet been a successful send.", points.Length, SendCompleteEventArgs.OutputLevel.Warning));
                            }
                            else if (retryCount > 5)
                            {
                                OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, $"Dropping {points.Length} points after {retryCount} retries.", points.Length, SendCompleteEventArgs.OutputLevel.Warning));
                            }
                            else
                            {
                                OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, $"Putting {points.Length} points back in the queue.", points.Length, SendCompleteEventArgs.OutputLevel.Warning));
                                retryCount++;
                                _failQueue.Enqueue(new Tuple<int, Point[]>(retryCount, points));
                            }
                        }
                        else
                        {
                            OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, $"Dropping {points.Length} points.", points.Length, SendCompleteEventArgs.OutputLevel.Warning));
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
                    OnSendBusinessEvent(new SendCompleteEventArgs(_senderConfiguration, $"Queue will reach max limit, cannot add more points. Have {QueueCount} points, want to add {points.Length} more. The limit is {_senderConfiguration.MaxQueueSize}.", QueueCount, SendCompleteEventArgs.OutputLevel.Error));
                    return;
                }

                _queue.Enqueue(points);
            }
        }

        public string TargetServer => _senderConfiguration.Properties.Url;
        public string TargetDatabase => _senderConfiguration.Properties.DatabaseName;

        public int QueueCount { get { return _queue.Sum(x => x.Length) + _failQueue.Sum(x => x.Item2.Length); } }

        protected virtual void OnSendBusinessEvent(SendCompleteEventArgs e)
        {
            var handler = SendCompleteEvent;
            handler?.Invoke(this, e);
        }
    }
}