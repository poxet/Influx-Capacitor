using InfluxDB.Net.Collector.Business;

namespace InfluxDB.Net.Collector
{
    public class Processor
    {
        private readonly ConfigBusiness _configBusiness;
        private readonly CounterBusiness _counterBusiness;

        public Processor(ConfigBusiness configBusiness, CounterBusiness counterBusiness)
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
        }

        public void Run(string[] configFileNames)
        {
            var config = _configBusiness.LoadFiles(configFileNames);

            var client = new InfluxDb(config.Database.Url, config.Database.Username, config.Database.Password);

            var pong = client.PingAsync().Result;
            System.Diagnostics.Debug.WriteLine("Ping: {0} ({1} ms)", pong.Status, pong.ResponseTime);

            var version = client.VersionAsync().Result;
            System.Diagnostics.Debug.WriteLine("Version: {0}", version);

            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            foreach (var counterGroup in counterGroups)
            {
                var engine = new CollectorEngine(client, config.Database.Name, counterGroup);
                engine.Start();
            }
        }
    }
}