using System.Diagnostics.CodeAnalysis;
using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Console.Commands.Config;
using Tharga.InfluxCapacitor.Console.Commands.Counter;
using Tharga.InfluxCapacitor.Console.Commands.Publish;
using Tharga.InfluxCapacitor.Console.Commands.Sender;
using Tharga.InfluxCapacitor.Console.Commands.Service;
using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Command;

namespace Tharga.InfluxCapacitor.Console
{
    [ExcludeFromCodeCoverage]
    internal static class Program
    {
        private static CompositeRoot _compositeRoot;

        private static void Main(string[] args)
        {
            System.Console.Title = Constants.ServiceName + " Management Console";
            _compositeRoot = new CompositeRoot();

            var clientConsole = _compositeRoot.ClientConsole;
            clientConsole.KeyReadEvent += ClientConsole_KeyReadEvent;
            var command = new RootCommand(clientConsole);

            command.RegisterCommand(new ConfigCommands(_compositeRoot));
            command.RegisterCommand(new ServiceCommands(_compositeRoot));
            command.RegisterCommand(new CounterCommands(_compositeRoot));
            command.RegisterCommand(new PublishCommands(_compositeRoot));
            command.RegisterCommand(new SenderCommands(_compositeRoot));

           new CommandEngine(command).Run(args);
        }

        private static void ClientConsole_KeyReadEvent(object sender, Toolkit.Console.Command.Base.KeyReadEventArgs e)
        {
            _compositeRoot.Logger.Debug(string.Format("Key '{0}' pressed. ({1}.{2})", e.ReadKey.Key, e.ReadKey.KeyChar, e.ReadKey.Modifiers));
        }
    }
}