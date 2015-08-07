using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Config
{
    internal class ConfigListCommand : ActionCommandBase
    {
        private readonly IConfigBusiness _configBusiness;

        public ConfigListCommand(IConfigBusiness configBusiness)
            : base("List", "Lists all available configuration files.")
        {
            _configBusiness = configBusiness;
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            var configFiles = _configBusiness.GetConfigFiles().ToArray();
            foreach (var configFile in configFiles)
            {
                OutputInformation("{0}", configFile);
            }
            OutputInformation("{0} files.", configFiles.Count());

            return true;
        }
    }
}