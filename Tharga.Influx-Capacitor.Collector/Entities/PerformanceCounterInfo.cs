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
        private readonly string _fieldName;
        private readonly List<ITag> _tags;
        private readonly float? _max;
        private readonly float? _min;
        private readonly float? _reverse;
        private readonly string _counterAlias;
        private Naming _instanceName;

        public PerformanceCounterInfo(string name, PerformanceCounter performanceCounters, ICounter counter)
        {
            _name = name;
            _performanceCounters = performanceCounters;
            _fieldName = counter.FieldName;
            _tags = (counter.Tags ?? new List<ITag>()).ToList();
            _max = counter.Max;
            _min = counter.Min;
            _reverse = counter.Reverse;
            _counterAlias = counter.CounterName.Alias;
        }

        internal PerformanceCounterInfo(string name, PerformanceCounter performanceCounters, Naming instanceName, string fieldName, IEnumerable<ITag> tags, float? max, float? min, float? reverse, string counterAlias)
        {
            _name = name;
            _performanceCounters = performanceCounters;
            _instanceName = instanceName;
            _fieldName = fieldName;
            _counterAlias = counterAlias;

            _tags = (tags ?? new List<ITag>()).ToList();
            _max = max;
            _min = min;
            _reverse = reverse;
        }

        internal PerformanceCounterInfo(string name, PerformanceCounter performanceCounters)
            : this(name, performanceCounters, null, null, null, null, null, null, null)
        {
        }

        public PerformanceCounterInfo(PerformanceCounter performanceCounter, ICounter counter)
            : this(null, performanceCounter, counter)
        {
            _name = counter.Name;
            if (_name == "*" && performanceCounter != null)
            {
                _name = performanceCounter.InstanceName;
            }
        }

        public string Name { get { return _name; } }

        public Naming CounterName { get { return _performanceCounters == null ? null : new Naming(_performanceCounters.CounterName, _counterAlias); } }

        public string CategoryName { get { return _performanceCounters == null ? null : _performanceCounters.CategoryName; } }

        /// <summary>
        /// Gets the read instance name for this counter.
        /// </summary>
        public Naming InstanceName
        {
            get
            {
                if (_instanceName != null)
                    return _instanceName;


                if (_performanceCounters == null)
                    return null;

                return new Naming(_performanceCounters.InstanceName);
            }

            set { _instanceName = value; }
        }

        /// <summary>
        /// Gets the specific name to use as a field for this counter. <c>null</c> to use the default "value" field.
        /// </summary>
        public string FieldName { get { return _fieldName; } }

        public string MachineName { get { return _performanceCounters == null ? null : _performanceCounters.MachineName; } }

        public IEnumerable<ITag> Tags { get { return _tags; } }

        public float? Max { get { return _max; } }

        public float? Min { get { return _min; } }

        public float? Reverse { get { return _reverse; } }

        public bool HasPerformanceCounter { get { return _performanceCounters != null; } }

        public float NextValue()
        {
            // if performance counter exists but its raw value is null, calling NextValue() will throw an exception
            // in this case, just return zero
            try
            {
                return _performanceCounters.NextValue();
            }
            catch (System.Exception)
            {
                return 0;
            }
        }
    }
}