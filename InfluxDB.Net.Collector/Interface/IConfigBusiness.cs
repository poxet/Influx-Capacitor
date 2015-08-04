namespace InfluxDB.Net.Collector.Interface
{
    public interface IConfigBusiness
    {
        IConfig LoadFile(string configurationFilename);
        IConfig LoadFiles(string[] configurationFilenames);
        IDatabaseConfig OpenDatabaseConfig();
        void SaveDatabaseUrl(string url, InfluxDbVersion influxDbVersion);
        void SaveDatabaseConfig(string databaseName, string username, string password);
    }
}