namespace InfluxDB.Net.Collector.Interface
{
    public interface IConfigBusiness
    {
        IConfig LoadFile(string configurationFilename);
        IConfig LoadFiles(string[] configurationFilenames);
        IDatabaseConfig GetDatabaseFromRegistry();
        void SetUrl(string url);
        void SetDatabase(string databaseName, string username, string password);
    }
}