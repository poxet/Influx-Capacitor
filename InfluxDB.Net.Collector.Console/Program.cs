using System.Diagnostics.CodeAnalysis;
using InfluxDB.Net.Collector.Console.Commands;
using InfluxDB.Net.Collector.Console.Commands.Processor;
using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Command;

namespace InfluxDB.Net.Collector.Console
{
    [ExcludeFromCodeCoverage]
    static class Program
    {
        private static void Main(string[] args)
        {
            var compositeRoot = new CompositeRoot();
            
            var command = new RootCommand(compositeRoot.ClientConsole);

            command.RegisterCommand(new SettupCommands(compositeRoot));
            command.RegisterCommand(new ProcessorCommands(compositeRoot));

            new CommandEngine(command).Run(args);
        }
    }
}