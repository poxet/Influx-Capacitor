using System;
using Tharga.Influx_Capacitor.Interface;

namespace Tharga.Influx_Capacitor.Sender
{
    public class InfluxDataSenderConfiguration : ISenderConfiguration
    {
        public InfluxDataSenderConfiguration(bool isEnabled, int maxQueueSize, string url, string databaseName, string userName, string password, TimeSpan? requestTimeout)
        {
            IsEnabled = isEnabled;
            MaxQueueSize = maxQueueSize;
            Properties = new System.Dynamic.ExpandoObject();
            Properties.Url = url;
            Properties.DatabaseName = databaseName;
            Properties.UserName = userName;
            Properties.Password = password;
            Properties.RequestTimeout = requestTimeout;
        }

        public bool IsEnabled { get; private set; }
        public string Type { get { return "InfluxDB"; } }
        public int MaxQueueSize { get; private set; }
        public dynamic Properties { get; private set; }
    }
}