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
                // retrieve all system performance counters, then
                var performanceCounters = GetPerformanceCounters(counter.CategoryName, counter.CounterName, counter.InstanceName, counter.MachineName)
                    .Select(p => new PerformanceCounterRegistration  { PerformanceCounter = p, FilteredInstanceName = p.InstanceName})
                    .ToList();

                if (performanceCounters.Count != 0)
                {
                    // 1) apply filtering
                    if (group.Filters != null)
                    {
                        for (var i = performanceCounters.Count-1; i >= 0; i--)
                        {
                            var x = performanceCounters[i];
                            var filteredInstanceName = group.Filters.Aggregate(x.FilteredInstanceName, (current, filter) => filter.Execute(current));

                            if (filteredInstanceName == null)
                            {
                                performanceCounters.RemoveAt(i);
                            }
                            else
                            {
                                // we save the instance name with filters applied
                                performanceCounters[i].FilteredInstanceName = filteredInstanceName;
                            }
                        }
                    }

                    // 2) retrieve values
                    foreach (var performanceCounter in performanceCounters)
                    {
                        try
                        {
                            performanceCounter.PerformanceCounter.NextValue();
                        }
                        catch (Exception ex)
                        {
                            OnGetPerformanceCounters(ex, counter.CategoryName, counter.CounterName, counter.InstanceName, counter.MachineName);
                        }
                    }

                    // 3) create infos
                    foreach (var performanceCounter in performanceCounters)
                    {
                        var name = counter.Name;
                        if (name == "*")
                        {
                            name = performanceCounter.PerformanceCounter.InstanceName;
                        }

                        performanceCounterInfos.Add(new PerformanceCounterInfo(name, performanceCounter.PerformanceCounter, performanceCounter.FilteredInstanceName, counter.FieldName, counter.Alias, counter.Tags, null));
                    }
                }
                else
                {
                    performanceCounterInfos.Add(new PerformanceCounterInfo(counter.Name, null, counter));
                }
            }

            return performanceCounterInfos;
        }

        public IEnumerable<string> GetCategoryNames()
        {
            var performanceCounterCategories = PerformanceCounterCategory.GetCategories();
            return performanceCounterCategories.Select(x => x.CategoryName);
        }

        public IEnumerable<string> GetCounterNames(string categoryName, string machineName)
        {
            var cat = PerformanceCounterHelper.GetPerformanceCounterCategory(categoryName, machineName);

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

        public IEnumerable<string> GetInstances(string category, string counterName, string machineName)
        {
            var cat = PerformanceCounterHelper.GetPerformanceCounterCategory(category, machineName);
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


        private IEnumerable<PerformanceCounter> GetPerformanceCounters(string categoryName, string counterName, string instanceName, string machineName)
        {
            var response = new List<PerformanceCounter>();
            try
            {
                if ((counterName.Contains("*") || (instanceName != null && (instanceName.Contains("*") || instanceName.Contains("|")))))
                {
                    var cat = PerformanceCounterHelper.GetPerformanceCounterCategory(categoryName, machineName);

                    List<string> instances = new List<string>();

                    if (instanceName == null)
                    {
                        instances.Add(null);
                    }
                    else if (instanceName.Contains("|"))
                    {
                        foreach (String instancePart in instanceName.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (instancePart.Contains("*"))
                            {
                                instances.AddRange(cat.GetInstanceNames().Where(x => Match(x, instancePart)));
                            }
                            else
                            {
                                instances.Add(instancePart);
                            }
                        }

                        if (instances.Count == 0)
                        {
                            instances.Add(null);
                        }
                    }
                    else if (instanceName.Contains("*"))
                    {
                        instances.AddRange(cat.GetInstanceNames().Where(x => Match(x, instanceName)));
                        //TODO: If this response gives no instances, this means that this counter should use null, for instance
                        if (instances.Count == 0)
                        {
                            instances.Add(null);
                        }
                    }
                    else
                    {
                        instances.Add(instanceName);
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
                            var processorCounter = new PerformanceCounter(categoryName, counter, instance, machineName);
                            //processorCounter.NextValue();
                            response.Add(processorCounter);
                        }
                    }
                }
                else
                {
                    var processorCounter = new PerformanceCounter(categoryName, counterName, instanceName, machineName);
                    //processorCounter.NextValue();
                    response.Add(processorCounter);
                }
            }
            catch (Exception exception)
            {
                OnGetPerformanceCounters(exception, categoryName, counterName, instanceName, machineName);
            }

            return response;
        }

        protected virtual void OnGetPerformanceCounters(Exception exception, string categoryName, string counterName, string instanceName, string machineName)
        {
            var handler = GetPerformanceCounterEvent;
            if (handler != null)
            {
                handler(this, new GetPerformanceCounterEventArgs(exception, categoryName, counterName, instanceName, machineName));
            }
        }

        /// <summary>
        /// Contains informations required to register a performance counter as a new IPerformanceCounterInfo
        /// </summary>
        private class PerformanceCounterRegistration
        {
            /// <summary>
            /// Gets or sets the system performance counter.
            /// </summary>
            public PerformanceCounter PerformanceCounter { get; set; }

            /// <summary>
            /// Gets or sets the name of the instance, after the filters have been applied.
            /// </summary>
            public string FilteredInstanceName { get; set; }
        }
    }
}