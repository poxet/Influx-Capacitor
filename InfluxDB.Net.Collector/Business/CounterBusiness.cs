using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                    var performanceCounter = GetPerformanceCounter(counter.CategoryName, counter.CounterName, counter.InstanceName);
                    if (performanceCounter != null)
                        performanceCounters.Add(new PerformanceCounterInfo(counter.Name, performanceCounter));
                }

                var performanceCounterGroup = new PerformanceCounterGroup(group.Name, group.SecondsInterval, performanceCounters);
                counterGroups.Add(performanceCounterGroup);
            }

            Thread.Sleep(100);

            return counterGroups;
        }

        private PerformanceCounter GetPerformanceCounter(string categoryName, string counterName, string instanceName)
        {
            try
            {
                if (instanceName == "*")
                {
                    //TODO: Get all instances of the counter.
                    var cats = PerformanceCounterCategory.GetCategories().Where(x => x.CategoryName == categoryName).ToArray();
                    var cnts = cats.SelectMany(xx => xx.GetInstanceNames().Select(y => xx.GetCounters(y))).ToArray();
                }

                var processorCounter = new PerformanceCounter(categoryName, counterName, instanceName);
                processorCounter.NextValue();
                return processorCounter;
            }
            catch (InvalidOperationException exception)
            {
                var message = exception.Message;
                message += " categoryName=" + categoryName + ", counterName=" + counterName + ", instanceName=" + (instanceName ?? "N/A");
                EventLog.WriteEntry("InfluxDB.Net.Collector", message, EventLogEntryType.Error);
                //TODO: Log when there is a counter that cannot be created
                return null;
            }
        }
    }
}