using System;
using System.Diagnostics;
using InfluxDB.Net.Models;

namespace InfluxDB.Net.Collector.Console
{
    static class Program
    {
        private static InfluxDb _client;

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                //url:      http://128.199.43.107:8086
                //username: root
                //Password: ?????
                throw new InvalidOperationException("Three parameters needs to be provided to this application. url, username and password for the InfluxDB database.");
            }

            var url = args[0];
            var username = args[1];
            var password = args[2];

            _client = new InfluxDb(url, username, password);

            var pong = _client.PingAsync().Result;
            System.Console.WriteLine("Ping: {0} ({1} ms)", pong.Status, pong.ResponseTime);

            var version = _client.VersionAsync().Result;
            System.Console.WriteLine("Version: {0}", version);

            var processorCounter = GetPerformanceCounter();
            System.Threading.Thread.Sleep(100);

            var result = RegisterCounterValue(processorCounter);
            System.Console.WriteLine(result.StatusCode);

            System.Console.WriteLine("Press enter to exit...");
            System.Console.ReadKey();
        }

        private static InfluxDbApiResponse RegisterCounterValue(PerformanceCounter processorCounter)
        {
            var data = processorCounter.NextValue();
            System.Console.WriteLine("Processor value: {0}%", data);
            var serie = new Serie.Builder("Processor")
                .Columns("Total")
                .Values(data)
                .Build();
            var result = _client.WriteAsync("QTest", TimeUnit.Milliseconds, serie);
            return result.Result;
        }

        private static PerformanceCounter GetPerformanceCounter()
        {
            var processorCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            processorCounter.NextValue();
            return processorCounter;
        }
    }
}