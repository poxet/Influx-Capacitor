using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Console.Commands.Service;

namespace Tharga.InfluxCapacitor.Console.Commands.Config
{
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
            var config = new DatabaseConfig(currentConfig.Url, string.Empty, string.Empty, string.Empty, currentConfig.InfluxDbVersion);

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