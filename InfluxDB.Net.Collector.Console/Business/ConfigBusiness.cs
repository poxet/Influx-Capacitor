using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;
using InfluxDB.Net.Collector.Console.Entities;

namespace InfluxDB.Net.Collector.Console.Business
{
    internal class ConfigBusiness
    {
        public Config LoadFile(string configurationFilename)
        {
            return LoadFiles(new[] { configurationFilename });
        }

        public Config LoadFiles(string[] configurationFilenames)
        {
            DatabaseConfig database = null;
            var groups = new List<CounterGroup>();

            foreach (var configurationFilename in configurationFilenames)
            {
                var document = new XmlDocument();
                document.Load(configurationFilename);

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
            var value = element.Attributes.GetNamedItem(name).Value;
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException(string.Format("No {0} attribute specified for the CounterGroup.", name));
            return value;
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
                        if (string.IsNullOrEmpty(url))
                            throw new ConfigurationErrorsException("There is no url provided in the configuration file.");
                        break;
                    case "Username":
                        username = item.InnerText;
                        if (string.IsNullOrEmpty(username))
                            throw new ConfigurationErrorsException("There is no username provided in the configuration file.");
                        break;
                    case "Password":
                        password = item.InnerText;
                        if (string.IsNullOrEmpty(password))
                            throw new ConfigurationErrorsException("There is no password provided in the configuration file.");
                        break;
                    case "Name":
                        name = item.InnerText;
                        if (string.IsNullOrEmpty(name))
                            throw new ConfigurationErrorsException("There is no name provided in the configuration file.");
                        break;
                }
            }
            var database = new DatabaseConfig(url, username, password, name);
            return database;
        }
    }
}
