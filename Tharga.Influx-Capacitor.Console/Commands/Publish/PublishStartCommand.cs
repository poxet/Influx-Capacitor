using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Publish
{
    internal class PublishStartCommand : ActionCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;
        private readonly IPublisherBusiness _publisherBusiness;
        private readonly ISendBusiness _sendBusiness;
        private readonly ITagLoader _tagLoader;

        public PublishStartCommand(IConfigBusiness configBusiness, ICounterBusiness counterBusiness, IPublisherBusiness publisherBusiness, ISendBusiness sendBusiness, ITagLoader tagLoader)
            : base("Start", "Start the publishing of counters.")
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
            _publisherBusiness = publisherBusiness;
            _sendBusiness = sendBusiness;
            _tagLoader = tagLoader;
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _publisherBusiness.GetCounterPublishers(config).ToArray();

            var index = 0;
            var counterPublisher = QueryParam("Counter", GetParam(paramList, index++), counterGroups.Select(x => new KeyValuePair<ICounterPublisher, string>(x, x.CounterName)));

            var processor = new Processor(_configBusiness, _counterBusiness, _publisherBusiness, _sendBusiness, _tagLoader);
            processor.EngineActionEvent += EngineActionEvent;

            //if (counterPublisher == null)
            //{
            //    if (!processor.RunAsync(new string[] { }).Wait(5000))
            //    {
            //        throw new InvalidOperationException("Unable to start processor engine.");
            //    }
            //}
            //else
            //{
                var counterPublishers = new[] { counterPublisher };
                await processor.RunAsync(null, counterPublishers, config.Application.Metadata);
            //}

            return true;
        }

        private void EngineActionEvent(object sender, EngineActionEventArgs e)
        {
            OutputLine(e.Message, e.OutputLevel);
        }
    }
}