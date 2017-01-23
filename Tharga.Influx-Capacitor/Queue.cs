using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;
using Tharga.InfluxCapacitor.QueueEvents;

namespace Tharga.InfluxCapacitor
{
    public class Queue : IQueue
    {
        private readonly object _syncRoot = new object();
        private readonly ISenderAgent _senderAgent;
        private readonly IQueueEvents _queueEvents;
        private readonly IQueueSettings _queueSettings;

        private bool _canSucceed; //Has successed to send at least once.

        private readonly Queue<Point[]> _queue = new Queue<Point[]>();
        private readonly Queue<Tuple<int, Point[]>> _failQueue = new Queue<Tuple<int, Point[]>>();
        private readonly QueueAction _queueAction;

        public Queue(ISenderAgent senderAgent)
            : this(senderAgent, new DropQueueEvents(), new QueueSettings(10, false, 20000))
        {
        }

        public Queue(ISenderAgent senderAgent, IQueueEvents queueEvents, IQueueSettings queueSettings)
        {
            queueEvents.DebugMessageEvent("Preparing new queue with target " + senderAgent.TargetDescription + ".");
            _queueAction = new QueueAction(queueEvents, GetQueueInfo);

            _senderAgent = senderAgent;
            _queueEvents = queueEvents;
            _queueSettings = queueSettings;

            if (queueSettings.FlushSecondsInterval > 0)
            {
                var timer = new Timer(queueSettings.FlushSecondsInterval * 1000);
                timer.Elapsed += Elapsed;
                timer.Start();
            }
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var response = Send();
                if (response.PointCount != 0)
                {
                    _queueEvents.TimerEvent(response);
                }
            }
            catch (Exception exception)
            {
                _queueEvents.ExceptionEvent(exception);
                throw;
            }
        }

        private SendResponse Send()
        {
            string responseMessage = null;
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var isSuccess = false;
            var pointCount = 0;

            lock (_syncRoot)
            {
                Point[] points = null;
                var retryCount = 0;
                try
                {
                    while (_queue.Count + _failQueue.Count > 0)
                    {
                        _queueAction.Execute(() =>
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
                        });

                        _queueEvents.DebugMessageEvent("Sending:" + Environment.NewLine + GetPointsString(points));
                        pointCount = points.Length;
                        var response = _senderAgent.SendAsync(points).Result;
                        if (response.IsSuccess)
                        {
                            _canSucceed = true;
                            isSuccess = true;
                            responseMessage = $"Sent {points.Length} points to server, with response '{response.StatusName}'.";
                            _queueEvents.SendEvent(new SendEventInfo(responseMessage, points.Length, SendEventInfo.OutputLevel.Information));
                        }
                        else
                        {
                            responseMessage = $"Failed to send {points.Length} points to server. Code '{response.StatusName}', Body '{response.Body ?? "n/a"}'.";
                            _queueEvents.SendEvent(new SendEventInfo(responseMessage, points.Length, SendEventInfo.OutputLevel.Error));
                            _queueAction.Execute(() => { _failQueue.Enqueue(new Tuple<int, Point[]>(retryCount, points)); });
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (points != null)
                    {
                        //var sb = new StringBuilder();
                        //sb.AppendLine(exception.Message);
                        ////sb.AppendLine(GetPointsString(points));
                        //_queueEvents.Message(sb.ToString(), QueueEventLevel.Error);
                        _queueEvents.ExceptionEvent(exception);
                    }
                    else
                    {
                        _queueEvents.ExceptionEvent(exception);
                    }

                    if (exception is AggregateException)
                    {
                        exception = exception.InnerException;
                    }

                    responseMessage = exception.Message;
                    _queueEvents.SendEvent(new SendEventInfo(exception));
                    if (points != null)
                    {
                        if (!_queueSettings.DropOnFail)
                        {
                            var invalidExceptionType = exception.IsExceptionValidForPutBack();

                            if (invalidExceptionType != null)
                            {
                                _queueEvents.SendEvent(new SendEventInfo(string.Format("Dropping {0} since the exception type {1} is not allowed for resend.", points.Length, invalidExceptionType), points.Length, SendEventInfo.OutputLevel.Warning));
                            }
                            else if (!_canSucceed)
                            {
                                _queueEvents.SendEvent(new SendEventInfo(string.Format("Dropping {0} points because there have never yet been a successful send.", points.Length), points.Length, SendEventInfo.OutputLevel.Warning));
                            }
                            else if (retryCount > 5)
                            {
                                _queueEvents.SendEvent(new SendEventInfo(string.Format("Dropping {0} points after {1} retries.", points.Length, retryCount), points.Length, SendEventInfo.OutputLevel.Warning));
                            }
                            else
                            {
                                _queueEvents.SendEvent(new SendEventInfo(string.Format("Putting {0} points back in the queue.", points.Length), points.Length, SendEventInfo.OutputLevel.Warning));
                                retryCount++;
                                _queueAction.Execute(() => { _failQueue.Enqueue(new Tuple<int, Point[]>(retryCount, points)); });
                            }
                        }
                        else
                        {
                            _queueEvents.SendEvent(new SendEventInfo(string.Format("Dropping {0} points.", points.Length), points.Length, SendEventInfo.OutputLevel.Warning));
                        }
                    }
                }
            }

            return new SendResponse(isSuccess, responseMessage, pointCount , stopWatch.Elapsed);
        }

        public IQueueCountInfo GetQueueInfo()
        {
            lock (_syncRoot)
            {
                return new QueueCountInfo(_queue.Sum(x => x.Length), _failQueue.Sum(x => x.Item2.Length));
            }
        }

        private string GetPointsString(Point[] points)
        {
            var sb = new StringBuilder();
            foreach (var point in points)
            {
                sb.AppendLine(_senderAgent.PointToString(point));
            }
            sb.AppendLine();
            return sb.ToString();
        }

        public void Enqueue(Point point)
        {
            Enqueue(new[] { point });
        }

        public void Enqueue(Point[] points)
        {
            lock (_syncRoot)
            {
                if (_queueSettings.MaxQueueSize - GetQueueInfo().TotalQueueCount < points.Length)
                {
                    _queueEvents.ExceptionEvent(new InvalidOperationException(string.Format("Queue will reach max limit, cannot add more points. Have {0} points, want to add {1} more. The limit is {2}.", GetQueueInfo().TotalQueueCount, points.Length, _queueSettings.MaxQueueSize)));
                    return;
                }

                _queueAction.Execute(() => { _queue.Enqueue(points); });
            }
        }

        private class QueueAction
        {
            private readonly IQueueEvents _queueEvents;
            private readonly Func<IQueueCountInfo> _getQueueInfo;

            public QueueAction(IQueueEvents queueEvents, Func<IQueueCountInfo> getQueueInfo)
            {
                _queueEvents = queueEvents;
                _getQueueInfo = getQueueInfo;
            }

            public void Execute(Action action)
            {
                var before = _getQueueInfo();
                action();
                _queueEvents.QueueChangedEvent(new QueueChangeEventInfo(before, _getQueueInfo()));
            }
        }
    }
}