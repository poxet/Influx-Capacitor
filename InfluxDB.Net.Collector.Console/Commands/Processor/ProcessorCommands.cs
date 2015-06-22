using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector.Console.Commands.Processor
{
    internal class ProcessorCommands : ContainerCommandBase
    {
        public ProcessorCommands(ICompositeRoot compositeRoot)
            : base("Processor")
        {
            RegisterCommand(new RunProcessorCommand(compositeRoot));
        }
    }
}