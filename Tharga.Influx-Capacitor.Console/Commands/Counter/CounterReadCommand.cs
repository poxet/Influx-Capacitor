using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class CounterReadCommand : ActionCommandBase
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
            var count = 0;
            foreach (var counter in counterGroup.PerformanceCounterInfos)
            {
                if (counter.PerformanceCounter != null)
                {
                    var nextValue = counter.PerformanceCounter.NextValue();
                    OutputInformation("{0}.{1}.{2}:\t{3}", counter.PerformanceCounter.CategoryName, counter.PerformanceCounter.CounterName, counter.PerformanceCounter.InstanceName, nextValue);
                    count++;
                }
                else
                {
                    OutputWarning("Cannot read counter {0}.", counter.Name);
                }
            }
            OutputInformation("{0} counters read.", count);

            return true;
        }
    }
}