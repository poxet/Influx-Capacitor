using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using InfluxDB.Net.Collector.Entities;
using InfluxDB.Net.Collector.Interface;

namespace InfluxDB.Net.Collector.Business
{
    public class ConfigBusiness : IConfigBusiness
    {
        const string ConfigPath = "Software\\Thargelion\\InfluxDB.Net.Collector\\Database";

        private readonly IFileLoaderAgent _fileLoaderAgent;
        private readonly IRegistryRepository _registryRepository;

        public ConfigBusiness(IFileLoaderAgent fileLoaderAgent, IRegistryRepository registryRepository)
        {
            _fileLoaderAgent = fileLoaderAgent;
            _registryRepository = registryRepository;
        }

        public IConfig LoadFile(string configurationFilename)
        {
            return LoadFiles(new[] { configurationFilename });
        }

        public IConfig LoadFiles(string[] configurationFilenames)
        {
            if (!configurationFilenames.Any())
                configurationFilenames = GetConfigFiles();

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
                        throw new InvalidOperationException("There are database configuration sections in more than one file.");
                    }
                    database = db;
                }
                groups.AddRange(grp);
            }

            if (database == null)
            {
                //Load config part from registry
                database = GetDatabaseFromRegistry();

                if (database == null)
                    throw new InvalidOperationException("No file contains configuration information for the database.");
            }

            var config = new Config(database, groups);
            return config;
        }

        public IDatabaseConfig GetDatabaseFromRegistry()
        {
            var url = _registryRepository.GetSetting(RegistryHKey.LocalMachine, ConfigPath, "Url", string.Empty);
            var databaseName = _registryRepository.GetSetting(RegistryHKey.LocalMachine, ConfigPath, "DatabaseName", string.Empty);
            var username = _registryRepository.GetSetting(RegistryHKey.LocalMachine, ConfigPath, "Username", string.Empty);
            var password = _registryRepository.GetSetting(RegistryHKey.LocalMachine, ConfigPath, "Password", string.Empty);

            return new DatabaseConfig(url, username, password, databaseName);
        }

        public void SetUrl(string url)
        {
            _registryRepository.SetSetting(RegistryHKey.LocalMachine, ConfigPath, "Url", url);
        }

        public void SetDatabase(string databaseName, string username, string password)
        {
            _registryRepository.SetSetting(RegistryHKey.LocalMachine, ConfigPath, "DatabaseName", databaseName);
            _registryRepository.SetSetting(RegistryHKey.LocalMachine, ConfigPath, "Username", username);
            _registryRepository.SetSetting(RegistryHKey.LocalMachine, ConfigPath, "Password", password);
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

        private string[] GetConfigFiles()
        {
            var configFile = ConfigurationManager.AppSettings["ConfigFile"];

            string[] configFiles;
            if (!string.IsNullOrEmpty(configFile))
            {
                configFiles = configFile.Split(';');
            }
            else
            {
                var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                configFiles = Directory.GetFiles(currentDirectory, "*.xml");
            }

            //EventLog.WriteEntry(ServiceName, "Loading config files: " + string.Join(", ", configFiles));

            return configFiles;
        }
    }

    public class Registry
    {
        /// <summary>
        /// Location in registry for auto start settings
        /// </summary>
        public const string AutoStartLocation = @"Software\Microsoft\Windows\CurrentVersion\Run";

        #region Members


        private readonly IRegistryRepository _registryRepository;
        private readonly RegistryHKey _registryHKey;
        private readonly Assembly _assembly;


        #endregion
        #region properties


        private string RegistryPath
        {
            get
            {
                var asmNameArray = _assembly.GetName().Name.Split('.');
                return string.Format("Software\\{0}\\{1}\\{2}", asmNameArray[0], asmNameArray[1], asmNameArray[2]);
            }
        }

        #endregion
        #region Constructors


        /// <summary>
        /// Initializes a new instance of the <see cref="Registry"/> class.
        /// </summary>
        /// <param name="registryHKey">The registry H key.</param>
        /// <param name="assembly">The assembly.</param>
        public Registry(RegistryHKey registryHKey, Assembly assembly)
        {
            _registryRepository = new RegistryRepository();
            _registryHKey = registryHKey;
            _assembly = assembly;
        }

        internal Registry(IRegistryRepository registryRepository, RegistryHKey registryHKey, Assembly assembly)
        {
            _registryRepository = registryRepository;
            _registryHKey = registryHKey;
            _assembly = assembly;
        }


        #endregion
        #region Setting


        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public object GetSetting(string keyName, object defaultValue = null)
        {
            return _registryRepository.GetSetting(_registryHKey, RegistryPath, keyName, defaultValue);
        }

        /// <summary>
        /// Sets the setting.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="value">The value.</param>
        public void SetSetting(string keyName, object value)
        {
            throw new NotImplementedException();
            //_registryRepository.SetSetting(_registryHKey, RegistryPath, keyName, value);
        }

        /// <summary>
        /// Disables auto start for provided assembly.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        public void RemoveSetting(string keyName)
        {
            throw new NotImplementedException();
            //_registryRepository.RemoveSetting(_registryHKey, RegistryPath, keyName);
        }

        /// <summary>
        /// Determines whether the specified key name has setting.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <returns>
        /// 	<c>true</c> if the specified key name has setting; otherwise, <c>false</c>.
        /// </returns>
        public bool HasSetting(string keyName)
        {
            throw new NotImplementedException();
            //return _registryRepository.HasSetting(_registryHKey, RegistryPath, keyName);
        }


        #endregion
        #region AutoStart


        /// <summary>
        /// Enables auto start for provided assembly.
        /// </summary>
        public void SetAutoStart()
        {
            throw new NotImplementedException();
            //_registryRepository.SetAutoStart(_registryHKey, _assembly.GetName().Name, _assembly.Location);
        }

        /// <summary>
        /// Determines whether auto start is enabled for the provided assembly.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if auto start is enabled; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAutoStartEnabled()
        {
            throw new NotImplementedException();
            //return _registryRepository.IsAutoStartEnabled(_registryHKey, _assembly.GetName().Name, _assembly.Location);
        }

        /// <summary>
        /// Removes the auto start.
        /// </summary>
        public void RemoveAutoStart()
        {
            throw new NotImplementedException();
            //_registryRepository.RemoveAutoStart(_registryHKey, _assembly.GetName().Name);
        }

        ///// <summary>
        ///// Uns the set auto start.
        ///// </summary>
        ///// <param name="environment">The environment.</param>
        ///// <param name="assembly">The assembly.</param>
        //public static void UnSetAutoStart(Environment environment, Assembly assembly)
        //{
        //    UnSetAutoStart(environment, assembly.GetName().Name);
        //}

        //private static void UnSetAutoStart(Environment environment, string keyName)
        //{
        //    var key = GetKey(environment, RunLocation);
        //    if (key == null) throw new InvalidOperationException(string.Format("Cannot get key for registry path {0}.", RunLocation));
        //    key.DeleteValue(keyName);
        //}

        /////// <summary>
        /////// Sets the auto start.
        /////// </summary>
        /////// <param name="doAutoStart">if set to <c>true</c> [do auto start].</param>
        /////// <param name="environment">The environment.</param>
        ////public static void SetAutoStart(bool doAutoStart, Environment environment = Environment.CurrentUser)
        ////{
        ////    if (doAutoStart)
        ////        SetAutoStart(environment, Assembly.GetEntryAssembly());
        ////    else
        ////    {
        ////        if (IsAutoStartEnabled(environment, Assembly.GetEntryAssembly()))
        ////            UnSetAutoStart(environment, Assembly.GetEntryAssembly());
        ////    }
        ////}


        #endregion
    }
}