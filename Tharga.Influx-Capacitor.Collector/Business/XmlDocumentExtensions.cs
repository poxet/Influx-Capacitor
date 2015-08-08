using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class InitaiteBusiness
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public InitaiteBusiness(IConfigBusiness configBusiness, ICounterBusiness counterBusiness)
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
        }

        public IEnumerable<Tuple<string,OutputLevel>> CreateAll()
        {
            yield return CreateProcessorCounter();
            yield return CreateMemoryCounter();
        }

        public Tuple<string, Tuple<string, OutputLevel>> CreateCounter(string groupName, int secondsInterval, List<ICounter> counters)
        {
            var response = new CounterGroup(groupName, secondsInterval, counters);
            var message = CreateFile(groupName, response);
            return new Tuple<string, Tuple<string, OutputLevel>>(groupName, message);
        }

        private Tuple<string, OutputLevel> CreateProcessorCounter()
        {
            var name = "processor";

            var counters = new List<ICounter> { new Counter("Processor", "% Processor Time", "*") };
            var response = new CounterGroup(name, 10, counters);
            return ConvertErrorsToWarnings(CreateFile(name, response));
        }

        private Tuple<string, OutputLevel> CreateMemoryCounter()
        {
            var name = "memory";

            var counters = new List<ICounter> { new Counter("Memory", "*") };
            var response = new CounterGroup(name, 10, counters);
            return ConvertErrorsToWarnings(CreateFile(name, response));
        }

        private static Tuple<string, OutputLevel> ConvertErrorsToWarnings(Tuple<string, OutputLevel> result)
        {
            return result.Item2 == OutputLevel.Error ? new Tuple<string, OutputLevel>(result.Item1, OutputLevel.Warning) : result;
        }

        private Tuple<string,OutputLevel> CreateFile(string name, CounterGroup response)
        {
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            if (counterGroups.Any(x => x.Name == response.Name))
            {
                return new Tuple<string, OutputLevel>(string.Format("There is already a counter group named {0}.", response.Name), OutputLevel.Error);
            }

            if (!_configBusiness.CreateConfig(name + ".xml", new List<ICounterGroup> { response }))
            {
                return new Tuple<string, OutputLevel>(string.Format("Did not create {0}, the file {0}.xml" + " already exists.", name), OutputLevel.Error);
            }

            return new Tuple<string, OutputLevel>(string.Format("Created counter config {0}.", name), OutputLevel.Information);
        }
    }

    public static class XmlDocumentExtensions
    {
        public static string ToFormattedString(this XmlDocument document)
        {
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\r\n", NewLineHandling = NewLineHandling.Replace };

            using (var writer = XmlWriter.Create(sb, settings))
            {
                document.Save(writer);
            }

            var contents = sb.ToString();
            return contents;
        }
    }
}