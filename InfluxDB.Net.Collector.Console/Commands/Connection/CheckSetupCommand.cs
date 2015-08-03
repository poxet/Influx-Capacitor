using System.Threading.Tasks;
using InfluxDB.Net.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector.Console.Commands.Connection
{
    internal class CheckSetupCommand : ActionCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly IInfluxDbAgentLoader _influxDbAgentLoader;

        public CheckSetupCommand(IConfigBusiness configBusiness, IInfluxDbAgentLoader influxDbAgentLoader)
            : base("Check", "Check connection")
        {
            _configBusiness = configBusiness;
            _influxDbAgentLoader = influxDbAgentLoader;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var connection = _configBusiness.OpenDatabaseConfig();

            var client = _influxDbAgentLoader.GetAgent(connection);
            var pong = await client.PingAsync();

            OutputInformation("Can Connect: {0}, version {1}", pong.Success, pong.Version);

            return true;
        }
    }
}