using System;
using System.Configuration;
using System.Linq;
using Tharga.InfluxCapacitor.Agents;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class KafkaDatabaseConfig : IDatabaseConfig
    {
        public KafkaDatabaseConfig(bool enabled, string url)
        {
            IsEnabled = enabled;
            Url = url;
        }

        public bool IsEnabled { get; private set; }
        public string Url { get; private set; }
        public string Username { get { throw new NotSupportedException(); } }
        public string Password { get { throw new NotSupportedException(); } }
        public string Name { get { return "N/A"; } }
        public TimeSpan? RequestTimeout { get { return null; } }

        public ISenderAgent GetSenderAgent()
        {
            if (string.IsNullOrEmpty(Url))
                throw new ConfigurationErrorsException("No Url property specified for Kafka.");

            var kafkaServers = Url.Split(';').Select(x => new Uri(x)).ToArray();
            return new KafkaSenderAgent(new KafkaAgent(kafkaServers));
        }
    }
}