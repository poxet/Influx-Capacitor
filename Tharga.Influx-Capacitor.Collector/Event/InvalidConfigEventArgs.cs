using System;

namespace Tharga.InfluxCapacitor.Collector.Event
{
    public class InvalidConfigEventArgs : EventArgs
    {
        private readonly Exception _exception;
        private readonly string _groupName;
        private readonly string _tagName;
        private readonly string _configFileName;

        public InvalidConfigEventArgs(Exception exception, string configFileName)
        {
            _exception = exception;
            _configFileName = configFileName;
        }

        public InvalidConfigEventArgs(string groupName, string tagName, string configFileName)
        {
            _groupName = groupName;
            _tagName = tagName;
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

                if (!string.IsNullOrEmpty(_groupName))
                    return string.Format("Ignoring counter group '{0}' in file '{1}' because there is already a counter group with that name.", _groupName, _configFileName);

                if (!string.IsNullOrEmpty(_tagName))
                    return string.Format("Ignoring tag '{0}' in file '{1}' because there is already a tag with that name.", _tagName, _configFileName);

                return string.Format("Ignoring invalid config file '{0}' with unknown issue.", _configFileName);
            }
        }
    }
}