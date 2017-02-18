using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Business;
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
        private readonly IMetaDataBusiness _metaDataBusiness;
        private readonly IQueueSettings _queueSettings;
        private readonly PointValidator _pointValidator;

        private bool _canSucceed; //Has successed to send at least once.

        private readonly Queue<Point[]> _queue = new Queue<Point[]>();
        private readonly Queue<RetryPoint> _failQueue = new Queue<RetryPoint>();
        private readonly QueueAction _queueAction;
        private bool _singlePointStream = true;
        private Timer _timer;

        public Queue(ISenderAgent senderAgent)
            : this(senderAgent, new DropQueueEvents(), new MetaDataBusiness(), new QueueSettings())
        {
        }

        public Queue(ISenderAgent senderAgent, IQueueEvents queueEvents, IMetaDataBusiness metaDataBusiness, IQueueSettings queueSettings)
        {
            _pointValidator = new PointValidator();
            queueEvents.OnDebugMessageEvent($"Preparing new queue with target {senderAgent.TargetDescription}.");
            _queueAction = new QueueAction(queueEvents, GetQueueInfo);

            _senderAgent = senderAgent;
            _queueEvents = queueEvents;
            _metaDataBusiness = metaDataBusiness;
            _queueSettings = queueSettings;
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var response = Send();
                if (response.PointCount != 0)
                {
                    _queueEvents.OnTimerEvent(response);
                }

                if (_queueSettings.Metadata && response.PointCount > 1)
                {
                    var metaPoint = _metaDataBusiness.BuildQueueMetadata("send", response, _senderAgent, GetQueueInfo());
                    EnqueueEx(new[] { metaPoint });
                }
            }
            catch (Exception exception)
            {
                _queueEvents.OnExceptionEvent(exception);
                throw;
            }
        }

        private SendResponse Send()
        {
            var responseMessage = new Tuple<bool, string>(true, string.Empty);
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var pointCount = 0;

            lock (_syncRoot)
            {
                //Point[] points = null;
                try
                {
                    //Send points added to the latest batch
                    while (_queue.Count > 0)
                    {
                    Point[] points = null;
                    _queueAction.Execute(() =>
                        {
                            if (_queue.Count > 0)
                            {
                                var pts = new List<Point>();
                                while (_queue.Count > 0)
                                {
                                    pts.AddRange(_queue.Dequeue());
                                }
                                points = pts.ToArray();
                            }
                        });

                        _queueEvents.OnDebugMessageEvent($"Sending:{Environment.NewLine}{GetPointsString(points)}");
                        pointCount = points.Length;
                        responseMessage = SendPointsNow(points, 0);
                    }

                    //Try to resend items from the fail quee, one by one
                    if (responseMessage.Item1)
                    {
                        while (_failQueue.Count > 0)
                        {
                            var failPoint = _failQueue.Dequeue();

                            SendPointsNow(new[] { failPoint.Point }, failPoint.RetryCount);
                            //if (!r.Item1)
                            //{
                            //    _failQueue.Enqueue(new RetryPoint(failPoint.RetryCount + 1, failPoint.Point));
                            //}
                        }
                    }
                }
                catch (Exception exception)
                {
                //    if (points != null)
                //    {
                //        _queueEvents.OnExceptionEvent(exception);
                //    }
                //    else
                //    {
                //        _queueEvents.OnExceptionEvent(exception);
                //    }
                //

                    responseMessage = new Tuple<bool, string>(false, exception?.Message ?? "Unknown");
                    _queueEvents.OnSendEvent(new SendEventInfo(exception));
                //    if (points != null)
                //    {
                //        var sb = new StringBuilder();
                //        foreach (var point in points)
                //        {
                //            sb.AppendLine(_senderAgent.PointToString(point));
                //        }
                //
                //        if (!_queueSettings.DropOnFail)
                //        {
                //            var invalidExceptionType = exception.IsExceptionValidForPutBack();
                //
                //            if (invalidExceptionType != null)
                //            {
                //                _queueEvents.OnSendEvent(new SendEventInfo($"Dropping {points.Length} since the exception type {invalidExceptionType} is not allowed for resend. {sb}", points.Length, SendEventInfo.OutputLevel.Warning));
                //            }
                //            else if (!_canSucceed)
                //            {
                //                _queueEvents.OnSendEvent(new SendEventInfo($"Dropping {points.Length} points because there have never yet been a successful send. {sb}", points.Length, SendEventInfo.OutputLevel.Warning));
                //            }
                //            //else if (retryCount > 5)
                //            //{
                //            //    _queueEvents.OnSendEvent(new SendEventInfo($"Dropping {points.Length} points after {retryCount} retries. {sb}", points.Length, SendEventInfo.OutputLevel.Warning));
                //            //}
                //            //else
                //            //{
                //            //    _queueEvents.OnSendEvent(new SendEventInfo($"Putting {points.Length} points back in the queue. {sb}", points.Length, SendEventInfo.OutputLevel.Warning));
                //            //    retryCount++;
                //            //    _queueAction.Execute(() => { _failQueue.Enqueue(new Tuple<int, Point[]>(retryCount, points)); });
                //            //}
                //        }
                //        else
                //        {
                //            _queueEvents.OnSendEvent(new SendEventInfo($"Dropping {points.Length} points {sb}.", points.Length, SendEventInfo.OutputLevel.Warning));
                //        }
                //    }
                }
            }

            return new SendResponse(responseMessage.Item1, responseMessage.Item2, pointCount, stopWatch.Elapsed);
        }

        private Tuple<bool, string> SendPointsNow(Point[] points, int retryCount)
        {
            try
            {
                bool isSuccess = false;
                string responseMessage;
                var response = _senderAgent.SendAsync(points).Result;
                if (response.IsSuccess)
                {
                    _canSucceed = true;
                    isSuccess = true;
                    responseMessage = $"Sent {points.Length} points to server, with response '{response.StatusName}'.";
                    _queueEvents.OnSendEvent(new SendEventInfo(responseMessage, points.Length, SendEventInfo.OutputLevel.Information));
                }
                else
                {
                    responseMessage = $"Failed to send {points.Length} points to server. Code '{response.StatusName}', Body '{response.Body ?? "n/a"}'.";
                    _queueEvents.OnSendEvent(new SendEventInfo(responseMessage, points.Length, SendEventInfo.OutputLevel.Error));
                    ReQueue(points, retryCount, null);
                }

                return new Tuple<bool, string>(isSuccess, responseMessage);
            }
            catch (Exception exception)
            {
                if (exception is AggregateException)
                {
                    exception = exception.InnerException;
                }

                ReQueue(points, retryCount, exception);

                return new Tuple<bool, string>(false, exception?.Message ?? "Unknown");
            }
        }

        private void ReQueue(Point[] points, int retryCount, Exception exception)
        {
            var invalidExceptionType = exception?.IsExceptionValidForPutBack();

            _queueAction.Execute(() =>
            {
                if (invalidExceptionType != null)
                {
                    _queueEvents.OnSendEvent(new SendEventInfo($"Dropping {points.Length} since the exception type {invalidExceptionType} is not allowed for resend.", points.Length, SendEventInfo.OutputLevel.Warning));
                }
                if (_canSucceed)
                {
                    foreach (var point in points)
                    {
                        if (retryCount < 5)
                        {
                            _failQueue.Enqueue(new RetryPoint(retryCount + 1, point));
                        }
                        else
                        {
                            _queueEvents.OnSendEvent(new SendEventInfo($"Dropping {points.Length} points after {retryCount} retries.", points.Length, SendEventInfo.OutputLevel.Warning));
                        }
                    }
                }
                else
                {
                    _queueEvents.OnSendEvent(new SendEventInfo($"Dropping {points.Length} points because there have never yet been a successful send.", points.Length, SendEventInfo.OutputLevel.Warning));
                }
            });
        }

        public IQueueCountInfo GetQueueInfo()
        {
            lock (_syncRoot)
            {
                return new QueueCountInfo(_queue.Sum(x => x.Length), _failQueue.Count);
            }
        }

        public IEnumerable<Point> Items
        {
            get { return _queue.SelectMany(x => x); }
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
            var response = EnqueueEx(points);

            if (_queueSettings.Metadata)
            {
                var metaPoint = _metaDataBusiness.BuildQueueMetadata("enqueue", response, _senderAgent, GetQueueInfo());
                EnqueueEx(new[] { metaPoint });
            }
        }

        private ISendResponse EnqueueEx(Point[] points)
        {
            var validPoints = new Point[] { };
            var stopwatch = new Stopwatch();
            var success = true;
            var message = string.Empty;

            try
            {
                stopwatch.Start();

                lock (_syncRoot)
                {
                    if (_queueSettings.MaxQueueSize - GetQueueInfo().TotalQueueCount < points.Length)
                    {
                        message = $"Queue will reach max limit, cannot add more points. Have {GetQueueInfo().TotalQueueCount} points, want to add {points.Length} more. The limit is {_queueSettings.MaxQueueSize}.";
                        _queueEvents.OnExceptionEvent(new InvalidOperationException(message));
                        success = false;
                    }
                    else
                    {
                        validPoints = _pointValidator.Clean(points).ToArray();
                        _queueAction.Execute(() => { _queue.Enqueue(validPoints); });
                        message = string.Join(", ", _pointValidator.Validate(points).ToArray());
                    }
                }
            }
            catch (Exception exception)
            {
                message = exception.Message;
                success = false;
            }
            finally
            {
                stopwatch.Stop();
                _queueEvents.OnEnqueueEvent(validPoints, points, _pointValidator.Validate(points).ToArray());

                if (_queueSettings.FlushSecondsInterval > 0 && _timer == null)
                {
                    _timer = new Timer(_queueSettings.FlushSecondsInterval * 1000);
                    _timer.Elapsed += Elapsed;
                    _timer.Start();
                }
            }

            return new SendResponse(success, message, validPoints.Length, stopwatch.Elapsed);
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
                _queueEvents.OnQueueChangedEvent(new QueueChangeEventInfo(before, _getQueueInfo()));
            }
        }
    }
}