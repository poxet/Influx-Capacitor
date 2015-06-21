using InfluxDB.Net.Collector.Agents;
using InfluxDB.Net.Collector.Business;
using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector.Console.Commands
{
    internal class SettupCommands : ContainerCommandBase
    {
        public SettupCommands()
            : base("Setup")
        {
            //TODO: Inject before this point
            RegisterCommand(new AutoSetupCommand(new InfluxDbAgentLoader(), new ConfigBusiness(new FileLoaderAgent(), new RegistryRepository())));
        }
    }
}
