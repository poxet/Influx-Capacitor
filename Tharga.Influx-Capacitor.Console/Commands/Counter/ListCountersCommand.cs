using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class ListCountersCommand : ActionCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public ListCountersCommand(IConfigBusiness configBusiness, ICounterBusiness counterBusiness)
            : base("List", "List performance counters.")
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();
            foreach (var counterGroup in counterGroups)
            {
                OutputInformation("{0}", counterGroup.Name);
                foreach (var info in counterGroup.PerformanceCounterInfos)
                {
                    var status = info.PerformanceCounter == null ? "-" : "OK";
                    OutputInformation("   {0}\t{1}", status, info.Name);
                }
            }

            return true;
        }
    }
}