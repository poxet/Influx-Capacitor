using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector
{
    internal class CollectorEngine
    {
        public event EventHandler<NotificationEventArgs> NotificationEvent;

        private readonly IPerformanceCounterGroup _performanceCounterGroup;
        private readonly ISendBusiness _sendBusiness;
        private readonly Timer _timer;
        private readonly string _name;
        private StopwatchHighPrecision _sw;
        private DateTime? _timestamp;
        private long _counter;

        public CollectorEngine(IPerformanceCounterGroup performanceCounterGroup, ISendBusiness sendBusiness)
        {
            _performanceCounterGroup = performanceCounterGroup;
            _sendBusiness = sendBusiness;
            if (performanceCounterGroup.SecondsInterval > 0)
            {
                _timer = new Timer(1000 * performanceCounterGroup.SecondsInterval);
                _timer.Elapsed += Elapsed;
            }
            _name = _performanceCounterGroup.Name;
        }

        private async void Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await RegisterCounterValuesAsync();
            }
            catch (Exception exception)
            {
                InvokeNotificationEvent(new NotificationEventArgs(exception.Message, OutputLevel.Error));
            }
        }

        private static DateTime Floor(DateTime dateTime, TimeSpan interval)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % interval.Ticks));
        }

        public async Task<int> RegisterCounterValuesAsync()
        {
            try
            {
                if (_timestamp == null)
                {
                    _sw = new StopwatchHighPrecision();
                    _timestamp = DateTime.UtcNow;
                    var nw = Floor(_timestamp.Value, new TimeSpan(0, 0, 0, 1));
                    //Debug.WriteLine("ROUND: " + (nw - _timestamp.Value).TotalMilliseconds);
                    _timestamp = nw;
                }
                else
                {
                    var elapsedTotal = _sw.ElapsedTotal;

                    var timeOffset = new TimeSpan(elapsedTotal).TotalSeconds - _performanceCounterGroup.SecondsInterval * _counter;

                    Debug.WriteLine("TErr: " + timeOffset);
                    if (timeOffset > 1)
                    {
                        _counter = _counter + 1 + (int)timeOffset;
                        Debug.WriteLine("Dropping " + (int)timeOffset + " steps.");
                        return -2;
                    }
                    if (timeOffset < -1)
                    {
                        Debug.WriteLine("Jumping 1 step.");
                        return -3;
                    }

                    //Adjust interval
                    var next = 1000 * (_performanceCounterGroup.SecondsInterval - timeOffset);
                    if (next > 0)
                    {
                        _timer.Interval = next;
                        Debug.WriteLine("Interval: " + next);
                    }
                }
                var timestamp = _timestamp.Value.AddSeconds(_performanceCounterGroup.SecondsInterval * _counter);
                Debug.WriteLine("Using: " + timestamp.ToString("h:mm:ss tt"));
                _counter++;

                var swMain = new StopwatchHighPrecision();

                var precision = TimeUnit.Seconds; //TimeUnit.Microseconds;

                //Prepare read
                var performanceCounterInfos = _performanceCounterGroup.PerformanceCounterInfos.Where(x => x.PerformanceCounter != null).ToArray();
                var values = new float[performanceCounterInfos.Length];

                var prepare = swMain.ElapsedSegment;

                //Perform Read (This should be as fast and short as possible)
                for (var i = 0; i < values.Count(); i++)
                {
                    values[i] = performanceCounterInfos[i].PerformanceCounter.NextValue();
                }

                var read = swMain.ElapsedSegment;
                var readSpan = (float)new TimeSpan(read).TotalMilliseconds;

                //Prepare result
                var points = new Point[performanceCounterInfos.Length];
                for (var i = 0; i < values.Count(); i++)
                {
                    var performanceCounterInfo = performanceCounterInfos[i];
                    var value = values[i];

                    var categoryName = performanceCounterInfo.PerformanceCounter.CategoryName;
                    var counterName = performanceCounterInfo.PerformanceCounter.CounterName;
                    var key = performanceCounterInfo.PerformanceCounter.InstanceName;

                    var point = new Point
                                    {
                                        Name = _name,
                                        Tags = new Dictionary<string, object>
                                                   {
                                                       { "hostname", Environment.MachineName },
                                                       { "category", categoryName },
                                                       { "counter", counterName },
                                                   },
                                        Fields = new Dictionary<string, object>
                                                     {
                                                         { "value", value },
                                                         //{ "readSpan", readSpan }, //Time in ms from the first, to the lats counter read in the group.
                                                         //{ "timeOffset", (float)(timeOffset * 1000) } //Time difference in ms from reported time, to when read actually started.
                                                     },
                                        Precision = precision,
                                        Timestamp = timestamp
                                    };

                    if (!string.IsNullOrEmpty(key))
                    {
                        point.Tags.Add("instance", key);
                    }

                    points[i] = point;
                }

                var format = swMain.ElapsedSegment;

                //Queue result
                _sendBusiness.Enqueue(points);

                var enque = swMain.ElapsedSegment;

                Debug.WriteLine(string.Format("Prepare: {0}, Read: {1} ms, Format: {2}, Enque: {3}, counters: {4}, ReadSpan: {5}", new TimeSpan(prepare).TotalMilliseconds, new TimeSpan(read).TotalMilliseconds, new TimeSpan(format).TotalMilliseconds, new TimeSpan(enque).TotalMilliseconds, points.Length, readSpan));

                return points.Length;
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry(Constants.ServiceName, exception.Message, EventLogEntryType.Error);
                return -1;
            }
        }

        public async Task StartAsync()
        {
            if (_timer == null) return;
            InvokeNotificationEvent(new NotificationEventArgs(string.Format("Started collector engine {0}.", _name), OutputLevel.Information));
            await RegisterCounterValuesAsync();
            _timer.Start();
        }

        private void InvokeNotificationEvent(NotificationEventArgs e)
        {
            var handler = NotificationEvent;
            if (handler != null) handler(this, e);
        }
    }
}