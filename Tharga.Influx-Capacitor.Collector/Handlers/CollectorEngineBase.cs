using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using InfluxDB.Net.Enums;
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
        private int _refreshCountdown;
        private readonly Timer _timer;
        protected readonly bool _metadata;

        //TODO: Cleanup
        protected DateTime? _timestamp; //TODO: Rename to first Read
        private readonly string _engineName;

        protected CollectorEngineBase(IPerformanceCounterGroup performanceCounterGroup, ISendBusiness sendBusiness, ITagLoader tagLoader, bool metadata)
        {
            _engineName = GetType().Name;
            _performanceCounterGroup = performanceCounterGroup;
            _name = _performanceCounterGroup.Name;
            _sendBusiness = sendBusiness;
            _tags = tagLoader.GetGlobalTags().Union(_performanceCounterGroup.Tags).ToArray();

            if (performanceCounterGroup.SecondsInterval > 0)
            {
                _timer = new Timer(1000 * performanceCounterGroup.SecondsInterval);
                _timer.Elapsed += Elapsed;
            }

            _metadata = metadata;
        }

        public event EventHandler<CollectRegisterCounterValuesEventArgs> CollectRegisterCounterValuesEvent;

        protected string EngineName
        {
            get
            {
                return _engineName;
            }
        }

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

        private ITag[] Tags
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
                    var infos = performanceCounterInfos[i];
                    var value = infos.NextValue();

                    // if the counter value is greater than the max limit, then we use the max value
                    // see https://support.microsoft.com/en-us/kb/310067
                    if (infos.Max.HasValue)
                    {
                        value = Math.Min(infos.Max.Value, value);
                    }

                    values[i] = value;
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
            if (performanceCounterInfos.Any(i => !string.IsNullOrEmpty(i.FieldName)))
            {
                // WIDE SCHEMA
                // If at least one field name is specified, then we use the "wide" schema format, where all values are stored as different fields.
                // We will try to generate as few points as possible (only one per instance)
                return FormatResultWide(performanceCounterInfos, values, precision, timestamp);
            }
            else
            {
                // LONG SCHEMA
                // If no field name is specified we use the default format which result in each counter being sent as one point.
                // Each point as counter, category and instance specified as tags
                return FormatResultLong(performanceCounterInfos, values, precision, timestamp);
            }
        }

        private IEnumerable<Point> FormatResultWide(IPerformanceCounterInfo[] performanceCounterInfos, float?[] values, TimeUnit precision, DateTime timestamp)
        {
            var valuesByInstance = new Dictionary<string, Dictionary<string, object>>();
            
            // first pass: we group all values by instance name
            // if no field name is specified, we store the value in default "value" field
            // if no instance name is specified, we use a default empty instance name
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (value != null)
                {
                    var performanceCounterInfo = performanceCounterInfos[i];
                    var fieldName = performanceCounterInfo.FieldName ?? "value";
                    var key = performanceCounterInfo.InstanceName;

                    Dictionary<string, object> fields;
                    if (!valuesByInstance.TryGetValue(key ?? string.Empty, out fields))
                    {
                        fields = new Dictionary<string, object>();
                        valuesByInstance[key ?? string.Empty] = fields;
                    }

                    fields[fieldName] = value;
                }
            }

            // second pass: for each instance name, we create a point
            if (valuesByInstance.Count != 0)
            {
                foreach (var instanceValues in valuesByInstance)
                {
                    var point = new Point
                    {
                        Measurement = Name,
                        Tags = GetTags(Tags, null, null),
                        Fields = instanceValues.Value,
                        Precision = precision,
                        Timestamp = timestamp
                    };

                    var instance = instanceValues.Key;
                    if (!string.IsNullOrEmpty(instance))
                    {
                        point.Tags.Add("instance", instance);
                    }

                    yield return point;
                }
            }
        }

        private IEnumerable<Point> FormatResultLong(IPerformanceCounterInfo[] performanceCounterInfos, float?[] values, TimeUnit precision, DateTime timestamp)
        {
            for (var i = 0; i < values.Count(); i++)
            {
                var value = values[i];
                if (value != null)
                {
                    var performanceCounterInfo = performanceCounterInfos[i];

                    var categoryName = performanceCounterInfo.CategoryName;
                    var counterName = performanceCounterInfo.CounterName;
                    var key = performanceCounterInfo.InstanceName;
                    var instanceAlias = performanceCounterInfo.Alias;
                    var tags = GetTags(Tags.Union(performanceCounterInfo.Tags), categoryName, counterName);
                    var fields = new Dictionary<string, object>
                    {
                        { "value", value },
                    };

                    var point = new Point
                    {
                        Measurement = Name,
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

        private static Dictionary<string, object> GetTags(IEnumerable<ITag> globalTags, string categoryName, string counterName)
        {
            var dictionary = new Dictionary<string, object>
            {
                { "hostname", Environment.MachineName }
            };

            if (categoryName != null)
            {
                dictionary.Add("category", categoryName);
            }

            if (counterName != null)
            {
                dictionary.Add("counter", counterName);
            }

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