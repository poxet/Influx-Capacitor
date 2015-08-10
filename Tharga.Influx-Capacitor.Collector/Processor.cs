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
        private readonly ISendBusiness _sendBusiness;

        public Processor(IConfigBusiness configBusiness, ICounterBusiness counterBusiness, ISendBusiness sendBusiness)
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
            _sendBusiness = sendBusiness;
        }

        public async void RunAsync(IPerformanceCounterGroup[] counterGroupsToRead)
        {
            foreach (var counterGroup in counterGroupsToRead)
            {
                var engine = new CollectorEngine(counterGroup, _sendBusiness);
                engine.NotificationEvent += Engine_NotificationEvent;
                await engine.StartAsync();
            }
        }

        public async Task RunAsync(string[] configFileNames)
        {
            var config = _configBusiness.LoadFiles(configFileNames);

            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            foreach (var counterGroup in counterGroups)
            {
                await CollectAssync(counterGroup);
            }
        }

        public async Task<int> CollectAssync(IPerformanceCounterGroup counterGroup)
        {
            var engine = new CollectorEngine(counterGroup, _sendBusiness);
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