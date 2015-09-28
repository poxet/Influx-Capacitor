using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class PerformanceCounterGroup : IPerformanceCounterGroup
    {
        private readonly object _syncRoot = new object();
        private readonly ICounterGroup _counterGroup;
        private readonly Func<ICounterGroup, IEnumerable<IPerformanceCounterInfo>> _counterLoader;
        private List<IPerformanceCounterInfo> _performanceCounterInfos;

        public PerformanceCounterGroup(ICounterGroup counterGroup, Func<ICounterGroup, IEnumerable<IPerformanceCounterInfo>> counterLoader)
        {
            _counterGroup = counterGroup;
            _counterLoader = counterLoader;
        }

        public string Name
        {
            get
            {
                return _counterGroup.Name;
            }
        }

        public int SecondsInterval
        {
            get
            {
                return _counterGroup.SecondsInterval;
            }
        }

        public int RefreshInstanceInterval
        {
            get
            {
                return _counterGroup.RefreshInstanceInterval;
            }
        }

        public IEnumerable<ITag> Tags
        {
            get
            {
                return _counterGroup.Tags;
            }
        }

        public CollectorEngineType CollectorEngineType
        {
            get
            {
                return _counterGroup.CollectorEngineType;
            }
        }

        public IEnumerable<IPerformanceCounterInfo> GetCounters()
        {
            if (_performanceCounterInfos == null)
            {
                lock (_syncRoot)
                {
                    if (_performanceCounterInfos == null)
                    {
                        _performanceCounterInfos = _counterLoader(_counterGroup).ToList();
                    }
                }
            }

            return _performanceCounterInfos;
        }

        public IEnumerable<IPerformanceCounterInfo> GetFreshCounters()
        {
            lock (_syncRoot)
            {
                var newPerformanceCounterInfos = _counterLoader(_counterGroup).ToList();
                if (_performanceCounterInfos == null)
                {
                    _performanceCounterInfos = newPerformanceCounterInfos;
                    Trace.TraceInformation("Loaded {0} counters.", _performanceCounterInfos.Count);
                }
                else
                {
                    //Get a new list of counters. Add new ones and remove obsolete ones. (Do not refresh the entire list since that messes up the metrics)
                    var cnt = _performanceCounterInfos.Count;

                    _performanceCounterInfos.RemoveAll(x => x.PerformanceCounter == null);
                    _performanceCounterInfos.RemoveAll(x => !newPerformanceCounterInfos.Any(y => y.Name == x.Name && y.PerformanceCounter.CategoryName == x.PerformanceCounter.CategoryName && y.PerformanceCounter.CounterName == x.PerformanceCounter.CounterName && y.PerformanceCounter.InstanceName == x.PerformanceCounter.InstanceName));
                    var removed = cnt - _performanceCounterInfos.Count;
                    cnt = _performanceCounterInfos.Count;

                    var newCounters = newPerformanceCounterInfos.Where(x => !_performanceCounterInfos.Any(y => y.Name == x.Name && y.PerformanceCounter.CategoryName == x.PerformanceCounter.CategoryName && y.PerformanceCounter.CounterName == x.PerformanceCounter.CounterName && y.PerformanceCounter.InstanceName == x.PerformanceCounter.InstanceName));
                    _performanceCounterInfos = _performanceCounterInfos.Union(newCounters).ToList();
                    var added = _performanceCounterInfos.Count - cnt;

                    Trace.TraceInformation("Updated {0} counters, adding {1} and removing {2}.", _performanceCounterInfos.Count, added, removed);
                }
                return _performanceCounterInfos;
            }
        }

        public void RemoveCounter(IPerformanceCounterInfo performanceCounterInfo)
        {
            _performanceCounterInfos.Remove(performanceCounterInfo);
        }
    }
}