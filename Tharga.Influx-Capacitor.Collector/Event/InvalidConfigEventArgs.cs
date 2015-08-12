using System;

namespace Tharga.InfluxCapacitor.Collector.Event
{
    public class InvalidConfigEventArgs : EventArgs
    {
        private readonly Exception _exception;
        private readonly string _groupName;
        private readonly string _configFileName;

        public InvalidConfigEventArgs(Exception exception, string configFileName)
        {
            _exception = exception;
            _configFileName = configFileName;
        }

        public InvalidConfigEventArgs(string groupName, string configFileName)
        {
            _groupName = groupName;
            _configFileName = configFileName;
        }

        public string Message
        {
            get
            {
                if (_exception != null)
                {
                    return string.Format("Ignoring invalid config file '{0}'. {1}", _configFileName, _exception.Message);
                }

                return string.Format("Ignoring counter group '{0}' in file '{1}' because there is already a counter group with that name.", _groupName, _configFileName);
            }
        }
    }
}