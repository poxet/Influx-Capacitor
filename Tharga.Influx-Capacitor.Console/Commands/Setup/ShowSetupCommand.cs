using System.Threading.Tasks;
using InfluxDB.Net.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector.Console.Commands.Setup
{
    internal class ShowSetupCommand : ActionCommandBase
    {
        private readonly IInfluxDbAgentLoader _influxDbAgentLoader;
        private readonly IConfigBusiness _configBusiness;

        public ShowSetupCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
            : base("Show", "Show setup.")
        {
            _influxDbAgentLoader = influxDbAgentLoader;
            _configBusiness = configBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var config = _configBusiness.OpenDatabaseConfig();
            OutputInformation("Url:      {0}", config.Url);
            OutputInformation("Version:  {0}", config.InfluxDbVersion);
            OutputInformation("Name:     {0}", config.Name);
            OutputInformation("Username: {0}", config.Username);

            var client = _influxDbAgentLoader.GetAgent(config);
            OutputInformation("Connect:  {0}", await client.CanConnect());
            OutputInformation("Version:  {0}", await client.VersionAsync());

            return true;
        }
    }
}