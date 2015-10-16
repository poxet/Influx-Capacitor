using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class CounterListCommand : ActionCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public CounterListCommand(IConfigBusiness configBusiness, ICounterBusiness counterBusiness)
            : base("List", "List performance counters.")
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            var cnt = 0;
            foreach (var counterGroup in counterGroups)
            {
                OutputInformation("{0}", counterGroup.Name);
                foreach (var counter in counterGroup.GetFreshCounters())
                {
                    if (counter.PerformanceCounter != null)
                    {
                        OutputInformation("   OK\t{0}.{1}.{2} {3}", counter.PerformanceCounter.CategoryName, counter.PerformanceCounter.CounterName, counter.PerformanceCounter.InstanceName, counter.Name);
                        cnt++;
                    }
                    else
                    {
                        var name = counter.Name;
                        if (string.IsNullOrEmpty(name))
                            name = "N/A";

                        OutputWarning("   -\t{0}", name);
                    }                    
                }
            }

            OutputInformation("Totally {0} counters that will be executed by the service.", cnt);

            return true;
        }
    }
}