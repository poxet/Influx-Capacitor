using System;
using InfluxDB.Net.Collector.Business;

namespace InfluxDB.Net.Collector
{
    public class Processor
    {
        public event EventHandler<NotificationEventArgs> NotificationEvent;

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
            InvokeNotificationEvent(string.Format("Ping: {0} ({1} ms)", pong.Status, pong.ResponseTime));

            var version = client.VersionAsync().Result;
            InvokeNotificationEvent(string.Format("Version: {0}", version));

            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            foreach (var counterGroup in counterGroups)
            {
                var engine = new CollectorEngine(client, config.Database.Name, counterGroup);
                engine.NotificationEvent += engine_NotificationEvent;
                engine.Start();
            }
        }

        void engine_NotificationEvent(object sender, NotificationEventArgs e)
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