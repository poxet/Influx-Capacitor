using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector.Agents;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class ConsoleSendBusiness : ISendBusiness
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly IInfluxDbAgentLoader _influxDbAgentLoader;
        private readonly Action<string, OutputLevel> _outputMessage;

        public ConsoleSendBusiness(IConfigBusiness configBusiness, IInfluxDbAgentLoader influxDbAgentLoader, Action<string, OutputLevel> outputMessage)
        {
            _configBusiness = configBusiness;
            _influxDbAgentLoader = influxDbAgentLoader;
            _outputMessage = outputMessage;
        }

        public void Enqueue(Point[] points)
        {
            foreach (var config in _configBusiness.OpenDatabaseConfig())
            {
                IFormatter formatter;
                try
                {
                    var agent = _influxDbAgentLoader.GetAgent(config);
                    var agentInfo = agent.GetAgentInfo();
                    formatter = agentInfo.Item1;
                    _outputMessage("Send to " + config.Url + " ver " + agentInfo.Item2, OutputLevel.Information);
                }
                catch (InvalidOperationException)
                {
                    var ifx = new InfluxDb("http://influx-capacitor.com", "-", "-", InfluxVersion.v09x);
                    formatter = ifx.GetFormatter();
                    _outputMessage("Unknown client version, simulation output for version " + ifx.GetClientVersion() + ".", OutputLevel.Warning);
                }

                foreach (var point in points)
                {
                    _outputMessage(formatter.PointToString(point), OutputLevel.Information);
                }
            }
        }

        public event EventHandler<SendBusinessEventArgs> SendBusinessEvent;
    }

    internal class CounterDataCommand : CounterCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public CounterDataCommand(IConfigBusiness configBusiness, ICounterBusiness counterBusiness)
            : base("Data", "Reads the value from the performance counter and show what would be send to influxDB.")
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            var index = 0;
            var counterGroup = QueryParam("Group", GetParam(paramList, index++), counterGroups.Select(x => new KeyValuePair<IPerformanceCounterGroup, string>(x, x.Name)));

            using (var oneTimeDryRunCollectorEngine = new SafeCollectorEngine(counterGroup, new ConsoleSendBusiness(_configBusiness, new InfluxDbAgentLoader(), ShowOutput), new TagLoader(_configBusiness)))
            {
                oneTimeDryRunCollectorEngine.CollectRegisterCounterValuesEvent += CollectRegisterCounterValuesEvent;
                await oneTimeDryRunCollectorEngine.CollectRegisterCounterValuesAsync();
            }

            return true;
        }

        private void ShowOutput(string message, OutputLevel outputLevel)
        {
            OutputLine(message, outputLevel);
        }

        private void CollectRegisterCounterValuesEvent(object sender, CollectRegisterCounterValuesEventArgs e)
        {
            OutputLine(e.Message, e.OutputLevel);
        }    
    }

    internal class CounterReadCommand : CounterCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public CounterReadCommand(IConfigBusiness configBusiness, ICounterBusiness counterBusiness)
            : base("Read", "Reads the value from the performance counter.")
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            var index = 0;
            var counterGroup = QueryParam("Group", GetParam(paramList, index++), counterGroups.Select(x => new KeyValuePair<IPerformanceCounterGroup, string>(x, x.Name)));
            ReadCounterGroup(counterGroup);

            return true;
        }
    }
}