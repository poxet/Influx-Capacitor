using System.Threading.Tasks;
using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector.Console.Commands.Setup
{
    internal class AutoSetupCommand : SetupCommandBase
    {
        private readonly IConfigBusiness _configBusiness;

        public AutoSetupCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
            : base("Auto", "Automatically run full setup.", influxDbAgentLoader, configBusiness)
        {
            _configBusiness = configBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var index = 0;

            var defaultUrl = _configBusiness.OpenDatabaseConfig().Url;

            var url = await GetServerUrlAsync(paramList, index++, defaultUrl);
            if (string.IsNullOrEmpty(url))
                return false;

            var config = _configBusiness.OpenDatabaseConfig();
            var logonInfo = await GetUsernameAsync(paramList, index++, config);
            if (logonInfo == null)
                return false;

            StartService();

            return true;
        }
    }
}