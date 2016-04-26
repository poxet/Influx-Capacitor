using System.Diagnostics.CodeAnalysis;
using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Console.Commands.Config;
using Tharga.InfluxCapacitor.Console.Commands.Counter;
using Tharga.InfluxCapacitor.Console.Commands.Publish;
using Tharga.InfluxCapacitor.Console.Commands.Sender;
using Tharga.InfluxCapacitor.Console.Commands.Service;
using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Command;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console
{
    [ExcludeFromCodeCoverage]
    internal static class Program
    {
        private static CompositeRoot _compositeRoot;

        private static void Main(string[] args)
        {
            System.Console.Title = Constants.ServiceName + " Management Console";

            CounterBusiness.ChangedCurrentCultureEvent += CounterBusiness_ChangedCurrentCultureEvent;
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

        private static void CounterBusiness_ChangedCurrentCultureEvent(object sender, ChangedCurrentCultureEventArgs e)
        {
            System.Console.WriteLine("Changed culture from {0} to {1} to be able to read counters.", e.PreviousCulture, e.NewCulture);
        }

        private static void ClientConsole_KeyReadEvent(object sender, KeyReadEventArgs e)
        {
            _compositeRoot.Logger.Debug(string.Format("Key '{0}' pressed. ({1}.{2})", e.ReadKey.Key, e.ReadKey.KeyChar, e.ReadKey.Modifiers));
        }
    }
}