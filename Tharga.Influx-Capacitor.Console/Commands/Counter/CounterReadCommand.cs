using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class CounterReadCommand : CounterCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public CounterReadCommand(IConfigBusiness configBusiness, ICounterBusiness counterBusiness)
            : base("Read", "Reads the value from the performance counter.")
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
            ReadCounterGroup(counterGroup);

            return true;
        }
    }
}