using System;
using System.Collections.Generic;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Entities;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class ConsoleSendBusiness : ISendBusiness
    {
        //private readonly IConfigBusiness _configBusiness;
        //private readonly IInfluxDbAgentLoader _influxDbAgentLoader;
        //private readonly Action<string, OutputLevel> _outputMessage;

        public event EventHandler<SendCompleteEventArgs> SendBusinessEvent;

        public ConsoleSendBusiness(IConfigBusiness configBusiness, IInfluxDbAgentLoader influxDbAgentLoader, Action<string, OutputLevel> outputMessage)
        {
        //    _configBusiness = configBusiness;
        //    _influxDbAgentLoader = influxDbAgentLoader;
        //    _outputMessage = outputMessage;
        }

        public void Enqueue(Point[] points)
        {
        //    foreach (var config in _configBusiness.OpenDatabaseConfig())
        //    {
        //        IFormatter formatter;
        //        try
        //        {
        //            var agent = _influxDbAgentLoader.GetAgent(config);
        //            var agentInfo = agent.GetAgentInfo();
        //            formatter = agentInfo.Item1;
        //            _outputMessage("Send to " + config.Url + " ver " + agentInfo.Item2, OutputLevel.Information);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            var ifx = new InfluxDb("http://influx-capacitor.com", "-", "-", InfluxVersion.v09x);
        //            formatter = ifx.GetFormatter();
        //            _outputMessage("Unknown client version, simulation output for version " + ifx.GetClientVersion() + ".", OutputLevel.Warning);
        //        }

        //        foreach (var point in points)
        //        {
        //            _outputMessage(formatter.PointToString(point), OutputLevel.Information);
        //        }
        //    }
        }

        public IEnumerable<IQueueCountInfo> GetQueueInfo()
        {
            //throw new NotImplementedException();
            yield break;
        }
    }
}