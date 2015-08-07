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
            foreach (var counterGroup in counterGroups)
            {
                OutputInformation("{0}", counterGroup.Name);
                foreach (var counter in counterGroup.PerformanceCounterInfos)
                {
                    if (counter.PerformanceCounter != null)
                    {
                        OutputInformation("   OK\t{0}.{1}.{2} {3}", counter.PerformanceCounter.CategoryName, counter.PerformanceCounter.CounterName, counter.PerformanceCounter.InstanceName, counter.Name);
                    }
                    else
                    {
                        OutputInformation("   -\t{1}", counter.Name);
                    }                    
                }
            }

            return true;
        }
    }
}