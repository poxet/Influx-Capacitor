using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
 using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class CounterCreateCommand : CounterCommandBase
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

                string instanceName = null;
                var cat = new PerformanceCounterCategory(categoryName);
                if (cat.CategoryType == PerformanceCounterCategoryType.MultiInstance)
                {
                    var instanceNames = _counterBusiness.GetInstances(categoryName, counterName);
                    instanceName = QueryParam("Instance", GetParam(paramList, index++), instanceNames.Select(x => new KeyValuePair<string, string>(x, x)));
                }

                var fieldName = QueryParam<string>("FieldName", GetParam(paramList, index++));

                addAnother = QueryParam("Add another counter?", GetParam(paramList, index++), new Dictionary<bool, string> { { true, "Yes" }, { false, "No" } });

                var collector = new Collector.Entities.Counter(categoryName, counterName, instanceName, fieldName, null, null);
                collectors.Add(collector);
            }

            var groupName = QueryParam<string>("Group Name", GetParam(paramList, index++));
            var secondsInterval = QueryParam<int>("Seconds Interval", GetParam(paramList, index++));

            var refreshInstanceInterval = 0;
            if (collectors.Any(x => x.InstanceName.Contains("*") || x.InstanceName.Contains("?")))
            {
                refreshInstanceInterval = QueryParam<int>("Refresh Instance Interval", GetParam(paramList, index++));
            }

            var collectorEngineType = QueryParam("Collector Engine Type", GetParam(paramList, index++), new Dictionary<CollectorEngineType, string> { { CollectorEngineType.Safe, CollectorEngineType.Safe.ToString() }, { CollectorEngineType.Exact, CollectorEngineType.Exact.ToString() } });

            var initaiteBusiness = new DataInitiator(_configBusiness, _counterBusiness);
            var response = initaiteBusiness.CreateCounter(groupName, secondsInterval, refreshInstanceInterval, collectors, collectorEngineType);

            OutputLine(response.Item2.Item1, response.Item2.Item2);

            TestNewCounterGroup(response.Item1);

            return false;
        }

        private void TestNewCounterGroup(string counterGroupName)
        {
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();
            var counterGroup = counterGroups.Single(x => x.Name == counterGroupName);

            ReadCounterGroup(counterGroup);
        }
    }
}