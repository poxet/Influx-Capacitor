using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using InfluxDB.Net.Collector.Entities;
using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector.Business
{
    public class CounterBusiness : ICounterBusiness
    {
        public List<IPerformanceCounterGroup> GetPerformanceCounterGroups(IConfig config)
        {
            if (config.Groups == null) throw new NullReferenceException("No groups in config.");

            var counterGroups = new List<IPerformanceCounterGroup>();

            foreach (var group in config.Groups)
            {
                var performanceCounters = new List<IPerformanceCounterInfo>();
                foreach (var counter in group.Counters)
                {
                    performanceCounters.Add(new PerformanceCounterInfo(counter.Name, GetPerformanceCounter(counter.CategoryName, counter.CounterName, counter.InstanceName)));
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