using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Console.Commands.Setting
{
    internal class SettingAutoCommand : SettingCommandBase
    {
        private readonly IConfigBusiness _configBusiness;

        public SettingAutoCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
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

            StartService(true);

            return true;
        }
    }
}