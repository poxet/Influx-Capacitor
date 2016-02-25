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
                    .Select(p => new PerformanceCounterInfo(p, counter))
                    .ToList();

                if (performanceCounters.Count != 0)
                {
                    // 1) apply filtering
                    if (group.Filters != null)
                    {
                        var filteredNames = new Dictionary<string, int>(StringComparer.Ordinal);
                        for (var i = performanceCounters.Count-1; i >= 0; i--)
                        {
                            var x = performanceCounters[i];
                            var filteredInstanceName = group.Filters.Aggregate(x.InstanceName, (current, filter) => filter.Execute(current));

                            if (filteredInstanceName == null)
                            {
                                performanceCounters.RemoveAt(i);
                            }
                            else
                            {
                                // we handle uniqueness of instance names :
                                // because of the filtering, multiples instances can have the same instance name,
                                // so we increment a counter for each instance after the first one
                                // eg: w3wp, w3wp#2, w3wp#3, etc.
                                int count;
                                if (filteredNames.TryGetValue(filteredInstanceName, out count))
                                {
                                    filteredInstanceName += "#" + (count + 1);
                                }

                                filteredNames[filteredInstanceName] = count + 1;
                                performanceCounters[i].InstanceName = filteredInstanceName;
                            }
                        }
                    }

                    // 2) retrieve values
                    foreach (var performanceCounter in performanceCounters)
                    {
                        try
                        {
                            performanceCounter.NextValue();
                        }
                        catch (Exception ex)
                        {
                            OnGetPerformanceCounters(ex, counter.CategoryName, counter.CounterName, counter.InstanceName, counter.MachineName);
                        }
                    }

                    // 3) create infos
                    foreach (var performanceCounter in performanceCounters)
                    {
                        performanceCounterInfos.Add(performanceCounter);
                    }
                }
                else
                {
                    performanceCounterInfos.Add(new PerformanceCounterInfo(null, counter));
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
                            if (!string.IsNullOrEmpty(machineName))
                            {
                                response.Add(new PerformanceCounter(categoryName, counter, instance, machineName));
                            }
                            else
                            {
                                response.Add(new PerformanceCounter(categoryName, counter, instance));
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(machineName))
                    {
                        response.Add(new PerformanceCounter(categoryName, counterName, instanceName, machineName));
                    }
                    else
                    {
                        response.Add(new PerformanceCounter(categoryName, counterName, instanceName));
                    }
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
    }
}