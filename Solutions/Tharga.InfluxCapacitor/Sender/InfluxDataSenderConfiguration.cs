using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Sender
{
    public class InfluxDataSenderConfiguration : ISenderConfiguration
    {
        public InfluxDataSenderConfiguration(bool isEnabled, int maxQueueSize, string url, string databaseName, string userName, string password)
        {
            IsEnabled = isEnabled;
            MaxQueueSize = maxQueueSize;
            Properties.Url = url;
            Properties.DatabaseName = databaseName;
            Properties.UserName = userName;
            Properties.Password = password;
        }

        public bool IsEnabled { get; }
        public string Type => "InfluxDB";
        public int MaxQueueSize { get; }
        public dynamic Properties { get; }
    }
}