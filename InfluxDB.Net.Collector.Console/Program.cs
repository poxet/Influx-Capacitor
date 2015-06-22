using System.Diagnostics.CodeAnalysis;
using InfluxDB.Net.Collector.Console.Commands;
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
            _clientConsole = new ClientConsole();
            var command = new RootCommand(_clientConsole);

            command.RegisterCommand(new SettupCommands());

            ////TODO: Inject before this point
            //var processor = new Processor(new ConfigBusiness(new FileLoaderAgent(), new RegistryRepository()), new CounterBusiness(), new InfluxDbAgentLoader());
            //processor.NotificationEvent += NotificationEvent;
            //Task.Factory.StartNew(() => processor.RunAsync(new string[]{}));

            new CommandEngine(command).Run(args);
        }

        private static void NotificationEvent(object sender, NotificationEventArgs e)
        {
            _clientConsole.WriteLine(e.Message, OutputLevel.Information);
        }
    }
}