using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using KafkaNet;
using KafkaNet.Model;
using KafkaNet.Protocol;

namespace Tharga.InfluxCapacitor.Collector.Agents
{
    [ExcludeFromCodeCoverage]
    public class KafkaAgent : IDisposable
    {
        private readonly BrokerRouter _router;

        public KafkaAgent(Uri[] kafkaServers)
        {
            var options = new KafkaOptions(kafkaServers);
            _router = new BrokerRouter(options);
        }

        public void Send(Point[] points)
        {
            using (var client = new Producer(_router))
            {
                //TODO: Convert to json
                var messages = points.Select(x => new Message(x.Name)).ToArray();
                //var messages = points.Select(x => new Message(x.ToJson())).ToArray();
                //var messages = points.Select(x => new Message(x.ToString())).ToArray();
                client.SendMessageAsync("InfluxCapacitor", messages).Wait();
            }
        }

        public void Dispose()
        {
            _router.Dispose();
        }
    }
}