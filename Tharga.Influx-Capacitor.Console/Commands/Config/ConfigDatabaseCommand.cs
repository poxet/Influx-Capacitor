using System.Linq;
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
            : base("Database", "Change the database settings without changing server.", influxDbAgentLoader, configBusiness)
        {
            _configBusiness = configBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var configs = _configBusiness.OpenDatabaseConfig().ToArray();
            if (configs.Length > 1)
            {
                OutputWarning("There are {0} databases configured. When using multiple targets you will have to update the config files manually.", configs.Length);
                return false;
            }

            var currentConfig = configs.First();
            var config = new InfluxDatabaseConfig(true, currentConfig.Url, string.Empty, string.Empty, string.Empty, null);

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