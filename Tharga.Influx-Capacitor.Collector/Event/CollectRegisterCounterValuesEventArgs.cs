using System;
using System.Collections.Generic;

namespace Tharga.InfluxCapacitor.Collector.Event
{
    public class CollectRegisterCounterValuesEventArgs : EventArgs
    {
        private readonly string _engineName;
        private readonly string _message;
        private readonly bool _success;

        public CollectRegisterCounterValuesEventArgs(string engineName, Exception exception)
        {
            _engineName = engineName;
            _message = exception.Message;
            _success = false;
        }

        public CollectRegisterCounterValuesEventArgs(string engineName, string message)
        {
            _engineName = engineName;
            _message = message;
            _success = true;
        }

        public CollectRegisterCounterValuesEventArgs(string engineName, int counters, Dictionary<string, long> timeInfo, double elapseOffset)
        {
            _engineName = engineName;
            _message = string.Format("Read {0} metrics in {1}ms. ElapseOffset: {2}ms", counters, new TimeSpan(timeInfo["Read"]).TotalMilliseconds.ToString("0.####"), elapseOffset.ToString("0.####"));
            _success = true;
        }

        public string EngineName
        {
            get
            {
                return _engineName;
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
        }

        public bool Success
        {
            get
            {
                return _success;
            }
        }
    }
}