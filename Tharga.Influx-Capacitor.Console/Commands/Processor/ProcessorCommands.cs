using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Processor
{
    //TODO: Cleanup
    internal class ProcessorCommands : ContainerCommandBase
    {
        public ProcessorCommands(ICompositeRoot compositeRoot)
            : base("Processor")
        {
            RegisterCommand(new RunProcessorCommand(compositeRoot));
        }
    }
}