using System;
using System.Collections.Generic;
using System.Threading;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class CounterBusiness : ICounterBusiness
    {
        private readonly PerformanceCounterProvider _perfCounterProvider;

        public event EventHandler<GetPerformanceCounterEventArgs> GetPerformanceCounterEvent;

        public CounterBusiness()
        {
            if (Thread.CurrentThread.CurrentCulture.Name != "en-US")
            {
                Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
            }

            this._perfCounterProvider = new PerformanceCounterProvider(this.OnGetPerformanceCounters, null);
        }

        public IEnumerable<IPerformanceCounterGroup> GetPerformanceCounterGroups(IConfig config)
        {
            if (config.Groups == null) throw new NullReferenceException("No groups in config.");

            foreach (var group in config.Groups)
            {
                yield return _perfCounterProvider.GetGroup(group);
            }
        }

        public IEnumerable<string> GetCategoryNames()
        {
            return _perfCounterProvider.GetCategoryNames();
        }

        public IEnumerable<string> GetCounterNames(string categoryName, string machineName)
        {
            return _perfCounterProvider.GetCounterNames(categoryName, machineName);
        }

        public IEnumerable<string> GetInstances(string category, string counterName, string machineName)
        {
            return _perfCounterProvider.GetInstances(category, counterName, machineName);
        }

        protected virtual void OnGetPerformanceCounters(string message)
        {
            var handler = GetPerformanceCounterEvent;
            if (handler != null)
            {
                handler(this, new GetPerformanceCounterEventArgs(message));
            }
        }
    }
}