using System;
using System.Collections.Generic;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class ConsoleSendBusiness : ISendBusiness
    {
        public ConsoleSendBusiness(IConfigBusiness configBusiness, IInfluxDbAgentLoader influxDbAgentLoader, Action<string, OutputLevel> outputMessage)
        {
        }

        public void Enqueue(Point[] points)
        {
        }

        public IEnumerable<IQueueCountInfo> GetQueueInfo()
        {
            yield break;
        }
    }
}