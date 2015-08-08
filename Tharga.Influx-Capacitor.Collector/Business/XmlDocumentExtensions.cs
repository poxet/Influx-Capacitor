using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;

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

        public IEnumerable<string> CreateAll()
        {
            yield return CreateProcessorCounter();
            yield return CreateMemoryCounter();
        }

        public Tuple<string, string> CreateCounter(string groupName, int secondsInterval, List<ICounter> counters)
        {
            var response = new CounterGroup(groupName, secondsInterval, counters);
            var message = CreateFile(groupName, response);
            return new Tuple<string, string>(groupName, message);
        }

        private string CreateProcessorCounter()
        {
            var name = "processor";

            var counters = new List<ICounter> { new Counter("Processor", "% Processor Time", "*") };
            var response = new CounterGroup(name, 10, counters);
            return CreateFile(name, response);
        }

        private string CreateMemoryCounter()
        {
            var name = "memory";

            var counters = new List<ICounter> { new Counter("Memory", "*") };
            var response = new CounterGroup(name, 10, counters);
            return CreateFile(name, response);
        }

        private string CreateFile(string name, CounterGroup response)
        {
            //Check if there is a counter with this name already
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();
            if (counterGroups.Any(x => x.Name == response.Name))
            {
                return string.Format("There is already a counter group named {0}.", response.Name);
                //OutputWarning("There is already a counter group named {0}.", response.Name);
            }
            else if (_configBusiness.CreateConfig(name + ".xml", new List<ICounterGroup> { response }))
            {
                return string.Format("Created counter config {0}.", name);
                //OutputInformation("Created counter config {0}.", name);
            }
            else
            {
                return string.Format("Did not create {0}, the file {0}.xml" + " already exists.", name);
                //OutputWarning("Did not create {0}, the file {0}.xml" + " already exists.", name);
            }
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