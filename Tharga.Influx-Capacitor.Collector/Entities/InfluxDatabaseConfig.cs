using System;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Influx_Capacitor.Interface;
using Tharga.Influx_Capacitor.Sender;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class InfluxDatabaseConfig : IDatabaseConfig
    {
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;
        private readonly string _name;
        private readonly TimeSpan? _requestTimeout;

        public InfluxDatabaseConfig(bool enabled, string url, string username, string password, string name, TimeSpan? requestTimeout)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url", "No url to influxDB provided.");

            IsEnabled = enabled;
            _url = url;
            _username = username;
            _password = password;
            _name = name;
            _requestTimeout = requestTimeout;
        }

        public bool IsEnabled { get; private set; }
        public string Url { get { return _url; } }
        public string Username { get { return _username; } }
        public string Password { get { return _password; } }
        public string Name { get { return _name; } }
        public TimeSpan? RequestTimeout { get { return _requestTimeout; } }

        public IDataSender GetDataSender(IInfluxDbAgentLoader influxDbAgentLoader, int maxQueueSize)
        {
            return new InfluxDataSender(new InfluxDataSenderConfiguration(IsEnabled, maxQueueSize, Url, Name, Username, Password, RequestTimeout));
        }
    }
}