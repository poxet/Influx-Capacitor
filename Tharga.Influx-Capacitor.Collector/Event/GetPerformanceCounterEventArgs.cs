using System;

namespace Tharga.InfluxCapacitor.Collector.Event
{
    public class GetPerformanceCounterEventArgs : EventArgs
    {
        private readonly Exception _exception;
        private readonly string _categoryName;
        private readonly string _counterName;
        private readonly string _instanceName;
        private readonly string _machineName;
        private readonly string _message;

        public GetPerformanceCounterEventArgs(Exception exception, string categoryName, string counterName, string instanceName, string machineName)
        {
            _exception = exception;
            _categoryName = categoryName;
            _counterName = counterName;
            _instanceName = instanceName;
            _machineName = machineName;
        }

        public GetPerformanceCounterEventArgs(string message)
        {
            _message = message;
        }

        public string Message
        {
            get
            {
                if (_message != null)
                    return _message;

                var instance = _instanceName == null ? string.Empty : "." + _instanceName;
                return string.Format("Unable to get performance counter {0}.{1}.{2}{3}. {4}", _machineName, _categoryName, _counterName, instance, _exception.Message);
            }
        }
    }
}