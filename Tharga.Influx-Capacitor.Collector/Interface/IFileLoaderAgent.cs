namespace InfluxDB.Net.Collector.Interface
{
    public interface IFileLoaderAgent
    {
        string ReadAllText(string path);
        void WriteAllText(string path, string data);
        bool DoesFileExist(string path);
        void DeleteFile(string path);
        bool DoesDirectoryExist(string path);
        void CreateDirectory(string path);
        string[] GetFiles(string path, string searchPattern);
        string GetApplicationFolderPath();
    }
}