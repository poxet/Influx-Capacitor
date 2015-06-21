using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using InfluxDB.Net.Collector.Entities;
using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector.Business
{
    public class ConfigBusiness
    {
        private readonly IFileLoader _fileLoader;

        public ConfigBusiness(IFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public Config LoadFile(string configurationFilename)
        {
            return LoadFiles(new[] { configurationFilename });
        }

        public Config LoadFiles(string[] configurationFilenames)
        {
            if ( !configurationFilenames.Any())
                throw new InvalidOperationException("No configuration files provided.");

            DatabaseConfig database = null;
            var groups = new List<CounterGroup>();

            foreach (var configurationFilename in configurationFilenames)
            {
                var fileData = _fileLoader.ReadAllText(configurationFilename);

                var document = new XmlDocument();
                document.LoadXml(fileData);

                var db = GetDatabaseConfig(document);
                var grp = GetCounterGroups(document).ToList();

                if (db != null)
                {
                    if (database != null)
                    {
                        throw new InvalidOperationException("There are database configuration sections in more than one file.");
                    }
                    database = db;
                }
                groups.AddRange(grp);
            }

            if ( database == null)
                throw new InvalidOperationException("No file contains configuration information for the database.");

            var config = new Config(database, groups);
            return config;
        }

        private IEnumerable<CounterGroup> GetCounterGroups(XmlDocument document)
        {
            var counterGroups = document.GetElementsByTagName("CounterGroup");
            foreach (XmlElement counterGroup in counterGroups)
            {
                yield return GetCounterGroup(counterGroup);
            }
        }

        private CounterGroup GetCounterGroup(XmlElement counterGroup)
        {
            var name = GetString(counterGroup, "Name");
            var secondsInterval = GetInt(counterGroup, "SecondsInterval");

            var counters = counterGroup.GetElementsByTagName("Counter");
            var cts = new List<Counter>();
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

        private Counter GetCounter(XmlElement counter)
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
            return new Counter(categoryName, counterName, instanceName);
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
                }
            }
            var database = new DatabaseConfig(url, username, password, name);
            return database;
        }
    }
}
