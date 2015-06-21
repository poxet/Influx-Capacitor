namespace InfluxDB.Net.Collector.Interface
{
    public interface IFileLoaderAgent
    {
        string ReadAllText(string path);
    }
}