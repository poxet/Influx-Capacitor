namespace InfluxDB.Net.Collector.Interface
{
    public interface IFileLoader
    {
        string ReadAllText(string path);
    }
}