using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Console.Commands.Counter;
using Tharga.InfluxCapacitor.Console.Commands.Service;

namespace Tharga.InfluxCapacitor.Console.Commands.Config
{
    internal class ConfigAutoCommand : ConfigCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public ConfigAutoCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness, ICounterBusiness counterBusiness)
            : base("Auto", "Automatically run full setup.", influxDbAgentLoader, configBusiness)
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
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

            var initaiteBusiness = new InitaiteBusiness(_configBusiness, _counterBusiness);
            var messages = initaiteBusiness.CreateAll();
            foreach(var message in messages)
                OutputInformation(message);

            return true;
        }
    }
}