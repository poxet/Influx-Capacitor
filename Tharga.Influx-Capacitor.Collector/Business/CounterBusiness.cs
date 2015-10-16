using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class CounterBusiness : ICounterBusiness
    {
        public event EventHandler<GetPerformanceCounterEventArgs> GetPerformanceCounterEvent;

        public CounterBusiness()
        {
            if (Thread.CurrentThread.CurrentCulture.Name != "en-US")
            {
                Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
            }
        }

        public IEnumerable<IPerformanceCounterGroup> GetPerformanceCounterGroups(IConfig config)
        {
            if (config.Groups == null) throw new NullReferenceException("No groups in config.");

            foreach (var group in config.Groups)
            {
                yield return new PerformanceCounterGroup(group, GetPerformanceCounterInfos);
            }
        }

        private IEnumerable<IPerformanceCounterInfo> GetPerformanceCounterInfos(ICounterGroup group)
        {
            var performanceCounterInfos = new List<IPerformanceCounterInfo>();
            foreach (var counter in group.Counters)
            {
                var performanceCounters = GetPerformanceCounters(counter.CategoryName, counter.CounterName, counter.InstanceName).ToArray();
                if (performanceCounters.Any())
                {
                    foreach (var performanceCounter in performanceCounters)
                    {
                        var name = counter.Name;
                        if (name == "*")
                        {
                            name = performanceCounter.InstanceName;
                        }

                        performanceCounterInfos.Add(new PerformanceCounterInfo(name, performanceCounter, counter.Alias, counter.Tags));
                    }
                }
                else
                {
                    performanceCounterInfos.Add(new PerformanceCounterInfo(counter.Name, null, counter.Alias, counter.Tags));
                }
            }
            return performanceCounterInfos;
        }

        public IEnumerable<string> GetCategoryNames()
        {
            var performanceCounterCategories = PerformanceCounterCategory.GetCategories();
            return performanceCounterCategories.Select(x => x.CategoryName);
        }

        public IEnumerable<string> GetCounterNames(string category)
        {
            var cat = new PerformanceCounterCategory(category);
            var instances = cat.CategoryType == PerformanceCounterCategoryType.SingleInstance ? new string[] { null } : cat.GetInstanceNames();
            foreach (var instance in instances)
            {
                var coutners = string.IsNullOrEmpty(instance) ? cat.GetCounters().Select(x => x.CounterName) : cat.GetCounters(instance).Select(x => x.CounterName);
                foreach (var counter in coutners)
                {
                    yield return counter;
                }
            }
        }

        public IEnumerable<string> GetInstances(string category, string counterName)
        {
            var cat = new PerformanceCounterCategory(category);
            return cat.GetInstanceNames();
        }

        private bool Match(string data, string pattern)
        {
            if (pattern == "*")
            {
                return true;
            }

            var reg = new Regex("^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$");
            return reg.IsMatch(data);
        }

        private IEnumerable<PerformanceCounter> GetPerformanceCounters(string categoryName, string counterName, string instanceName)
        {
            var response = new List<PerformanceCounter>();
            try
            {
                if ((counterName.Contains("*") || (instanceName != null && instanceName.Contains("*"))))
                {
                    var cat = new PerformanceCounterCategory(categoryName);

                    string[] instances;
                    if (instanceName.Contains("*"))
                    {
                        instances = cat.GetInstanceNames().Where(x => Match(x, instanceName)).ToArray();
                        //TODO: If this response gives no instances, this means that this counter should use null, for instance
                        if (!instances.Any())
                        {
                            instances = new[] { (string)null };
                        }
                    }
                    else
                    {
                        instances = new[] { instanceName };
                    }

                    var counterNames = new[] { counterName };
                    if (counterName.Contains("*"))
                    {
                        foreach (var instance in instances)
                        {
                            if (string.IsNullOrEmpty(instance))
                                counterNames = cat.GetCounters().Where(x => Match(x.CounterName, counterName)).Select(x => x.CounterName).ToArray();
                            else
                                counterNames = cat.GetCounters(instance).Where(x => Match(x.CounterName, counterName)).Select(x => x.CounterName).ToArray();
                        }
                    }

                    foreach(var counter in counterNames)
                    {
                        foreach (var instance in instances)
                        {
                            var processorCounter = new PerformanceCounter(categoryName, counter, instance);
                            processorCounter.NextValue();
                            response.Add(processorCounter);
                        }
                    }
                }
                else
                {
                    var processorCounter = new PerformanceCounter(categoryName, counterName, instanceName);
                    processorCounter.NextValue();
                    response.Add(processorCounter);
                }
            }
            catch (Exception exception)
            {
                OnGetPerformanceCounters(exception, categoryName, counterName, instanceName);
            }

            return response;
        }

        protected virtual void OnGetPerformanceCounters(Exception exception, string categoryName, string counterName, string instanceName)
        {
            var handler = GetPerformanceCounterEvent;
            if (handler != null)
            {
                handler(this, new GetPerformanceCounterEventArgs(exception, categoryName, counterName, instanceName));
            }
        }
    }
}