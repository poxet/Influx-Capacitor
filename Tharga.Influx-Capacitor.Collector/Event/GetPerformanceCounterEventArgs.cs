using System;

namespace Tharga.InfluxCapacitor.Collector.Event
{
    public class GetPerformanceCounterEventArgs : EventArgs
    {
        private readonly Exception _exception;
        private readonly string _categoryName;
        private readonly string _counterName;
        private readonly string _instanceName;

        public GetPerformanceCounterEventArgs(Exception exception, string categoryName, string counterName, string instanceName)
        {
            _exception = exception;
            _categoryName = categoryName;
            _counterName = counterName;
            _instanceName = instanceName;
        }

        public string Message
        {
            get
            {
                var instance = _instanceName == null ? string.Empty : "." + _instanceName;
                return string.Format("Unable to get performance counter {0}.{1}{2}. {3}", _categoryName, _counterName, instance, _exception.Message);
            }
        }
    }
}