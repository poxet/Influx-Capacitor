using System.Threading.Tasks;
using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector.Console.Commands.Setup
{
    internal class DatabaseSetupCommand : SetupCommandBase
    {
        public DatabaseSetupCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
            : base("Database", "Setup the database", influxDbAgentLoader, configBusiness)
        {
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var index = 0;

            var response = await GetServerUrlAsync(paramList, index++, null, InfluxDbVersion.Auto);
            if (string.IsNullOrEmpty(response.Item1))
                return false;

            var logonInfo = await GetUsernameAsync(response.Item1, response.Item2, paramList, index++);
            if (logonInfo == null)
                return false;

            StartService();

            return true;
        }
    }
}