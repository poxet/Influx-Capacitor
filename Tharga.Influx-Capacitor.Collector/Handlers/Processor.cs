using System;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Handlers
{
    public enum CollectorEngineType { Safe, Exact }

    public class Processor
    {
        public event EventHandler<EngineActionEventArgs> EngineActionEvent;

        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;
        private readonly ISendBusiness _sendBusiness;
        private readonly ITagLoader _tagLoader;

        public Processor(IConfigBusiness configBusiness, ICounterBusiness counterBusiness, ISendBusiness sendBusiness, ITagLoader tagLoader)
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
            _sendBusiness = sendBusiness;
            _tagLoader = tagLoader;
        }

        public async Task RunAsync(IPerformanceCounterGroup[] counterGroups)
        {
            foreach (var counterGroup in counterGroups)
            {
                var engine = GetCollectorEngine(counterGroup, counterGroup.CollectorEngineType);
                engine.CollectRegisterCounterValuesEvent += CollectRegisterCounterValuesEvent;
                await engine.StartAsync();
            }
        }

        public async Task RunAsync(string[] configFileNames)
        {
            var config = _configBusiness.LoadFiles(configFileNames);
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();
            await RunAsync(counterGroups);
        }

        public async Task<int> CollectAssync(IPerformanceCounterGroup counterGroup)
        {
            var engine = GetCollectorEngine(counterGroup, counterGroup.CollectorEngineType);
            engine.CollectRegisterCounterValuesEvent += CollectRegisterCounterValuesEvent;
            return await engine.CollectRegisterCounterValuesAsync();
        }

        private ICollectorEngine GetCollectorEngine(IPerformanceCounterGroup counterGroup, CollectorEngineType collectorEngineType)
        {
            switch (collectorEngineType)
            {
                case CollectorEngineType.Exact:
                    return new ExactCollectorEngine(counterGroup, _sendBusiness, _tagLoader);
                case CollectorEngineType.Safe:
                    return new SafeCollectorEngine(counterGroup, _sendBusiness, _tagLoader);
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Unknown collector engine type {0}.", collectorEngineType));
            }            
        }

        private void CollectRegisterCounterValuesEvent(object sender, CollectRegisterCounterValuesEventArgs e)
        {
            OnEngineActionEvent(new EngineActionEventArgs(e.EngineName, e.Message, e.OutputLevel));
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