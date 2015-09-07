using System.Collections.Generic;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Console.Commands.Service;

namespace Tharga.InfluxCapacitor.Console.Commands.Config
{
    class ConfigureApplicationCommand : ConfigCommandBase
    {
        private readonly IConfigBusiness _configBusiness;

        public ConfigureApplicationCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
            : base("Application", "Change the application configuration.", influxDbAgentLoader, configBusiness)
        {
            _configBusiness = configBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var index = 0;
            var flushSecondsInterval = QueryParam<int>("Flush Seconds Interval", GetParam(paramList, index++));
            var debugMode = QueryParam<bool>("Debug Mode", GetParam(paramList, index++), new Dictionary<bool, string> { { false, "No" }, { true, "Yes" } });

            _configBusiness.SaveApplicationConfig(flushSecondsInterval, debugMode);

            var result = await ServiceCommands.GetServiceStatusAsync();
            if (result != null)
            {
                await ServiceCommands.RestartServiceAsync();
            }

            return true;
        }
    }

    class ConfigDatabaseCommand : ConfigCommandBase
    {
        private readonly IConfigBusiness _configBusiness;

        public ConfigDatabaseCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
            : base("Database", "Change the database settings for the current server.", influxDbAgentLoader, configBusiness)
        {
            _configBusiness = configBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var currentConfig = _configBusiness.OpenDatabaseConfig();
            var config = new DatabaseConfig(currentConfig.Url, string.Empty, string.Empty, string.Empty);

            var index = 0;
            var logonInfo = await GetUsernameAsync(paramList, index++, config, "config_database");
            if (logonInfo == null)
                return false;

            var result = await ServiceCommands.GetServiceStatusAsync();
            if (result != null)
            {
                await ServiceCommands.RestartServiceAsync();
            }

            return true;
        }
    }
}