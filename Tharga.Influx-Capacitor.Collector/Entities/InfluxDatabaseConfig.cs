using System;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class InfluxDatabaseConfig : IDatabaseConfig
    {
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;
        private readonly string _name;

        public InfluxDatabaseConfig(string url, string username, string password, string name)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url", "No url to influxDB provided.");

            _url = url;
            _username = username;
            _password = password;
            _name = name;
        }

        public string Url { get { return _url; } }
        public string Username { get { return _username; } }
        public string Password { get { return _password; } }
        public string Name { get { return _name; } }

        public IDataSender GetDataSender(IInfluxDbAgentLoader influxDbAgentLoader)
        {
            return new InfluxDataSender(influxDbAgentLoader, this);
        }
    }
}