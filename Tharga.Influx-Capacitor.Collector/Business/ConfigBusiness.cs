using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using InfluxDB.Net;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class ConfigBusiness : IConfigBusiness
    {
        private readonly IFileLoaderAgent _fileLoaderAgent;

        public ConfigBusiness(IFileLoaderAgent fileLoaderAgent)
        {
            _fileLoaderAgent = fileLoaderAgent;
        }

        public IConfig LoadFile(string configurationFilename)
        {
            return LoadFiles(new[] { configurationFilename });
        }

        public IConfig LoadFiles(string[] configurationFilenames)
        {
            if (!configurationFilenames.Any())
            {
                configurationFilenames = GetConfigFiles().ToArray();
            }

            IDatabaseConfig database = null;
            var groups = new List<ICounterGroup>();

            foreach (var configurationFilename in configurationFilenames)
            {
                var fileData = _fileLoaderAgent.ReadAllText(configurationFilename);

                var document = new XmlDocument();
                document.LoadXml(fileData);

                var db = GetDatabaseConfig(document);
                var grp = GetCounterGroups(document).ToList();

                if (db != null)
                {
                    if (database != null)
                    {
                        throw new InvalidOperationException("There are database configuration sections in more than one config file.");
                    }

                    database = db;
                }
                groups.AddRange(grp);
            }

            var config = new Config(database, groups);
            return config;
        }

        private string GetAppDataFolder()
        {
            var path = _fileLoaderAgent.GetApplicationFolderPath();
            if (!_fileLoaderAgent.DoesDirectoryExist(path))
            {
                _fileLoaderAgent.CreateDirectory(path);

                if (!_fileLoaderAgent.DoesDirectoryExist(path))
                    throw new InvalidOperationException(string.Format("Unable to create directory {0}.", path));

                TestWriteAndDeleteAccess(path);
            }

            return path;
        }

        private void TestWriteAndDeleteAccess(string path)
        {
            var sampleFileName = path + "\\test.txt";
            _fileLoaderAgent.WriteAllText(sampleFileName, "ABC");

            if (!_fileLoaderAgent.DoesFileExist(sampleFileName))
                throw new InvalidOperationException(string.Format("Unable to create testfile {0} in application folder.", sampleFileName));

            _fileLoaderAgent.DeleteFile(sampleFileName);

            if (_fileLoaderAgent.DoesFileExist(sampleFileName))
                throw new InvalidOperationException(string.Format("Unable to delete testfile {0} in application folder.", sampleFileName));
        }

        public IDatabaseConfig OpenDatabaseConfig()
        {
            var path = GetAppDataFolder();
            var databaseConfigFilePath = path + "\\database.xml";
            if (!_fileLoaderAgent.DoesFileExist(databaseConfigFilePath))
            {
                return new DatabaseConfig(Constants.NoConfigUrl, null, null, null, InfluxDbVersion.Auto);
            }

            var config = LoadFile(databaseConfigFilePath);
            return config.Database;
        }

        public void SaveDatabaseUrl(string url, InfluxDbVersion influxDbVersion)
        {
            var config = OpenDatabaseConfig();
            var newDbConfig = new DatabaseConfig(url, config.Username, config.Password, config.Name, influxDbVersion);
            SaveDatabaseConfigEx(newDbConfig);
        }

        public void SaveDatabaseConfig(string databaseName, string username, string password)
        {
            var config = OpenDatabaseConfig();
            var newDbConfig = new DatabaseConfig(config.Url, username, password, databaseName, config.InfluxDbVersion);
            SaveDatabaseConfigEx(newDbConfig);
        }

        private void SaveDatabaseConfigEx(DatabaseConfig newDbConfig)
        {
            var path = GetAppDataFolder();
            var databaseConfigFilePath = path + "\\database.xml";

            var xml = new XmlDocument();
            var xme = xml.CreateElement(Constants.ServiceName);
            xml.AppendChild(xme);
            var dme = xml.CreateElement("Database");
            xme.AppendChild(dme);

            var xmeUrl = xml.CreateElement("Url");
            xmeUrl.InnerText = newDbConfig.Url;
            dme.AppendChild(xmeUrl);

            var xmeUsername = xml.CreateElement("Username");
            xmeUsername.InnerText = newDbConfig.Username;
            dme.AppendChild(xmeUsername);

            var xmePassword = xml.CreateElement("Password");
            xmePassword.InnerText = newDbConfig.Password;
            dme.AppendChild(xmePassword);

            var xmeName = xml.CreateElement("Name");
            xmeName.InnerText = newDbConfig.Name;
            dme.AppendChild(xmeName);

            var xmlData = xml.OuterXml;

            _fileLoaderAgent.WriteAllText(databaseConfigFilePath, xmlData);
        }

        private IEnumerable<ICounterGroup> GetCounterGroups(XmlDocument document)
        {
            var counterGroups = document.GetElementsByTagName("CounterGroup");
            foreach (XmlElement counterGroup in counterGroups)
            {
                yield return GetCounterGroup(counterGroup);
            }
        }

        private ICounterGroup GetCounterGroup(XmlElement counterGroup)
        {
            var name = GetString(counterGroup, "Name");
            var secondsInterval = GetInt(counterGroup, "SecondsInterval");

            var counters = counterGroup.GetElementsByTagName("Counter");
            var cts = new List<ICounter>();
            foreach (XmlElement counter in counters)
            {
                cts.Add(GetCounter(counter));
            }
            return new CounterGroup(name, secondsInterval, cts);
        }

        private static string GetString(XmlElement element, string name)
        {
            var attr = element.Attributes.GetNamedItem(name);
            if (attr == null || string.IsNullOrEmpty(attr.Value))
                throw new InvalidOperationException(string.Format("No {0} attribute specified for the CounterGroup.", name));
            return attr.Value;
        }

        private static int GetInt(XmlElement element, string name)
        {
            var stringValue = GetString(element, name);
            int value;
            if (!int.TryParse(stringValue, out value))
                throw new InvalidOperationException(string.Format("Cannot parse attribute {0} value to integer.", name));
            return value;
        }

        private ICounter GetCounter(XmlElement counter)
        {
            string categoryName = null;
            string counterName = null;
            string instanceName = null;
            foreach (XmlElement item in counter.ChildNodes)
            {
                switch (item.Name)
                {
                    case "CategoryName":
                        categoryName = item.InnerText;
                        break;
                    case "CounterName":
                        counterName = item.InnerText;
                        break;
                    case "InstanceName":
                        instanceName = item.InnerText;
                        break;
                }
            }
            var namedItem = counter.Attributes.GetNamedItem("Name");
            var name = namedItem != null ? namedItem.Value : null;
            return new Counter(name, categoryName, counterName, instanceName);
        }

        private static DatabaseConfig GetDatabaseConfig(XmlDocument document)
        {
            var databases = document.GetElementsByTagName("Database");
            if (databases.Count == 0)
                return null;

            string url = null;
            string username = null;
            string password = null;
            string name = null;
            var influxDbVersion = InfluxDbVersion.Auto;
            foreach (XmlElement item in databases[0].ChildNodes)
            {
                switch (item.Name)
                {
                    case "Url":
                        url = item.InnerText;
                        break;
                    case "Username":
                        username = item.InnerText;
                        break;
                    case "Password":
                        password = item.InnerText;
                        break;
                    case "Name":
                        name = item.InnerText;
                        break;
                    case "InfluxDbVersion":
                        if (!Enum.TryParse(item.InnerText, true, out influxDbVersion))
                        {
                            influxDbVersion = InfluxDbVersion.Auto;
                        }
                        break;
                    case "":
                        break;
                }
            }

            var database = new DatabaseConfig(url, username, password, name, influxDbVersion);
            return database;
        }

        public IEnumerable<string> GetConfigFiles()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configFilesInCurrentDirectory = _fileLoaderAgent.GetFiles(currentDirectory, "*.xml");

            var configFilesInProgramData = _fileLoaderAgent.GetFiles(_fileLoaderAgent.GetApplicationFolderPath(), "*.xml");

            foreach (var configFile in configFilesInCurrentDirectory.Union(configFilesInProgramData))
            {
                var fileData = _fileLoaderAgent.ReadAllText(configFile);

                var document = new XmlDocument();
                document.LoadXml(fileData);

                var db = GetDatabaseConfig(document);
                var grp = GetCounterGroups(document).ToList();

                if (db != null || grp.Any())
                {
                    yield return configFile;
                }
            }
        }
    }
}