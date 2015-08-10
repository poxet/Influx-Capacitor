using System.Threading.Tasks;
using InfluxDB.Net;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Console.Commands.Service;

namespace Tharga.InfluxCapacitor.Console.Commands.Config
{
    internal class ConfigServerCommand : ConfigCommandBase
    {
        public ConfigServerCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
            : base("Change", "Change connection for server and database.", influxDbAgentLoader, configBusiness)
        {
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var index = 0;            

            var response = await GetServerUrlAsync(paramList, index++, null, InfluxDbVersion.Auto);
            if (string.IsNullOrEmpty(response.Item1))
                return false;

            var config = new DatabaseConfig(response.Item1, null, null, null, response.Item2);
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