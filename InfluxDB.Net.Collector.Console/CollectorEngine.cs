using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using InfluxDB.Net.Collector.Console.Entities;
using InfluxDB.Net.Models;

namespace InfluxDB.Net.Collector.Console
{
    public class CollectorEngine
    {
        private readonly PerformanceCounterGroup _performanceCounterGroup;
        private readonly Timer _timer;
        private readonly InfluxDb _client;
        private readonly string _databaseName;
        private readonly string _name;

        public CollectorEngine(InfluxDb client, string databaseName, PerformanceCounterGroup performanceCounterGroup)
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
            await RegisterCounterValues();
        }

        private async Task RegisterCounterValues()
        {
            var columnNames = new List<string>();
            var datas = new List<object>();

            foreach (var processorCounter in _performanceCounterGroup.PerformanceCounters)
            {
                var data = processorCounter.NextValue();

                columnNames.Add(processorCounter.InstanceName);
                datas.Add(data);

                //System.Console.WriteLine("{0} {1}: {2}", processorCounter.CounterName, processorCounter.InstanceName, data);
            }

            var serie = new Serie.Builder(_name).Columns(columnNames.Select(x => _name + x).ToArray()).Values(datas.ToArray()).Build();
            var result = await _client.WriteAsync(_databaseName, TimeUnit.Milliseconds, serie);
            System.Console.WriteLine(_name + " --> " + result.StatusCode);
        }

        public void Start()
        {
            if (_timer == null) return;
            Task.Factory.StartNew(() => RegisterCounterValues());
            _timer.Start();
        }
    }
}