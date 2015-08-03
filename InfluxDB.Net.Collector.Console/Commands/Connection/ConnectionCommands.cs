using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector.Console.Commands.Connection
{
    internal class ConnectionCommands : ContainerCommandBase
    {
        public ConnectionCommands(ICompositeRoot compositeRoot)
            : base("Connection")
        {
            RegisterCommand(new ShowSetupCommand(compositeRoot.ConfigBusiness, compositeRoot.InfluxDbAgentLoader));
            RegisterCommand(new CheckSetupCommand(compositeRoot.ConfigBusiness, compositeRoot.InfluxDbAgentLoader));
        }
    }
}