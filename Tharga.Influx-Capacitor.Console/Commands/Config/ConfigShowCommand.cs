using System;
using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Config
{
    internal class ConfigShowCommand : ActionCommandBase
    {
        private readonly IInfluxDbAgentLoader _influxDbAgentLoader;
        private readonly IConfigBusiness _configBusiness;

        public ConfigShowCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
            : base("Show", "Show setup.")
        {
            _influxDbAgentLoader = influxDbAgentLoader;
            _configBusiness = configBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var configs = _configBusiness.OpenDatabaseConfig().ToArray();

            if (!configs.Any() || configs.All(x => x.Url == Constants.NoConfigUrl))
            {
                OutputWarning("No database configuration exists.");
                return false;
            }

            foreach (var config in  configs)
            {
                if (config.Url == Constants.NoConfigUrl)
                {
                    continue;
                }

                OutputInformation("Url:      {0}", config.Url);
                OutputInformation("Name:     {0}", config.Name);
                OutputInformation("Username: {0}", config.Username);

                try
                {
                    var client = _influxDbAgentLoader.GetAgent(config);
                    OutputInformation("Connect:  {0}", await client.CanConnect());
                    OutputInformation("Version:  {0}", await client.VersionAsync());
                }
                catch (InvalidOperationException)
                {
                    OutputError("Unable to connect to database.");
                }
                OutputInformation("");
            }

            return true;
        }
    }
}