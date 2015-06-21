using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using InfluxDB.Net.Collector.Console.Entities;

namespace InfluxDB.Net.Collector.Console.Business
{
    internal class CounterBusiness
    {
        public List<PerformanceCounterGroup> GetPerformanceCounterGroups(Config config)
        {
            var counterGroups = new List<PerformanceCounterGroup>();

            foreach (var group in config.Groups)
            {
                var performanceCounters = new List<PerformanceCounter>();
                foreach (var counter in group.Counters)
                {
                    performanceCounters.Add(GetPerformanceCounter(counter.CategoryName, counter.CounterName, counter.InstanceName));
                }

                var performanceCounterGroup = new PerformanceCounterGroup(group.Name, group.SecondsInterval, performanceCounters);
                counterGroups.Add(performanceCounterGroup);
            }

            Thread.Sleep(100);

            return counterGroups;
        }

        private PerformanceCounter GetPerformanceCounter(string categoryName, string counterName, string instanceName)
        {
            var processorCounter = new PerformanceCounter(categoryName, counterName, instanceName);
            processorCounter.NextValue();
            return processorCounter;
        }
    }
}