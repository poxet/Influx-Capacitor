using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor
{
    public class Queue : IQueue
    {
        private readonly object _syncRoot = new object();
        private readonly ISenderAgent _senderAgent;
        private readonly IQueueEvents _queueEvents;
        private readonly IQueueSettings _queueSettings;

        private bool _canSucceed; //Has successed to send at least once.

        //private ILogger _logger;
        ////private const string MutexId = "InfluxQueue";
        ////private static readonly IInfluxDbAgent _agent;
        ////private static readonly IFormatter _formatter;
        private readonly Queue<Point[]> _queue = new Queue<Point[]>();
        private readonly Queue<Tuple<int, Point[]>> _failQueue = new Queue<Tuple<int, Point[]>>();
        private readonly QueueAction _queueAction;
        ////private static readonly MyLogger _logger = new MyLogger();
        ////private static Timer _sendTimer;
        ////private static MutexSecurity _securitySettings;
        ////private static bool? _enabled;
        //private readonly bool _metadata;

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

            //    //try
            //    //{
            //    //    if (Enabled)
            //    //    {
            //    //        var influxVersion = InfluxVersion.Auto; //TODO: Move to settings
            //    //        _logger.Info(string.Format("Initiating influxdb agent to address {0} database {1} user {2} version {3}.",Address, DatabaseName, UserName, influxVersion));
            //    //        _agent = new InfluxDbAgent(Address, DatabaseName, UserName, Password, null, influxVersion);
            //    //        _formatter = _agent.GetAgentInfo().Item1;
            //    //    }
            //    //}
            //    //catch(Exception exception)
            //    //{
            //    //    _logger.Error(exception);
            //    //    _enabled = false;
            //    //}
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //var metaPoints = new List<Point>();

                //var previousQueueCount = TotalQueueCount;
                //_queueEvents.DebugMessageEvent(string.Format("Starting to send {0} points to {1}.", previousQueueCount, _senderAgent.TargetDescription));
                var success = Send();
                //var postQueueCount = TotalQueueCount;
                //_queueEvents.DebugMessageEvent(string.Format("Done sending {0} points to server {1}. Now {2} items in queue.", previousQueueCount, _senderAgent.TargetDescription, postQueueCount));

                //if (_metadata)
                //{
                //    metaPoints.Add(MetaDataBusiness.GetQueueCountPoints("Send", senderAgent.TargetServer, senderAgent.TargetDatabase, previousQueueCount, postQueueCount - previousQueueCount + _dataSenders.Count, success));
                //}
                //
                //if (_metadata)
                //{
                //    foreach (var dataSender in _dataSenders)
                //    {
                //        dataSender.Enqueue(metaPoints.ToArray());
                //    }
                //}
            }
            catch (Exception exception)
            {
                _queueEvents.ExceptionEvent(exception);
                throw;
            }
        }

        //public int TotalQueueCount
        //{
        //    get
        //    {
        //        lock (_syncRoot)
        //        {
        //            return _queue.Sum(x => x.Length) + _failQueue.Sum(x => x.Item2.Length);
        //        }
        //    }
        //}
        //
        //public int QueueCount
        //{
        //    get
        //    {
        //        lock (_syncRoot)
        //        {
        //            return _queue.Sum(x => x.Length);
        //        }
        //    }
        //}
        //
        //public int FailQueueCount
        //{
        //    get
        //    {
        //        lock (_syncRoot)
        //        {
        //            return _failQueue.Sum(x => x.Item2.Length);
        //        }
        //    }
        //}

        private SendResponse Send()
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
                        var response = _senderAgent.SendAsync(points).Result;
                        if (response.IsSuccess)
                        {
                            _canSucceed = true;
                            _queueEvents.SendEvent(new SendEventInfo($"Sent {points.Length} points to server, with response '{response.StatusName}'.", points.Length, SendEventInfo.OutputLevel.Information));
                        }
                        else
                        {
                            _queueEvents.SendEvent(new SendEventInfo($"Failed to send {points.Length} points to server. Code '{response.StatusName}', Body '{response.Body ?? "n/a"}'.", points.Length, SendEventInfo.OutputLevel.Error));
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

            return new SendResponse(responseMessage, stopWatch.Elapsed);
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

        //private static QueueAction

        ////private static string Address
        ////{
        ////    get
        ////    {
        ////        var influxDbAddress = ConfigurationManager.AppSettings["InfluxDbAddress"];
        ////        if (influxDbAddress == null) throw new ConfigurationErrorsException("No InfluxDbAddress configured.");
        ////        return influxDbAddress;
        ////    }
        ////}
        ////
        ////private static string DatabaseName
        ////{
        ////    get
        ////    {
        ////        var databaseName = ConfigurationManager.AppSettings["InfluxDbName"];
        ////        if (databaseName == null) throw new ConfigurationErrorsException("No InfluxDbName configured.");
        ////        return databaseName;
        ////    }
        ////}
        ////
        ////private static string UserName
        ////{
        ////    get
        ////    {
        ////        var influxDbUserName = ConfigurationManager.AppSettings["InfluxDbUserName"];
        ////        if (influxDbUserName == null) throw new ConfigurationErrorsException("No InfluxDbUserName configured.");
        ////        return influxDbUserName;
        ////    }
        ////}
        ////
        ////private static string Password
        ////{
        ////    get
        ////    {
        ////        var influxDbPassword = ConfigurationManager.AppSettings["InfluxDbPassword"];
        ////        if (influxDbPassword == null) throw new ConfigurationErrorsException("No InfluxDbPassword configured.");
        ////        return influxDbPassword;
        ////    }
        ////}

        ////private static bool Enabled
        ////{
        ////    get
        ////    {
        ////        if (_enabled == null)
        ////        {
        ////            var enabledString = ConfigurationManager.AppSettings["InfluxDbEnabled"];
        ////
        ////            bool enabled;
        ////            if (!bool.TryParse(enabledString, out enabled))
        ////                enabled = true;
        ////
        ////            _enabled = enabled;
        ////        }
        ////
        ////        return _enabled ?? true;
        ////    }
        ////}

        //private static async void SendTimerElapsed(object sender, ElapsedEventArgs e)
        //{
        //    ////_logger.Debug("SendTimerElapsed.");
        //    //
        //    //var pts = new List<Point>();
        //    //InfluxDbApiResponse result = null;
        //    //bool createdNew;
        //    //using (var mutex = new Mutex(false, MutexId, out createdNew, _securitySettings))
        //    //{
        //    //    mutex.WaitOne();
        //    //    while (_queue.Count > 0)
        //    //    {
        //    //        var points = _queue.Dequeue();
        //    //        pts.AddRange(points);
        //    //    }
        //    //    mutex.ReleaseMutex();
        //    //}
        //    //
        //    //if (pts.Count == 0)
        //    //{
        //    //    //_logger.Debug("Nothing to send.");
        //    //    return;
        //    //}
        //    //
        //    //try
        //    //{
        //    //    _logger.Debug(string.Format("Sending {0} measurements.", pts.Count + 1));
        //    //    var data = new StringBuilder();
        //    //    foreach (var item in pts)
        //    //    {
        //    //        data.AppendLine(_formatter.PointToString(item));
        //    //    }
        //    //    _logger.Debug(data.ToString());
        //    //
        //    //    result = await _agent.WriteAsync(pts.ToArray());
        //    //    _logger.Info(result.StatusCode + ": " + result.Body);
        //    //}
        //    //catch (Exception exception)
        //    //{
        //    //    _logger.Error(exception);
        //    //    //TODO: Only re-enqueue points in certain situations
        //    //    //_queue.Enqueue(pts.ToArray());
        //    //}
        //}

        //public static void Enqueue(Point point)
        //{
        //    //if (!Enabled)
        //    //{
        //    //    _logger.Debug("Ignoreing enqueue becuase the queue is disabled.");
        //    //    return;
        //    //}

        //    //bool createdNew;
        //    //using (var mutex = new Mutex(false, MutexId, out createdNew, _securitySettings))
        //    //{
        //    //    try
        //    //    {
        //    //        mutex.WaitOne();

        //    //        _logger.Debug(string.Format("Enqueue measurement. There will be {0} items in the queue.", _queue.Count + 1));
        //    //        _queue.Enqueue(new[] { point });

        //    //        if (_sendTimer != null)
        //    //            return;

        //    //        if (_sendTimer == null)
        //    //        {
        //    //            _sendTimer = new Timer();
        //    //            _sendTimer.Interval = 10000; //TODO: Move to settings
        //    //            _sendTimer.Elapsed += SendTimerElapsed;
        //    //            _sendTimer.Start();
        //    //        }
        //    //    }
        //    //    finally
        //    //    {
        //    //        mutex.ReleaseMutex();
        //    //    }
        //    //}
        //}
    }
}