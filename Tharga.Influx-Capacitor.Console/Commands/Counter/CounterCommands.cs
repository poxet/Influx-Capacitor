using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class CounterCommands : ContainerCommandBase
    {
        public CounterCommands(ICompositeRoot compositeRoot)
            : base("Counter")
        {
            RegisterCommand(new CounterListCommand(compositeRoot.ConfigBusiness, compositeRoot.CounterBusiness));
            //RegisterCommand(new CounterDisableCommand(compositeRoot.ConfigBusiness, compositeRoot.CounterBusiness));
            RegisterCommand(new CounterReadCommand(compositeRoot.ConfigBusiness, compositeRoot.CounterBusiness));
            RegisterCommand(new InitiateDefaultCommand(compositeRoot.ConfigBusiness, compositeRoot.CounterBusiness));
            RegisterCommand(new ConfigCreateCommand(compositeRoot.ConfigBusiness, compositeRoot.CounterBusiness));
        }
    }
}
