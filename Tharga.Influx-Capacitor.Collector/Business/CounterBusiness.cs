using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class CounterBusiness : ICounterBusiness
    {
        public List<IPerformanceCounterGroup> GetPerformanceCounterGroups(IConfig config)
        {
            if (config.Groups == null) throw new NullReferenceException("No groups in config.");

            var counterGroups = new List<IPerformanceCounterGroup>();

            foreach (var group in config.Groups)
            {
                var performanceCounterInfos = new List<IPerformanceCounterInfo>();
                foreach (var counter in group.Counters)
                {
                    var performanceCounters = GetPerformanceCounters(counter.CategoryName, counter.CounterName, counter.InstanceName);
                    if (performanceCounters != null)
                    {
                        foreach (var performanceCounter in performanceCounters)
                        {
                            var name = counter.Name;
                            if (name == "*")
                            {
                                name = performanceCounter.InstanceName;
                            }

                            performanceCounterInfos.Add(new PerformanceCounterInfo(name, performanceCounter));
                        }
                    }
                }

                var performanceCounterGroup = new PerformanceCounterGroup(group.Name, group.SecondsInterval, performanceCounterInfos);
                counterGroups.Add(performanceCounterGroup);
            }

            Thread.Sleep(100);

            return counterGroups;
        }

        private IEnumerable<PerformanceCounter> GetPerformanceCounters(string categoryName, string counterName, string instanceName)
        {
            var response = new List<PerformanceCounter>();
            try
            {
                if (instanceName == "*")
                {
                    var cat = new PerformanceCounterCategory(categoryName);
                    var instances = cat.GetInstanceNames();
                    response.AddRange(instances.Select(instance => cat.GetCounters(instance).Single(x => x.CounterName == counterName)));
                }
                else
                {
                    var processorCounter = new PerformanceCounter(categoryName, counterName, instanceName);
                    processorCounter.NextValue();
                    response.Add(processorCounter);
                }
            }
            catch (InvalidOperationException exception)
            {
                var message = exception.Message;
                message += " categoryName=" + categoryName + ", counterName=" + counterName + ", instanceName=" + (instanceName ?? "N/A");
                EventLog.WriteEntry(Constants.ServiceName, message, EventLogEntryType.Error);
            }

            return response;
        }
    }
}