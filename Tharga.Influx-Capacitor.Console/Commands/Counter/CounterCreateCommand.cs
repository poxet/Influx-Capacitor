using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class CounterCreateCommand : ActionCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public CounterCreateCommand(IConfigBusiness configBusiness, ICounterBusiness counterBusiness)
            : base("Create", "Create a new performance counter config file.")
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var index = 0;

            var addAnother = true;
            var collectors = new List<ICounter>();

            while (addAnother)
            {
                var categoryNames = _counterBusiness.GetCategoryNames();
                var categoryName = QueryParam("Category", GetParam(paramList, index++), categoryNames.Select(x => new KeyValuePair<string, string>(x, x)));

                var counterNames = _counterBusiness.GetCounterNames(categoryName);
                var counterName = QueryParam("Counter", GetParam(paramList, index++), counterNames.Select(x => new KeyValuePair<string, string>(x, x)));

                var instanceNames = _counterBusiness.GetInstances(categoryName, counterName);
                var instanceName = QueryParam("Instance", GetParam(paramList, index++), instanceNames.Select(x => new KeyValuePair<string, string>(x, x)));

                addAnother = QueryParam<bool>("Add another?", GetParam(paramList, index++), new Dictionary<bool, string> { { true, "Yes" }, { false, "No" } });

                var collector = new Collector.Entities.Counter(categoryName, counterName, instanceName);
                collectors.Add(collector);
            }

            var groupName = QueryParam<string>("Group Name", GetParam(paramList, index++));
            var secondsInterval = QueryParam<int>("Seconds Interval", GetParam(paramList, index++));

            var initaiteBusiness = new InitaiteBusiness(_configBusiness, _counterBusiness);
            var message = initaiteBusiness.CreateCounter(groupName, secondsInterval, collectors);
            OutputInformation(message);

            return false;
        }
    }
}