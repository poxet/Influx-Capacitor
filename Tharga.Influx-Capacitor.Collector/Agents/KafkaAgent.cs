//using System;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using InfluxDB.Net;
//using InfluxDB.Net.Contracts;
//using InfluxDB.Net.Enums;
//using InfluxDB.Net.Models;
//using KafkaNet;
//using KafkaNet.Model;
//using KafkaNet.Protocol;

//namespace Tharga.InfluxCapacitor.Collector.Agents
//{
//    [ExcludeFromCodeCoverage]
//    public class KafkaAgent : IDisposable
//    {
//        private readonly BrokerRouter _router;
//        private readonly IFormatter _formatter;

//        public KafkaAgent(Uri[] kafkaServers)
//        {
//            var options = new KafkaOptions(kafkaServers);
//            _router = new BrokerRouter(options);

//            var influxDbClient = new InfluxDb("http://localhost", "reapadda", "qwerty", InfluxVersion.v09x);
//            _formatter = influxDbClient.GetFormatter();
//        }

//        public void Send(Point[] points)
//        {
//            using (var client = new Producer(_router))
//            {
//                var messages = points.Select(x =>
//                    {
//                        var pointToString = _formatter.PointToString(x);
//                        return new Message(pointToString);
//                    }).ToArray();
//                client.SendMessageAsync("InfluxCapacitor", messages).Wait();
//            }
//        }

//        public void Dispose()
//        {
//            _router.Dispose();
//        }
//    }
//}