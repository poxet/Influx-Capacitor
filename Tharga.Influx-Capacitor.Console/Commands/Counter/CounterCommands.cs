using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class CounterCommands : ContainerCommandBase
    {
        public CounterCommands(ICompositeRoot compositeRoot)
            : base("Counter")
        {
            RegisterCommand(new ListCountersCommand(compositeRoot.ConfigBusiness, compositeRoot.CounterBusiness));
        }
    }
}
