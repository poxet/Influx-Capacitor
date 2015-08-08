using System;
using System.Collections.Generic;
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
        private readonly Timer _timer;
        private readonly IInfluxDbAgent _client;
        private readonly string _name;

        public CollectorEngine(IInfluxDbAgent client, IPerformanceCounterGroup performanceCounterGroup)
        {
            _client = client;
            _performanceCounterGroup = performanceCounterGroup;
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

        public async Task<int> RegisterCounterValuesAsync()
        {
            var points = new List<Point>();

            foreach (var performanceCounterInfo in _performanceCounterGroup.PerformanceCounterInfos.Where(x => x.PerformanceCounter != null))
            {
                var value = performanceCounterInfo.PerformanceCounter.NextValue();
                var categoryName = performanceCounterInfo.PerformanceCounter.CategoryName;
                var counterName = performanceCounterInfo.PerformanceCounter.CounterName;
                var key = performanceCounterInfo.Name.Clean();

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
                        { "value", value }
                    },
                    Precision = TimeUnit.Microseconds,
                    Timestamp = DateTime.UtcNow
                };

                if (!string.IsNullOrEmpty(key))
                {
                    point.Tags.Add("instance", key);
                }

                points.Add(point);
            }

            await _client.WriteAsync(points.ToArray());

            return points.Count;
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