using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector.Business
{
    public class FileLoader : IFileLoader
    {
        public string ReadAllText(string path)
        {
            return System.IO.File.ReadAllText(path);
        }
    }
}