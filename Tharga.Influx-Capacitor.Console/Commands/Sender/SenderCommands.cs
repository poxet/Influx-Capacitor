using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Sender
{
    internal class SenderCommands : ContainerCommandBase
    {
        public SenderCommands(CompositeRoot compositeRoot)
            : base("Sender")
        {
            RegisterCommand(new SenderQueueCommand(compositeRoot));
        }
    }
}