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
        private static RootCommand _command;

        private static void Main(string[] args)
        {
            System.Console.Title = Constants.ServiceName + " Management Console";
            _compositeRoot = new CompositeRoot();

            var clientConsole = _compositeRoot.ClientConsole;
            clientConsole.KeyReadEvent += ClientConsole_KeyReadEvent;
            _compositeRoot.CounterBusiness.ChangedCurrentCultureEvent += CounterBusiness_ChangedCurrentCultureEvent;
            _compositeRoot.CounterBusiness.GetPerformanceCounterEvent += CounterBusiness_GetPerformanceCounterEvent;
            _command = new RootCommand(clientConsole);

            _command.RegisterCommand(new ConfigCommands(_compositeRoot));
            _command.RegisterCommand(new ServiceCommands(_compositeRoot));
            _command.RegisterCommand(new CounterCommands(_compositeRoot));
            _command.RegisterCommand(new PublishCommands(_compositeRoot));
            _command.RegisterCommand(new SenderCommands(_compositeRoot));


           new CommandEngine(_command).Run(args);
        }

        private static void CounterBusiness_GetPerformanceCounterEvent(object sender, Collector.Event.GetPerformanceCounterEventArgs e)
        {
            _command.OutputWarning(e.Message);
        }

        private static void CounterBusiness_ChangedCurrentCultureEvent(object sender, Collector.Event.ChangedCurrentCultureEventArgs e)
        {
            _command.OutputInformation(string.Format("Changed culture from {0} to {1}.", e.PreviousCulture, e.NewCulture));
        }

        private static void ClientConsole_KeyReadEvent(object sender, Toolkit.Console.Command.Base.KeyReadEventArgs e)
        {
            _compositeRoot.Logger.Debug(string.Format("Key '{0}' pressed. ({1}.{2})", e.ReadKey.Key, e.ReadKey.KeyChar, e.ReadKey.Modifiers));
        }
    }
}