using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using InfluxDB.Net.Collector.Interface;
using InfluxDB.Net.Models;
using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector
{
    internal class CollectorEngine
    {
        public event EventHandler<NotificationEventArgs> NotificationEvent;

        private readonly IPerformanceCounterGroup _performanceCounterGroup;
        private readonly Timer _timer;
        private readonly IInfluxDbAgent _client;
        private readonly string _databaseName;
        private readonly string _name;

        public CollectorEngine(IInfluxDbAgent client, string databaseName, IPerformanceCounterGroup performanceCounterGroup)
        {
            _client = client;
            _performanceCounterGroup = performanceCounterGroup;
            _databaseName = databaseName;
            if (performanceCounterGroup.SecondsInterval > 0)
            {
                _timer = new Timer(1000 * performanceCounterGroup.SecondsInterval);
                _timer.Elapsed += Elapsed;
            }
            _name = _performanceCounterGroup.Name;
        }

        private async void Elapsed(object sender, ElapsedEventArgs e)
        {
            await RegisterCounterValuesAsync();
        }

        internal async Task RegisterCounterValuesAsync()
        {
            var columnNames = new List<string>();
            var datas = new List<object>();

            //Counter data
            foreach (var processorCounter in _performanceCounterGroup.PerformanceCounters)
            {
                var data = processorCounter.NextValue();

                columnNames.Add(processorCounter.InstanceName);
                datas.Add(data);
            }

            if (datas.Any())
            {
                //Append metadata
                columnNames.Add("MachineName");
                datas.Add(Environment.MachineName);

                var serie = new Serie.Builder(_name)
                    .Columns(columnNames.Select(x => _name + x).ToArray())
                    .Values(datas.ToArray())
                    .Build();
                var result = await _client.WriteAsync(TimeUnit.Milliseconds, serie);
                InvokeNotificationEvent(new NotificationEventArgs(string.Format("Collector engine {0} executed: {1}", _name, result.StatusCode), OutputLevel.Information));
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