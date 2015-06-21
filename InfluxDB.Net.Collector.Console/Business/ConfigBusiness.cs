using System.Configuration;
using System.Xml;
using InfluxDB.Net.Collector.Console.Entities;

namespace InfluxDB.Net.Collector.Console.Business
{
    public class ConfigBusiness
    {
        public Config LoadFile(string configurationFilename)
        {
            var document = new XmlDocument();
            document.Load(configurationFilename);

            var database = GetDatabaseConfig(document);

            var config = new Config(database);
            return config;
        }

        private static DatabaseConfig GetDatabaseConfig(XmlDocument document)
        {
            var databases = document.GetElementsByTagName("Database");
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
