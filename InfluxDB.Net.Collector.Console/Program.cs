using InfluxDB.Net.Collector.Business;

namespace InfluxDB.Net.Collector.Console
{
    static class Program
    {
        static void Main(string[] args)
        {
            var processor = new Processor(new ConfigBusiness(), new CounterBusiness());
            processor.Run(args);

            System.Console.WriteLine("Press enter to exit...");
            System.Console.ReadKey();
        }
    }
}