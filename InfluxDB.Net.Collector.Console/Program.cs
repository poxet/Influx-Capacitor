using System;

namespace InfluxDB.Net.Collector.Console
{
    static class Program
    {
        private static InfluxDb _client;

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                //url:      http://128.199.43.107:8086
                //username: root
                //Password: ?????
                throw new InvalidOperationException("Three parameters needs to be provided to this application. url, username and password for the InfluxDB database.");
            }

            var url = args[0];
            var username = args[1];
            var password = args[2];

            _client = new InfluxDb(url, username, password);

            var pong = _client.PingAsync().Result;
            System.Console.WriteLine("Ping: {0} ({1} ms)", pong.Status, pong.ResponseTime);

            var version = _client.VersionAsync().Result;
            System.Console.WriteLine("Version: {0}", version);

            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();
        }
    }
}