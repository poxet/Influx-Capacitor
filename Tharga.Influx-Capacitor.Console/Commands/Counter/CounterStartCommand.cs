using System;
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

        public override async Task<bool> InvokeAsync(string paramList)
        {
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            var index = 0;
            var counterGroup = QueryParam("Group", GetParam(paramList, index++), counterGroups.Select(x => new KeyValuePair<IPerformanceCounterGroup, string>(x, x.Name)));

            var processor = new Processor(_configBusiness, _counterBusiness, _sendBusiness);

            if (counterGroup == null)
            {
                if (!processor.RunAsync(new string[] { }).Wait(5000))
                {
                    throw new InvalidOperationException("Unable to start processor engine.");
                }
            }
            else
            {
                var counterGroupsToRead = counterGroup != null ? new[] { counterGroup } : counterGroups;


                processor.RunAsync(counterGroupsToRead);
            }

            return true;
        }
    }
}