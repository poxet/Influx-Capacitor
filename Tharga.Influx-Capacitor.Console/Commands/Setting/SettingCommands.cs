using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Setting
{
    internal class SettingCommands : ContainerCommandBase
    {
        public SettingCommands(ICompositeRoot compositeRoot)
            : base("Setting")
        {
            RegisterCommand(new SettingAutoCommand(compositeRoot.InfluxDbAgentLoader, compositeRoot.ConfigBusiness));
            RegisterCommand(new SettingChangeCommand(compositeRoot.InfluxDbAgentLoader, compositeRoot.ConfigBusiness));
            RegisterCommand(new SettingShowCommand(compositeRoot.InfluxDbAgentLoader, compositeRoot.ConfigBusiness));
        }
    }
}