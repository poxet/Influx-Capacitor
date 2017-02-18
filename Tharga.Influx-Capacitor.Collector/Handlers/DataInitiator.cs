using System;
using System.Collections.Generic;
using System.Linq;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Collector.Handlers
{
    public class DataInitiator
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public DataInitiator(IConfigBusiness configBusiness, ICounterBusiness counterBusiness)
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
        }

        public IEnumerable<Tuple<string, OutputLevel>> CreateAll()
        {
            yield return CreateProcessorCounter();
            yield return CreateMemoryCounter();
            yield return CreateDiskCounter();
        }

        public Tuple<string, Tuple<string, OutputLevel>> CreateCounter(string groupName, int secondsInterval, int refreshInstanceInterval, List<ICounter> counters, CollectorEngineType collectorEngineType)
        {
            var response = new CounterGroup(groupName, secondsInterval, refreshInstanceInterval, counters, new ITag[] { }, collectorEngineType, null, null, false);
            var message = CreateFile(groupName, response);
            return new Tuple<string, Tuple<string, OutputLevel>>(groupName, message);
        }

        private Tuple<string, OutputLevel> CreateProcessorCounter()
        {
            var name = "processor";

            var counters = new List<ICounter>
            {
                new Counter("Processor", new Naming("% Processor Time"), new Naming("*"), null, null, 100, 0, null, null),
                new Counter("Processor", new Naming("% Idle Time"), new Naming("*"), null, null, 100, 0, null, null),
                new Counter("Processor", new Naming("% Processor Time","% Idle Processor Time"), new Naming("*"), null, null, 100, 0, null, 100)
            };
            var response = new CounterGroup(name, 10, 0, counters, new ITag[] { }, CollectorEngineType.Safe, null, null, false);
            return ConvertErrorsToWarnings(CreateFile(name, response));
        }

        private Tuple<string, OutputLevel> CreateMemoryCounter()
        {
            var name = "memory";

            var counters = new List<ICounter>
            {
                //new Counter("Memory", new Naming("*"), new Naming(string.Empty), null, null, null, null, null, null),
                new Counter("Memory", new Naming("% Committed Bytes In Use"), new Naming(string.Empty), null, null, null, null, null, null),
                new Counter("Memory", new Naming("% Committed Bytes In Use","% Committed Bytes Free"), new Naming(string.Empty), null, null, null, null, null, 100),
            };
            var response = new CounterGroup(name, 10, 0, counters, new ITag[] { }, CollectorEngineType.Safe, null, null, false);
            return ConvertErrorsToWarnings(CreateFile(name, response));
        }

        private Tuple<string, OutputLevel> CreateDiskCounter()
        {
            var name = "disk";

            var counters = new List<ICounter>
            {
                new Counter("LogicalDisk", new Naming("Free Megabytes"), new Naming("*"), null, null, null, null, null, null),
                new Counter("LogicalDisk", new Naming("% Free Space"), new Naming("*"), null, null, 100, 0, null, null),
                new Counter("LogicalDisk", new Naming("% Free Space", "% Used Space"), new Naming("*"), null, null, 100, 0, null, 100),
            };
            var response = new CounterGroup(name, 600, 0, counters, new ITag[] { }, CollectorEngineType.Safe, null, null, false);
            return ConvertErrorsToWarnings(CreateFile(name, response));
        }

        private static Tuple<string, OutputLevel> ConvertErrorsToWarnings(Tuple<string, OutputLevel> result)
        {
            return result.Item2 == OutputLevel.Error ? new Tuple<string, OutputLevel>(result.Item1, OutputLevel.Warning) : result;
        }

        private Tuple<string,OutputLevel> CreateFile(string name, CounterGroup counterGroup)
        {
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            if (counterGroups.Any(x => x.Name == counterGroup.Name))
            {
                return new Tuple<string, OutputLevel>(string.Format("There is already a counter group named {0}.", counterGroup.Name), OutputLevel.Error);
            }

            if (!_configBusiness.CreateConfig(name + ".xml", new List<ICounterGroup> { counterGroup }))
            {
                return new Tuple<string, OutputLevel>(string.Format("Did not create {0}, the file {0}.xml" + " already exists.", name), OutputLevel.Error);
            }

            return new Tuple<string, OutputLevel>(string.Format("Created counter config {0}.", name), OutputLevel.Information);
        }
    }
}