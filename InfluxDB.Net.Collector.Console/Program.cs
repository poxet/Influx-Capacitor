using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using InfluxDB.Net.Collector.Console.Business;
using InfluxDB.Net.Collector.Console.Entities;
using InfluxDB.Net.Models;

namespace InfluxDB.Net.Collector.Console
{
    static class Program
    {
        private static InfluxDb _client;
        private static Config _config;

        static void Main(string[] args)
        {
            var configurationFilename = "";
            if (args.Length > 0)
            {
                configurationFilename = args[0];
            }

            var configBusiness = new ConfigBusiness();
            _config = configBusiness.LoadFile(configurationFilename);

            _client = new InfluxDb(_config.Database.Url, _config.Database.Username, _config.Database.Password);

            var pong = _client.PingAsync().Result;
            System.Console.WriteLine("Ping: {0} ({1} ms)", pong.Status, pong.ResponseTime);

            var version = _client.VersionAsync().Result;
            System.Console.WriteLine("Version: {0}", version);

            var processorCounters = new List<PerformanceCounter>
            {
                GetPerformanceCounter("Processor", "% Processor Time", "_Total"),
                GetPerformanceCounter("Processor", "% Processor Time", "0"),
                GetPerformanceCounter("Processor", "% Processor Time", "1"),
                GetPerformanceCounter("Processor", "% Processor Time", "2"),
                GetPerformanceCounter("Processor", "% Processor Time", "3"),
            };
            Thread.Sleep(100);

            RegisterCounterValues("Processor", processorCounters);

            System.Console.WriteLine("Press enter to exit...");
            System.Console.ReadKey();
        }

        private static InfluxDbApiResponse RegisterCounterValues(string name, IEnumerable<PerformanceCounter> processorCounters)
        {
            var columnNames = new List<string>();
            var datas = new List<object>();

            foreach (var processorCounter in processorCounters)
            {
                var data = processorCounter.NextValue();

                columnNames.Add(processorCounter.InstanceName);
                datas.Add(data);

                System.Console.WriteLine("{0} {1}: {2}", processorCounter.CounterName, processorCounter.InstanceName, data);
            }

            var serie = new Serie.Builder(name)
                .Columns(columnNames.Select(x => name + x).ToArray())
                .Values(datas.ToArray()) //TODO: This is provided as one value, hot as a list as it should
                .Build();
            var result = _client.WriteAsync(_config.Database.Name, TimeUnit.Milliseconds, serie);
            return result.Result;
        }

        private static PerformanceCounter GetPerformanceCounter(string categoryName, string counterName, string instanceName)
        {
            var processorCounter = new PerformanceCounter(categoryName, counterName, instanceName);
            processorCounter.NextValue();
            return processorCounter;
        }
    }
}