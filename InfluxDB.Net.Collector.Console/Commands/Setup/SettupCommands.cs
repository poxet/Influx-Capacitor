using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector.Console.Commands.Setup
{
    internal class SettupCommands : ContainerCommandBase
    {
        public SettupCommands(ICompositeRoot compositeRoot)
            : base("Setup")
        {
            RegisterCommand(new AutoSetupCommand(compositeRoot.InfluxDbAgentLoader, compositeRoot.ConfigBusiness));
            RegisterCommand(new DatabaseSetupCommand(compositeRoot.InfluxDbAgentLoader, compositeRoot.ConfigBusiness));
            RegisterCommand(new ShowSetupCommand(compositeRoot.InfluxDbAgentLoader, compositeRoot.ConfigBusiness));
        }
    }
}