using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class CounterStartCommand : ActionCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;
        private readonly ISendBusiness _sendBusiness;

        public CounterStartCommand(IConfigBusiness configBusiness, ICounterBusiness counterBusiness, ISendBusiness sendBusiness)
            : base("Start", "Start the counter and run the collector.")
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
            _sendBusiness = sendBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            var index = 0;
            var counterGroup = QueryParam("Group", GetParam(paramList, index++), counterGroups.Select(x => new KeyValuePair<IPerformanceCounterGroup, string>(x, x.Name)));

            var counterGroupsToRead = counterGroup != null ? new[] { counterGroup } : counterGroups;

            var processor = new Processor(_configBusiness, _counterBusiness, _sendBusiness);

            processor.RunAsync(counterGroupsToRead);

            return true;
        }
    }
}