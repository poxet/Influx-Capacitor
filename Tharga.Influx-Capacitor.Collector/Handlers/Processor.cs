using System;
using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Handlers
{
    public class Processor
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;
        private readonly IPublisherBusiness _publisherBusiness;
        private readonly ISendBusiness _sendBusiness;
        private readonly ITagLoader _tagLoader;

        public Processor(IConfigBusiness configBusiness, ICounterBusiness counterBusiness, IPublisherBusiness publisherBusiness, ISendBusiness sendBusiness, ITagLoader tagLoader)
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
            _sendBusiness = sendBusiness;
            _tagLoader = tagLoader;
            _publisherBusiness = publisherBusiness;
        }

        public event EventHandler<EngineActionEventArgs> EngineActionEvent;

        public async Task RunAsync(IPerformanceCounterGroup[] counterGroups, ICounterPublisher[] counterPublishers, bool metadata)
        {
            if (counterGroups != null)
                foreach (var counterGroup in counterGroups)
                {
                    var engine = GetCollectorEngine(counterGroup, counterGroup.CollectorEngineType, metadata);
                    engine.CollectRegisterCounterValuesEvent += CollectRegisterCounterValuesEvent;
                    await engine.StartAsync();
                }

            if (counterPublishers != null)
                foreach (var counterPublisher in counterPublishers)
                {
                    var engine = GetPublisherEngine(counterPublisher);
                    engine.PublishRegisterCounterValuesEvent += PublishRegisterCounterValuesEvent;
                    await engine.StartAsync();
                }
        }

        public async Task RunAsync(string[] configFileNames)
        {
            var config = _configBusiness.LoadFiles(configFileNames);
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();
            var counterPublishers = _publisherBusiness.GetCounterPublishers(config).ToArray();
            await RunAsync(counterGroups, counterPublishers, config.Application.Metadata);
        }

        public async Task<int> CollectAssync(IPerformanceCounterGroup counterGroup, bool metadata)
        {
            var engine = GetCollectorEngine(counterGroup, counterGroup.CollectorEngineType, metadata);
            engine.CollectRegisterCounterValuesEvent += CollectRegisterCounterValuesEvent;
            return await engine.CollectRegisterCounterValuesAsync();
        }

        private IPublisherEngine GetPublisherEngine(ICounterPublisher counterPublisher)
        {
            return new PublisherEngine(counterPublisher, _sendBusiness, _tagLoader);
        }

        private ICollectorEngine GetCollectorEngine(IPerformanceCounterGroup counterGroup, CollectorEngineType collectorEngineType, bool metadata)
        {
            switch (collectorEngineType)
            {
                case CollectorEngineType.Exact:
                    return new ExactCollectorEngine(counterGroup, _sendBusiness, _tagLoader, metadata);
                case CollectorEngineType.Safe:
                    return new SafeCollectorEngine(counterGroup, _sendBusiness, _tagLoader, metadata);
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Unknown collector engine type {0}.", collectorEngineType));
            }
        }

        private void PublishRegisterCounterValuesEvent(object sender, PublishRegisterCounterValuesEventArgs e)
        {
            OnEngineActionEvent(new EngineActionEventArgs(e.EngineName, e.Message, e.OutputLevel));
        }

        private void CollectRegisterCounterValuesEvent(object sender, CollectRegisterCounterValuesEventArgs e)
        {
            OnEngineActionEvent(new EngineActionEventArgs(e.EngineName, e.Message, e.OutputLevel));
        }

        protected virtual void OnEngineActionEvent(EngineActionEventArgs e)
        {
            var handler = EngineActionEvent;
            if (handler != null)
                handler(this, e);
        }
    }
}