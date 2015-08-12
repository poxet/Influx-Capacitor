using System;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Handlers
{
    public class Processor
    {
        public event EventHandler<EngineActionEventArgs> EngineActionEvent;

        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;
        private readonly ISendBusiness _sendBusiness;

        public Processor(IConfigBusiness configBusiness, ICounterBusiness counterBusiness, ISendBusiness sendBusiness)
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
            _sendBusiness = sendBusiness;
        }

        public async void RunAsync(IPerformanceCounterGroup[] counterGroups)
        {
            foreach (var counterGroup in counterGroups)
            {
                var engine = new CollectorEngine(counterGroup, _sendBusiness);
                await engine.StartAsync();
                engine.CollectRegisterCounterValuesEvent += CollectRegisterCounterValuesEvent;
            }
        }

        public async Task RunAsync(string[] configFileNames)
        {
            var config = _configBusiness.LoadFiles(configFileNames);
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();
            RunAsync(counterGroups);
        }

        public async Task<int> CollectAssync(IPerformanceCounterGroup counterGroup)
        {
            var engine = new CollectorEngine(counterGroup, _sendBusiness);
            engine.CollectRegisterCounterValuesEvent += CollectRegisterCounterValuesEvent;
            return await engine.CollectRegisterCounterValuesAsync();
        }

        private void CollectRegisterCounterValuesEvent(object sender, CollectRegisterCounterValuesEventArgs e)
        {
            OnEngineActionEvent(new EngineActionEventArgs(e.EngineName, e.Message, e.Success));
        }

        protected virtual void OnEngineActionEvent(EngineActionEventArgs e)
        {
            var handler = EngineActionEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}