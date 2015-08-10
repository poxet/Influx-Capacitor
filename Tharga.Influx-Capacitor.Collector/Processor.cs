using System;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector
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

        public async void RunAsync(IPerformanceCounterGroup[] counterGroupsToRead)
        {
            foreach (var counterGroup in counterGroupsToRead)
            {
                var engine = new CollectorEngine(counterGroup);
                engine.NotificationEvent += Engine_NotificationEvent;
                await engine.StartAsync();
            }
        }

        [Obsolete("Use the RunAsync method that takes counters, not config file.")]
        public async Task RunAsync(string[] configFileNames)
        {
            var config = _configBusiness.LoadFiles(configFileNames);

            //var client = _influxDbAgentLoader.GetAgent(config.Database);
            //var pong = await client.PingAsync();
            //InvokeNotificationEvent(string.Format("Ping: {0} (ver {1}, {2} ms)", pong.Success ? "success" : "fail", pong.Version, pong.ResponseTime), OutputLevel.Information);

            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            foreach (var counterGroup in counterGroups)
            {
                //var engine = new CollectorEngine(client, counterGroup);
                var engine = new CollectorEngine(counterGroup);
                engine.NotificationEvent += Engine_NotificationEvent;
                await engine.StartAsync();
            }
        }

        public async Task<int> CollectAssync(IPerformanceCounterGroup counterGroup)
        {
            //var config = _configBusiness.LoadFiles();
            //var client = _influxDbAgentLoader.GetAgent(config.Database);
            var engine = new CollectorEngine(counterGroup);

            return await engine.RegisterCounterValuesAsync();
        }

        void Engine_NotificationEvent(object sender, NotificationEventArgs e)
        {
            InvokeNotificationEvent(e.Message, e.OutputLevel);
        }

        private void InvokeNotificationEvent(string message, OutputLevel outputLevel)
        {
            var handler = NotificationEvent;
            if (handler != null) handler(this, new NotificationEventArgs(message, outputLevel));
        }
    }
}