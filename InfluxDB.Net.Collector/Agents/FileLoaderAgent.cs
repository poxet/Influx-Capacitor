using System.Diagnostics.CodeAnalysis;
using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector.Agents
{
    [ExcludeFromCodeCoverage]
    public class FileLoaderAgent : IFileLoaderAgent
    {
        public string ReadAllText(string path)
        {
            return System.IO.File.ReadAllText(path);
        }
    }
}