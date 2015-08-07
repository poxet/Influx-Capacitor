using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Agents
{
    [ExcludeFromCodeCoverage]
    public class FileLoaderAgent : IFileLoaderAgent
    {
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string path, string data)
        {
            if (DoesFileExist(path))
            {
                DeleteFile(path);
            }

            File.WriteAllText(path, data);
        }

        public bool DoesFileExist(string path)
        {
            return File.Exists(path);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public bool DoesDirectoryExist(string path)
        {
            return Directory.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public string[] GetFiles(string path, string searchPattern)
        {
            if (!Directory.Exists(path))
            {
                return new string[] { };
            }

            return Directory.GetFiles(path, searchPattern);
        }

        public string GetApplicationFolderPath()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Thargelion\\" + Constants.ServiceName;
            return path;
        }
    }
}