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
}