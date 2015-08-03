using System.Threading.Tasks;
using InfluxDB.Net.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector.Console.Commands.Connection
{
    internal class ShowSetupCommand : ActionCommandBase
    {
        private readonly IConfigBusiness _configBusiness;

        public ShowSetupCommand(IConfigBusiness configBusiness)
            : base("Show", "Show setup.")
        {
            _configBusiness = configBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var config = _configBusiness.OpenDatabaseConfig();

            OutputInformation("Url:      {0}", config.Url);
            OutputInformation("Name:     {0}", config.Name);
            OutputInformation("Username: {0}", config.Username);

            return true;
        }
    }
}