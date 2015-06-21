using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using InfluxDB.Net.Collector.Agents;
using InfluxDB.Net.Collector.Business;
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

            var processor = new Processor(new ConfigBusiness(new FileLoaderAgent()), new CounterBusiness(), new InfluxDbAgentLoader());
            processor.NotificationEvent += NotificationEvent;
            Task.Factory.StartNew(() => processor.RunAsync(args));

            new CommandEngine(command).Run(new string[] { });
        }

        private static void NotificationEvent(object sender, NotificationEventArgs e)
        {
            _clientConsole.WriteLine(e.Message, OutputLevel.Information);
        }
    }
}