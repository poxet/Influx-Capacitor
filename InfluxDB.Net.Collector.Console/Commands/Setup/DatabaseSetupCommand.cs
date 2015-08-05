using System.Threading.Tasks;
using InfluxDB.Net.Collector.Entities;
using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector.Console.Commands.Setup
{
    internal class DatabaseSetupCommand : SetupCommandBase
    {
        public DatabaseSetupCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
            : base("change", "Change setup.", influxDbAgentLoader, configBusiness)
        {
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var index = 0;

            var response = await GetServerUrlAsync(paramList, index++, null, InfluxDbVersion.Auto);
            if (string.IsNullOrEmpty(response.Item1))
                return false;

            var config = new DatabaseConfig(response.Item1, null, null, null, response.Item2);
            var logonInfo = await GetUsernameAsync(paramList, index++, config);
            if (logonInfo == null)
                return false;

            StartService();

            return true;
        }
    }
}