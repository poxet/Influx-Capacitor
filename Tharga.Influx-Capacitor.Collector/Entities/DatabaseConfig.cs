using System;
using InfluxDB.Net;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class DatabaseConfig : IDatabaseConfig
    {
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;
        private readonly string _name;
        private readonly InfluxDbVersion _influxDbVersion;

        public DatabaseConfig(string url, string username, string password, string name, InfluxDbVersion influxDbVersion)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url", "No url to influxDB provided.");

            _url = url;
            _username = username;
            _password = password;
            _name = name;
            _influxDbVersion = influxDbVersion;
        }

        public string Url { get { return _url; } }
        public string Username { get { return _username; } }
        public string Password { get { return _password; } }
        public string Name { get { return _name; } }
        public InfluxDbVersion InfluxDbVersion { get { return _influxDbVersion; } }
        public int FlushSecondsInterval { get { return 10; } } //TODO: Make this configurable
    }
}