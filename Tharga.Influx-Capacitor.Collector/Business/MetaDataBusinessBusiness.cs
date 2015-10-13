using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class MetaDataBusiness : IMetaDataBusiness
    {
        public static async Task<InfluxDbApiResponse> TestWriteAccess(IInfluxDbAgent client, string action)
        {
            var points = new[]
            {
                new Point
                    {
                        Name = Constants.ServiceName, 
                        Tags = new Dictionary<string, string>
                        {
                            { "hostname", Environment.MachineName },
                            { "version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                            { "action", action },
                        },
                        Fields = new Dictionary<string, object>
                        {
                            { "value", 1 }
                        },
                        Precision = TimeUnit.Milliseconds,
                        Timestamp = DateTime.UtcNow
                    },
            };
            return await client.WriteAsync(points);
        }

        public static Point GetQueueCountPoints(string action, string targetServer, string targetDatabase, int previousQueueCount, int queueCountChange)
        {
            var point = new Point
                            {
                                Name = Constants.ServiceName,
                                Tags = new Dictionary<string, string>
                                           {
                                               { "hostname", Environment.MachineName },
                                               { "version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                                               { "action", action },
                                               { "targetServer", targetServer },
                                               { "targetDatabase", targetDatabase },
                                           },
                                Fields = new Dictionary<string, object>
                                             {
                                                 { "value", previousQueueCount + queueCountChange },
                                                 { "change", queueCountChange },
                                                 { "previousQueueCount", previousQueueCount },
                                             },
                                Precision = TimeUnit.Milliseconds,
                                Timestamp = DateTime.UtcNow
                            };
            return point;
        }
    }
}