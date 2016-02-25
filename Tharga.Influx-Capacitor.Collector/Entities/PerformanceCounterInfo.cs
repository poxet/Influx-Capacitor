using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class PerformanceCounterInfo : IPerformanceCounterInfo
    {
        private readonly string _name;
        private readonly PerformanceCounter _performanceCounters;
        private readonly string _alias;
        private readonly string _instanceName;
        private readonly string _fieldName;
        private readonly List<ITag> _tags;
        private readonly float? _max;

        public PerformanceCounterInfo(string name, PerformanceCounter performanceCounters, ICounter counter)
        {
            _name = name;
            _performanceCounters = performanceCounters;
            _fieldName = counter.FieldName;
            _alias = counter.Alias;
            _tags = (counter.Tags ?? new List<ITag>()).ToList();
            _max = counter.Max;
        }

        public PerformanceCounterInfo(string name, PerformanceCounter performanceCounters, string instanceName, string fieldName, string alias, IEnumerable<ITag> tags, float? max)
        {
            _name = name;
            _performanceCounters = performanceCounters;
            _instanceName = instanceName;
            _fieldName = fieldName;

            _alias = alias;
            _tags = (tags ?? new List<ITag>()).ToList();
            _max = max;
        }

        internal PerformanceCounterInfo(string name, PerformanceCounter performanceCounters)
            : this(name, performanceCounters, null, null, null, null, null)
        {
        }

        public string Name { get { return _name; } }

        public string CounterName { get { return _performanceCounters == null ? null : _performanceCounters.CounterName; } }

        public string CategoryName { get { return _performanceCounters == null ? null : _performanceCounters.CategoryName; } }

        /// <summary>
        /// Gets the read instance name for this counter.
        /// </summary>
        public string InstanceName
        {
            get
            {
                if (_instanceName != null)
                    return _instanceName;


                if (_performanceCounters == null)
                    return null;

                return _performanceCounters.InstanceName;
            }
        }

        /// <summary>
        /// Gets the specific name to use as a field for this counter. <c>null</c> to use the default "value" field.
        /// </summary>
        public string FieldName { get { return _fieldName; } }

        public string MachineName { get { return _performanceCounters == null ? null : _performanceCounters.MachineName; } }

        public string Alias { get { return _alias; } }

        public IEnumerable<ITag> Tags { get { return _tags; } }

        public float? Max { get { return _max; } }

        public bool HasPerformanceCounter { get { return _performanceCounters != null; } }


        public float NextValue()
        {
            return _performanceCounters.NextValue();
        }
    }
}