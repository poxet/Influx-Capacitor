using System;
using System.Threading.Tasks;
using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector
{
    public class Processor
    {
        public event EventHandler<NotificationEventArgs> NotificationEvent;

        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;
        private readonly IInfluxDbAgentLoader _influxDbAgentLoader;

        public Processor(IConfigBusiness configBusiness, ICounterBusiness counterBusiness, IInfluxDbAgentLoader influxDbAgentLoader)
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
            _influxDbAgentLoader = influxDbAgentLoader;
        }

        public async Task RunAsync(string[] configFileNames)
        {
            var config = _configBusiness.LoadFiles(configFileNames);

            var client = _influxDbAgentLoader.GetAgent(config.Database);

            var pong = await client.PingAsync();
            InvokeNotificationEvent(string.Format("Ping: {0} ({1} ms)", pong.Status, pong.ResponseTime));

            try
            {
                var version = await client.VersionAsync();
                InvokeNotificationEvent(string.Format("Version: {0}", version));
            }
            catch (Exception exception)
            {
                InvokeNotificationEvent(exception.Message);
            }

            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            foreach (var counterGroup in counterGroups)
            {
                var engine = new CollectorEngine(client, config.Database.Name, counterGroup);
                engine.NotificationEvent += Engine_NotificationEvent;
                await engine.StartAsync();
            }
        }

        void Engine_NotificationEvent(object sender, NotificationEventArgs e)
        {
            InvokeNotificationEvent(e.Message);
        }

        private void InvokeNotificationEvent(string message)
        {
            var handler = NotificationEvent;
            if (handler != null) handler(this, new NotificationEventArgs(message));
        }
    }
}