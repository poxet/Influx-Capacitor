using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Console.Commands.Service;

namespace Tharga.InfluxCapacitor.Console.Commands.Config
{
    internal class ConfigServerCommand : ConfigCommandBase
    {
        private readonly IConfigBusiness _configBusiness;

        public ConfigServerCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
            : base("Server", "Change the database and server settings.", influxDbAgentLoader, configBusiness)
        {
            _configBusiness = configBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var index = 0;

            var configs = _configBusiness.OpenDatabaseConfig().ToArray();
            if (configs.Length > 1)
            {
                OutputWarning("There are {0} database targets configured. When using multiple targets you will have to update the config files manually.", configs.Length);
                return false;
            }

            var response = await GetServerUrlAsync(paramList, index++, null);
            if (string.IsNullOrEmpty(response))
                return false;

            var config = new InfluxDatabaseConfig(true, response, null, null, null, null);
            var logonInfo = await GetUsernameAsync(paramList, index++, config, "config_change");
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