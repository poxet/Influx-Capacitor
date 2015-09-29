using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Agents;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class CounterRawCommand : CounterCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public CounterRawCommand(IConfigBusiness configBusiness, ICounterBusiness counterBusiness)
            : base("Raw", "Shows Raw data that will be sent to influxDB.")
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            var index = 0;
            var counterGroup = QueryParam("Group", GetParam(paramList, index++), counterGroups.Select(x => new KeyValuePair<IPerformanceCounterGroup, string>(x, x.Name)));

            using (var oneTimeDryRunCollectorEngine = new SafeCollectorEngine(counterGroup, new ConsoleSendBusiness(_configBusiness, new InfluxDbAgentLoader(), ShowOutput), new TagLoader(_configBusiness)))
            {
                oneTimeDryRunCollectorEngine.CollectRegisterCounterValuesEvent += CollectRegisterCounterValuesEvent;
                await oneTimeDryRunCollectorEngine.CollectRegisterCounterValuesAsync();
            }

            return true;
        }

        private void ShowOutput(string message, OutputLevel outputLevel)
        {
            OutputLine(message, outputLevel);
        }

        private void CollectRegisterCounterValuesEvent(object sender, CollectRegisterCounterValuesEventArgs e)
        {
            OutputLine(e.Message, e.OutputLevel);
        }    
    }
}