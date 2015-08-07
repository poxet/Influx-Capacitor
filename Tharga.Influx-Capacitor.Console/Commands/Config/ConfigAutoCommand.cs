using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Console.Commands.Service;

namespace Tharga.InfluxCapacitor.Console.Commands.Config
{
    internal class ConfigAutoCommand : ConfigCommandBase
    {
        private readonly IConfigBusiness _configBusiness;

        public ConfigAutoCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
            : base("Auto", "Automatically run full setup.", influxDbAgentLoader, configBusiness)
        {
            _configBusiness = configBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var index = 0;
            
            var defaultUrl = _configBusiness.OpenDatabaseConfig().Url;
            var influxDbVersion = _configBusiness.OpenDatabaseConfig().InfluxDbVersion;

            var response = await GetServerUrlAsync(paramList, index++, defaultUrl, influxDbVersion);
            if (string.IsNullOrEmpty(response.Item1)) 
                return false;

            var config = _configBusiness.OpenDatabaseConfig();
            var logonInfo = await GetUsernameAsync(paramList, index++, config);
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