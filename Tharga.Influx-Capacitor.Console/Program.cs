using System.Diagnostics.CodeAnalysis;
using Tharga.InfluxCapacitor.Console.Commands.Config;
using Tharga.InfluxCapacitor.Console.Commands.Counter;
using Tharga.InfluxCapacitor.Console.Commands.Processor;
using Tharga.InfluxCapacitor.Console.Commands.Service;
using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Command;

namespace Tharga.InfluxCapacitor.Console
{
    [ExcludeFromCodeCoverage]
    static class Program
    {
        private static void Main(string[] args)
        {
            var compositeRoot = new CompositeRoot();
            
            var command = new RootCommand(compositeRoot.ClientConsole);

            command.RegisterCommand(new ConfigCommands(compositeRoot));
            command.RegisterCommand(new ServiceCommands());
            command.RegisterCommand(new CounterCommands(compositeRoot));
            command.RegisterCommand(new ProcessorCommands(compositeRoot));

            new CommandEngine(command).Run(args);
        }
    }
}