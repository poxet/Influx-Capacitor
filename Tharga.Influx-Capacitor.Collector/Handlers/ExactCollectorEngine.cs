using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector.Handlers
{
    internal class PublisherEngine : IPublisherEngine
    {
        private readonly ICounterPublisher _counterPublisher;
        private readonly ISendBusiness _sendBusiness;
        private readonly Timer _timer;
        private ITag[] _tags;
        private readonly PerformanceCounter _perfCounter;

        public event EventHandler<PublishRegisterCounterValuesEventArgs> PublishRegisterCounterValuesEvent;

        public PublisherEngine(ICounterPublisher counterPublisher, ISendBusiness sendBusiness, ITagLoader tagLoader)
        {
            _counterPublisher = counterPublisher;
            _sendBusiness = sendBusiness;
            _tags = tagLoader.GetGlobalTags(); //.Union(_performanceCounterGroup.Tags).ToArray();

            if (counterPublisher.SecondsInterval > 0)
            {
                _timer = new Timer(1000 * counterPublisher.SecondsInterval);
                _timer.Elapsed += Elapsed;
            }

            _perfCounter = SetupCategory();
            //_perfCounter = new PerformanceCounter(_counterPublisher.CategoryName, _counterPublisher.CounterName, false) { RawValue = _counterPublisher.GetValue() };
        }

        private async void Elapsed(object sender, ElapsedEventArgs e)
        {
            await PublishRegisterCounterValuesAsync();
        }

        public async Task PublishRegisterCounterValuesAsync()
        {
            try
            {
                _timer.Stop();
                var value = _counterPublisher.GetValue();                
                _perfCounter.RawValue = value;

                //Console.WriteLine("{0} read {1} and published to {2}", _counterPublisher.CounterName, value, _perfCounter.CounterName);
                Console.WriteLine("{0} read {1}.", _counterPublisher.CounterName, value);

                //var performanceCounterInfo = performanceCounterInfos[i];

                var categoryName = _counterPublisher.CategoryName;
                var counterName = _counterPublisher.CounterName;
                //var key = performanceCounterInfo.PerformanceCounter.InstanceName;
                //var instanceAlias = performanceCounterInfo.Alias;
                //var tags = GetTags(_tags.Union(performanceCounterInfo.Tags), categoryName, counterName);
                var tags = GetTags(_tags, categoryName, counterName);
                var fields = new Dictionary<string, object>
                    {
                        { "value", value },
                        //{ "readSpan", readSpan }, //Time in ms from the first, to the lats counter read in the group.
                        //{ "timeOffset", (float)(timeOffset * 1000) } //Time difference in ms from reported time, to when read actually started.
                    };

                var point = new Point
                {
                    Name = _counterPublisher.CounterName,
                    Tags = tags,
                    Fields = fields,
                    Precision = TimeUnit.Seconds,
                    Timestamp = DateTime.UtcNow,
                };

                //if (!string.IsNullOrEmpty(key))
                //{
                //    point.Tags.Add("instance", key);
                //    if (!string.IsNullOrEmpty(instanceAlias))
                //    {
                //        point.Tags.Add(instanceAlias, key);
                //    }
                //}

                //TODO: Have a setting to evaluate if this is to be sent directly to Graphana. Of just published as a performance counter.
                _sendBusiness.Enqueue(new[] { point });
            }
            catch (Exception exception)
            {
                OnPublishRegisterCounterValuesEvent(new PublishRegisterCounterValuesEventArgs(_counterPublisher.CounterName, exception));
            }
            finally
            {
                _timer.Start();
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

        private void OnPublishRegisterCounterValuesEvent(PublishRegisterCounterValuesEventArgs e)
        {
            var handler = PublishRegisterCounterValuesEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private PerformanceCounter SetupCategory()
        {
            //PerformanceCounterCategory.Delete(_counterPublisher.CategoryName);

            if (!PerformanceCounterCategory.Exists(_counterPublisher.CategoryName))
            {
                var counterCreationDataCollection = new CounterCreationDataCollection { new CounterCreationData { CounterName = "Random", CounterType = PerformanceCounterType.NumberOfItems64 }, new CounterCreationData { CounterName = "TotalMemory", CounterType = PerformanceCounterType.NumberOfItems64 } };
                PerformanceCounterCategory.Create(_counterPublisher.CategoryName, _counterPublisher.CategoryHelp, PerformanceCounterCategoryType.SingleInstance, counterCreationDataCollection);
            }
            var cat = PerformanceCounterCategory.GetCategories().Single(x => x.CategoryName == _counterPublisher.CategoryName);
            var counter = cat.GetCounters().Single(x => x.CounterName == _counterPublisher.CounterName);
            counter.ReadOnly = false;
            return counter;

            //if (!PerformanceCounterCategory.Exists(_counterPublisher.CategoryName))
            //{
            //    var counterCreationData = new CounterCreationData
            //    {
            //        CounterType = _counterPublisher.CounterType,
            //        CounterName = _counterPublisher.CounterName
            //    };
            //    var counterCreationDataCollection = new CounterCreationDataCollection { counterCreationData };
            //    PerformanceCounterCategory.Create(_counterPublisher.CategoryName, _counterPublisher.CategoryHelp, _counterPublisher.PerformanceCounterCategoryType, counterCreationDataCollection);
            //}
            //else
            //{
            //    //Check if current counter exists
            //    var category = PerformanceCounterCategory.GetCategories().Single(x => x.CategoryName == _counterPublisher.CategoryName);
            //    var counters = category.GetCounters();
            //    if (counters.Any(x => x.CounterName == _counterPublisher.CounterName))
            //    {
            //        return;
            //    }

            //    var counterCreationData = new CounterCreationData
            //    {
            //        CounterType = _counterPublisher.CounterType,
            //        CounterName = _counterPublisher.CounterName
            //    };
            //    var counterCreationDataCollection = new CounterCreationDataCollection { counterCreationData };

            //    foreach (var item in counters)
            //    {
            //        counterCreationDataCollection.Add(new CounterCreationData(item.CounterName, item.CounterHelp, item.CounterType));
            //    }

            //    PerformanceCounterCategory.Delete(_counterPublisher.CategoryName);
            //    PerformanceCounterCategory.Create(_counterPublisher.CategoryName, _counterPublisher.CategoryHelp, _counterPublisher.PerformanceCounterCategoryType, counterCreationDataCollection);
            //}
        }

        public async Task StartAsync()
        {
            if (_timer == null) return;
            await PublishRegisterCounterValuesAsync();
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
    }

    internal class ExactCollectorEngine : CollectorEngineBase
    {
        private readonly object _syncRoot = new object();
        private StopwatchHighPrecision _sw;
        private long _counter;
        private int _missCounter;

        public ExactCollectorEngine(IPerformanceCounterGroup performanceCounterGroup, ISendBusiness sendBusiness, ITagLoader tagLoader, bool metadata)
            : base(performanceCounterGroup, sendBusiness, tagLoader, metadata)
        {
        }

        public override async Task<int> CollectRegisterCounterValuesAsync()
        {
            lock (_syncRoot)
            {
                try
                {
                    var swMain = new StopwatchHighPrecision();
                    var timeInfo = new Dictionary<string, long>();

                    double elapseOffsetSeconds = 0;
                    if (_timestamp == null)
                    {
                        _sw = new StopwatchHighPrecision();
                        _timestamp = DateTime.UtcNow;
                        var nw = Floor(_timestamp.Value, new TimeSpan(0, 0, 0, 1));
                        _timestamp = nw;
                    }
                    else
                    {
                        var elapsedTotal = _sw.ElapsedTotal;

                        elapseOffsetSeconds = new TimeSpan(elapsedTotal).TotalSeconds - SecondsInterval * _counter;

                        if (_missCounter >= 6)
                        {
                            //Reset everything and start over.
                            OnCollectRegisterCounterValuesEvent(new CollectRegisterCounterValuesEventArgs(Name, string.Format("Missed {0} cycles. Resetting and starting over.", _missCounter), OutputLevel.Warning));

                            _timestamp = null;
                            _counter = 0;
                            _missCounter = 0;
                            return -4;
                        }

                        if (elapseOffsetSeconds > SecondsInterval)
                        {
                            _missCounter++;
                            _counter = _counter + 1 + (int)(elapseOffsetSeconds / SecondsInterval);
                            OnCollectRegisterCounterValuesEvent(new CollectRegisterCounterValuesEventArgs(Name, string.Format("Dropping {0} steps.", (int)elapseOffsetSeconds), OutputLevel.Warning));
                            return -2;
                        }

                        if (elapseOffsetSeconds < SecondsInterval * -1)
                        {
                            _missCounter++;
                            OnCollectRegisterCounterValuesEvent(new CollectRegisterCounterValuesEventArgs(Name, string.Format("Jumping 1 step. ({0} = new TimeSpan({1}).TotalSeconds - {2} * {3})", (int)elapseOffsetSeconds, elapsedTotal, SecondsInterval, _counter), OutputLevel.Warning));
                            return -3;
                        }

                        _missCounter = 0;

                        //Adjust interval
                        var next = 1000 * (SecondsInterval - elapseOffsetSeconds);
                        if (next > 0)
                        {
                            SetTimerInterval(next);
                        }
                    }

                    var timestamp = _timestamp.Value.AddSeconds(SecondsInterval * _counter);
                    _counter++;

                    var precision = TimeUnit.Seconds;
                    timeInfo.Add(TimerConstants.Synchronize, swMain.ElapsedSegment);

                    //TODO: Create a mutex lock here (So that two counters canno read the same signature at the same time, since the content of the _performanceCounterGroup might change during this process.

                    //Prepare read
                    var performanceCounterInfos = PrepareCounters();
                    timeInfo.Add(TimerConstants.Prepare, swMain.ElapsedSegment);

                    //Perform Read (This should be as fast and short as possible)
                    var values = ReadValues(performanceCounterInfos);
                    timeInfo.Add(TimerConstants.Read, swMain.ElapsedSegment);

                    //Prepare result                
                    var points = FormatResult(performanceCounterInfos, values, precision, timestamp).ToArray();
                    timeInfo.Add(TimerConstants.Format, swMain.ElapsedSegment);

                    //Queue result
                    Enqueue(points);
                    timeInfo.Add(TimerConstants.Enque, swMain.ElapsedSegment);

                    //Cleanup
                    RemoveObsoleteCounters(values, performanceCounterInfos);
                    timeInfo.Add(TimerConstants.Cleanup, swMain.ElapsedSegment);

                    if (_metadata)
                    {
                        Enqueue(new[] { MetaDataBusiness.GetCollectorPoint(EngineName, Name, points.Length, timeInfo, elapseOffsetSeconds) });
                    }

                    OnCollectRegisterCounterValuesEvent(new CollectRegisterCounterValuesEventArgs(Name, points.Count(), timeInfo, elapseOffsetSeconds, OutputLevel.Default));

                    //TODO: Release mutex

                    //TOOD: Send metadata about the read to influx, (this should be configurable)

                    return points.Length;
                }
                catch (Exception exception)
                {
                    OnCollectRegisterCounterValuesEvent(new CollectRegisterCounterValuesEventArgs(Name, exception));
                    return -1;
                }
            }
        }
    }
}