namespace InfluxDB.Net.Collector.Interface
{
    public interface IConfigBusiness
    {
        IConfig LoadFile(string configurationFilename);
        IConfig LoadFiles(string[] configurationFilenames);
    }
}