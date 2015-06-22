using System.Diagnostics.CodeAnalysis;
using InfluxDB.Net.Collector.Console.Commands;
using InfluxDB.Net.Collector.Console.Commands.Processor;
using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Command;
using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector.Console
{
    [ExcludeFromCodeCoverage]
    static class Program
    {
        private static ClientConsole _clientConsole;

        private static void Main(string[] args)
        {
            var compositeRoot = new CompositeRoot();

            _clientConsole = new ClientConsole();
            var command = new RootCommand(_clientConsole);

            command.RegisterCommand(new SettupCommands(compositeRoot));
            command.RegisterCommand(new ProcessorCommands(compositeRoot));

            new CommandEngine(command).Run(args);
        }

        private static void NotificationEvent(object sender, NotificationEventArgs e)
        {
            _clientConsole.WriteLine(e.Message, OutputLevel.Information);
        }
    }
}