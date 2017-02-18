using System.Collections.Generic;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Console.Commands.Service;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Console.Commands.Config
{
    class ConfigureApplicationCommand : ConfigCommandBase
    {
        private readonly IConfigBusiness _configBusiness;

        public ConfigureApplicationCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness, IMetaDataBusiness metaDataBusiness)
            : base("Application", "Change the application configuration.", influxDbAgentLoader, configBusiness, metaDataBusiness)
        {
            _configBusiness = configBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var index = 0;
            var flushSecondsInterval = QueryParam<int>("Flush Seconds Interval", GetParam(paramList, index++));
            var debugMode = QueryParam("Debug Mode", GetParam(paramList, index++), new Dictionary<bool, string> { { false, "No" }, { true, "Yes" } });

            _configBusiness.SaveApplicationConfig(flushSecondsInterval, debugMode, true, 20000);

            var result = await ServiceCommands.GetServiceStatusAsync();
            if (result != null)
            {
                await ServiceCommands.RestartServiceAsync();
            }

            return true;
        }
    }
}