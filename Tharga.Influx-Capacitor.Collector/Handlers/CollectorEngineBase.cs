using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Handlers
{
    public abstract class CollectorEngineBase : ICollectorEngine, IDisposable
    {
        private readonly IPerformanceCounterGroup _performanceCounterGroup;
        private readonly ISendBusiness _sendBusiness;
        private readonly string _name;
        private readonly ITag[] _tags;
        private int _refreshCountdown = 0;
        private readonly Timer _timer;

        //TODO: Cleanup
        protected DateTime? _timestamp; //TODO: Rename to first Read

        protected CollectorEngineBase(IPerformanceCounterGroup performanceCounterGroup, ISendBusiness sendBusiness, ITagLoader tagLoader)
        {
            _performanceCounterGroup = performanceCounterGroup;
            _name = _performanceCounterGroup.Name;
            _sendBusiness = sendBusiness;
            _tags = tagLoader.GetGlobalTags().Union(_performanceCounterGroup.Tags).ToArray();

            if (performanceCounterGroup.SecondsInterval > 0)
            {
                _timer = new Timer(1000 * performanceCounterGroup.SecondsInterval);
                _timer.Elapsed += Elapsed;
            }
        }

        public event EventHandler<CollectRegisterCounterValuesEventArgs> CollectRegisterCounterValuesEvent;

        protected int SecondsInterval
        {
            get
            {
                return _performanceCounterGroup.SecondsInterval;
            }
        }

        protected string Name
        {
            get
            {
                return _name;
            }
        }

        protected ITag[] Tags
        {
            get
            {
                return _tags;
            }
        }

        public async Task StartAsync()
        {
            if (_timer == null) return;
            await CollectRegisterCounterValuesAsync();
            _timer.Start();
        }

        public void StopTimer()
        {
            _timer.Stop();
        }

        protected void ResumeTimer()
        {
            _timer.Start();
        }

        public abstract Task<int> CollectRegisterCounterValuesAsync();

        private async void Elapsed(object sender, ElapsedEventArgs e)
        {
            await CollectRegisterCounterValuesAsync();
        }

        protected void Enqueue(Point[] points)
        {
            _sendBusiness.Enqueue(points);
        }

        protected IPerformanceCounterInfo[] PrepareCounters()
        {
            if (_refreshCountdown == 0)
            {
                _refreshCountdown = _performanceCounterGroup.RefreshInstanceInterval - 1;
                return _performanceCounterGroup.GetFreshCounters().ToArray();
            }

            _refreshCountdown--;

            if (_refreshCountdown < 0)
                _refreshCountdown = _performanceCounterGroup.RefreshInstanceInterval - 1;

            return _performanceCounterGroup.GetCounters().ToArray();
        }

        protected void RemoveObsoleteCounters(float?[] values, IPerformanceCounterInfo[] performanceCounterInfos)
        {
            if (!values.Any(x => !x.HasValue))
                return;

            for (var i = 0; i < values.Count(); i++)
            {
                _performanceCounterGroup.RemoveCounter(performanceCounterInfos[i]);
            }

            Trace.TraceInformation("Removed {0} counters.", values.Count());
        }

        protected void SetTimerInterval(double interval)
        {
            _timer.Interval = interval;
        }

        protected void OnCollectRegisterCounterValuesEvent(CollectRegisterCounterValuesEventArgs e)
        {
            var handler = CollectRegisterCounterValuesEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected static DateTime Floor(DateTime dateTime, TimeSpan interval)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % interval.Ticks));
        }

        protected static float?[] ReadValues(IPerformanceCounterInfo[] performanceCounterInfos)
        {
            var values = new float?[performanceCounterInfos.Length];
            for (var i = 0; i < values.Count(); i++)
            {
                try
                {
                    values[i] = performanceCounterInfos[i].PerformanceCounter.NextValue();
                }
                catch (InvalidOperationException)
                {
                    values[i] = null;
                }
            }
            return values;
        }

        protected IEnumerable<Point> FormatResult(IPerformanceCounterInfo[] performanceCounterInfos, float?[] values, TimeUnit precision, DateTime timestamp)
        {
            for (var i = 0; i < values.Count(); i++)
            {
                var value = values[i];
                if (value != null)
                {
                    var performanceCounterInfo = performanceCounterInfos[i];

                    var categoryName = performanceCounterInfo.PerformanceCounter.CategoryName;
                    var counterName = performanceCounterInfo.PerformanceCounter.CounterName;
                    var key = performanceCounterInfo.PerformanceCounter.InstanceName;
                    var instanceAlias = performanceCounterInfo.Alias;
                    var tags = GetTags(Tags, categoryName, counterName);
                    var fields = new Dictionary<string, object>
                                     {
                                         { "value", value },
                                         //{ "readSpan", readSpan }, //Time in ms from the first, to the lats counter read in the group.
                                         //{ "timeOffset", (float)(timeOffset * 1000) } //Time difference in ms from reported time, to when read actually started.
                                     };

                    var point = new Point
                                    {
                                        Name = Name,
                                        Tags = tags,
                                        Fields = fields,
                                        Precision = precision,
                                        Timestamp = timestamp
                                    };

                    if (!string.IsNullOrEmpty(key))
                    {
                        point.Tags.Add("instance", key);
                        if (!string.IsNullOrEmpty(instanceAlias))
                        {
                            point.Tags.Add(instanceAlias, key);
                        }
                    }

                    yield return point;
                }
            }
        }
        private static Dictionary<string, string> GetTags(IEnumerable<ITag> globalTags, string categoryName, string counterName)
        {
            var dictionary = new Dictionary<string, string>
                                 {
                                     { "hostname", Environment.MachineName },
                                     { "category", categoryName },
                                     { "counter", counterName },
                                 };
            foreach (var tag in globalTags)
            {
                dictionary.Add(tag.Name, tag.Value);
            }
            return dictionary;
        }

        public void Dispose()
        {
            _timer.Stop();
        }
    }
}