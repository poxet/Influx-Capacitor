using Tharga.InfluxCapacitor.Service;
using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Command;

namespace Tharga.Influx_Capacitor.Service.Console
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var ws = new WindowsService();

            var command = new RootCommand(ws.Console);
            var engine = new CommandEngine(command);
            ws.Start(args);
            engine.Run(args);
        }
    }
}