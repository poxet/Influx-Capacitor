using System.Diagnostics.CodeAnalysis;
using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Console.Commands.Config;
using Tharga.InfluxCapacitor.Console.Commands.Counter;
using Tharga.InfluxCapacitor.Console.Commands.Service;
using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Command;

namespace Tharga.InfluxCapacitor.Console
{
    [ExcludeFromCodeCoverage]
    internal static class Program
    {
        private static void Main(string[] args)
        {
            System.Console.Title = Constants.ServiceName + " Management Console";
            var compositeRoot = new CompositeRoot();

            var command = new RootCommand(compositeRoot.ClientConsole);

            command.RegisterCommand(new ConfigCommands(compositeRoot));
            command.RegisterCommand(new ServiceCommands(compositeRoot));
            command.RegisterCommand(new CounterCommands(compositeRoot));

            new CommandEngine(command).Run(args);
        }
    }
}