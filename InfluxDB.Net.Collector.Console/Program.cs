using InfluxDB.Net.Collector.Console.Business;
using InfluxDB.Net.Collector.Console.Entities;

namespace InfluxDB.Net.Collector.Console
{
    static class Program
    {
        private static InfluxDb _client;
        private static Config _config;

        static void Main(string[] args)
        {
            var configBusiness = new ConfigBusiness();
            _config = configBusiness.LoadFiles(args);

            _client = new InfluxDb(_config.Database.Url, _config.Database.Username, _config.Database.Password);

            var pong = _client.PingAsync().Result;
            System.Console.WriteLine("Ping: {0} ({1} ms)", pong.Status, pong.ResponseTime);

            var version = _client.VersionAsync().Result;
            System.Console.WriteLine("Version: {0}", version);

            var counterBusiness = new CounterBusiness();
            var counterGroups = counterBusiness.GetPerformanceCounterGroups(_config).ToArray();

            foreach (var counterGroup in counterGroups)
            {
                var engine = new CollectorEngine(_client, _config.Database.Name, counterGroup);
                engine.Start();
            }

            System.Console.WriteLine("Press enter to exit...");
            System.Console.ReadKey();
        }
    }
}