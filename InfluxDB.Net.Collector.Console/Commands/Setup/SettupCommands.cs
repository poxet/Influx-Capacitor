using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector.Console.Commands
{
    internal class SettupCommands : ContainerCommandBase
    {
        public SettupCommands(ICompositeRoot compositeRoot)
            : base("Setup")
        {
            RegisterCommand(new AutoSetupCommand(compositeRoot.InfluxDbAgentLoader, compositeRoot.ConfigBusiness));
        }
    }
}
