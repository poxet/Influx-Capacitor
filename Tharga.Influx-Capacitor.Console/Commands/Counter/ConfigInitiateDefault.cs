using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class ConfigInitiateDefault : ActionCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public ConfigInitiateDefault(IConfigBusiness configBusiness, ICounterBusiness counterBusiness)
            : base("Initiate", "Create counter configurations to get started.")
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            CreateProcessorCounter();
            CreateMemoryCounter();

            return true;
        }

        private void CreateProcessorCounter()
        {
            var name = "processor";

            var counters = new List<ICounter> { new Collector.Entities.Counter("Processor", "% Processor Time", "*") };
            var response = new CounterGroup(name, 10, counters);
            CreateFile(name, response);
        }

        private void CreateMemoryCounter()
        {
            var name = "memory";

            var counters = new List<ICounter> { new Collector.Entities.Counter("Memory", "*") };
            var response = new CounterGroup(name, 10, counters);            
            CreateFile(name, response);
        }

        private void CreateFile(string name, CounterGroup response)
        {
            //Check if there is a counter with this name already
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();
            if (counterGroups.Any(x => x.Name == response.Name))
            {
                OutputWarning("There is already a counter group named " + response.Name + ".");
            }
            else if (_configBusiness.CreateConfig(name + ".xml", new List<ICounterGroup> { response }))
            {
                OutputInformation("Created counter config " + name + ".");
            }
            else
            {
                OutputWarning("Did not create " + name + ", the file " + name + ".xml" + " already exists.");
            }
        }
    }
}