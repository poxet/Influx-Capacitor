using System;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Interface;
using Tharga.InfluxCapacitor.Sender;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class InfluxDatabaseConfig : IDatabaseConfig
    {
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;
        private readonly string _name;

        public InfluxDatabaseConfig(bool enabled, string url, string username, string password, string name)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url", "No url to influxDB provided.");

            IsEnabled = enabled;
            _url = url;
            _username = username;
            _password = password;
            _name = name;
        }

        public bool IsEnabled { get; private set; }
        public string Url { get { return _url; } }
        public string Username { get { return _username; } }
        public string Password { get { return _password; } }
        public string Name { get { return _name; } }

        public IDataSender GetDataSender(IInfluxDbAgentLoader influxDbAgentLoader, int maxQueueSize)
        {
            return new InfluxDataSender(new InfluxDataSenderConfiguration(IsEnabled, maxQueueSize, Url, Name, Username, Password));
        }
    }
}