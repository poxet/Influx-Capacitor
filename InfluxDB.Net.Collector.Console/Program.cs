using InfluxDB.Net.Collector.Business;
using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Command;
using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector.Console
{
    static class Program
    {
        private static ClientConsole _clientConsole;

        private static void Main(string[] args)
        {
            _clientConsole = new ClientConsole();
            var command = new RootCommand(_clientConsole);

            var processor = new Processor(new ConfigBusiness(new FileLoader()), new CounterBusiness());
            processor.NotificationEvent += NotificationEvent;
            processor.Run(args);

            new CommandEngine(command).Run(new string[] { });
        }

        private static void NotificationEvent(object sender, NotificationEventArgs e)
        {
            _clientConsole.WriteLine(e.Message, OutputLevel.Information);
        }
    }
}