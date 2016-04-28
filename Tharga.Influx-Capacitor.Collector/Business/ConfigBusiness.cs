using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Collector.Publishers;
using Random = Tharga.InfluxCapacitor.Collector.Publishers.Random;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class ConfigBusiness : IConfigBusiness
    {
        public event EventHandler<InvalidConfigEventArgs> InvalidConfigEvent;

        private readonly IFileLoaderAgent _fileLoaderAgent;

        public ConfigBusiness(IFileLoaderAgent fileLoaderAgent)
        {
            _fileLoaderAgent = fileLoaderAgent;
        }

        public IConfig LoadFile(string configurationFilename)
        {
            return LoadFiles(new[] { configurationFilename });
        }

        public IConfig LoadFiles()
        {
            return LoadFiles(new string[] { });
        }

        public IConfig LoadFiles(string[] configurationFilenames)
        {
            if (!configurationFilenames.Any())
            {
                configurationFilenames = GetConfigFiles().ToArray();
            }

            var databases = new List<IDatabaseConfig>();
            var groups = new List<ICounterGroup>();
            var publishers = new List<ICounterPublisher>();
            IApplicationConfig application = null;
            var tags = new List<ITag>();
            var providers = new List<ICounterProviderConfig>();

            foreach (var configurationFilename in configurationFilenames)
            {
                var fileData = _fileLoaderAgent.ReadAllText(configurationFilename);

                var document = new XmlDocument();
                document.LoadXml(fileData);

                var dbs = GetDatabaseConfig(document);
                var grps = GetCounterGroups(document).ToList();
                var pbs = GetPublishers(document).ToList();
                var app = GetApplicationConfig(document);
                var tgs = GetGlobalTags(document);
                var pcs = GetCounterProviderConfig(document);

                foreach (var db in dbs)
                {
                    databases.Add(db);
                }

                foreach (var grp in grps)
                {
                    if (groups.Any(x => x.Name == grp.Name))
                    {
                        OnInvalidConfigEvent(grp.Name, null, configurationFilename);
                    }
                    else
                    {
                        groups.Add(grp);
                    }
                }

                foreach (var pb in pbs)
                {
                    if (publishers.Any(x => pb.CounterName == x.CounterName))
                    {
                        OnInvalidConfigEvent(pb.CounterName, null, configurationFilename);
                    }
                    else
                    {
                        publishers.Add(pb);
                    }
                }

                foreach (var pc in pcs)
                {
                    pc.Load(typeof(PerformanceCounterProvider).Assembly, typeof(PerformanceCounterProvider).Namespace);
                    providers.Add(pc);
                }

                if (app != null)
                {
                    if (application != null)
                    {
                        throw new InvalidOperationException("There are application configuration sections in more than one config file.");
                    }

                    application = app;
                }

                foreach (var tg in tgs)
                {
                    if (tags.Any(x => x.Name == tg.Name))
                    {
                        OnInvalidConfigEvent(null, tg.Name, configurationFilename);
                    }
                    else
                    {
                        tags.Add(tg);
                    }
                }
            }

            if (application == null)
            {
                application = new ApplicationConfig(10, false, true, 20000);
            }

            var config = new Config(databases, application, groups, publishers, tags, providers);
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

        public IEnumerable<IDatabaseConfig> OpenDatabaseConfig()
        {
            var path = GetAppDataFolder();
            var databaseConfigFilePath = path + "\\database.xml";
            if (!_fileLoaderAgent.DoesFileExist(databaseConfigFilePath))
            {
                return new List<InfluxDatabaseConfig> { new InfluxDatabaseConfig(true, Constants.NoConfigUrl, null, null, null, null) };
            }

            var config = LoadFile(databaseConfigFilePath);
            return config.Databases;
        }

        public void SaveDatabaseUrl(string url)
        {
            var config = OpenDatabaseConfig().First();
            var newDbConfig = new InfluxDatabaseConfig(true, url, config.Username, config.Password, config.Name, config.RequestTimeout);
            SaveDatabaseConfigEx(newDbConfig);
        }

        public void SaveDatabaseConfig(string databaseName, string username, string password)
        {
            var config = OpenDatabaseConfig().First();
            var newDbConfig = new InfluxDatabaseConfig(true, config.Url, username, password, databaseName, null);
            SaveDatabaseConfigEx(newDbConfig);
        }

        private void SaveDatabaseConfigEx(InfluxDatabaseConfig newDbConfig)
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
            xmePassword.InnerText = Encrypt(newDbConfig.Password);
            dme.AppendChild(xmePassword);

            var xmeName = xml.CreateElement("Name");
            xmeName.InnerText = newDbConfig.Name;
            dme.AppendChild(xmeName);

            var xmlData = xml.ToFormattedString();

            _fileLoaderAgent.WriteAllText(databaseConfigFilePath, xmlData);
        }

        public void SaveApplicationConfig(int flushSecondsInterval, bool debugMode, bool metadata, int maxQueueSize)
        {
            var newDbConfig = new ApplicationConfig(flushSecondsInterval, debugMode, metadata, maxQueueSize);
            SaveApplicationConfigEx(newDbConfig);
        }

        public void InitiateApplicationConfig()
        {
            var path = GetAppDataFolder();
            var applicationConfigFilePath = path + "\\application.xml";

            if (File.Exists(applicationConfigFilePath))
                return;

            SaveApplicationConfig(10, false, true, 20000);
        }

        private void SaveApplicationConfigEx(ApplicationConfig applicationConfig)
        {
            var path = GetAppDataFolder();
            var applicationConfigFilePath = path + "\\application.xml";

            var xml = new XmlDocument();
            var xme = xml.CreateElement(Constants.ServiceName);
            xml.AppendChild(xme);
            var dme = xml.CreateElement("Application");
            xme.AppendChild(dme);

            var xmeFlushSecondsInterval = xml.CreateElement("FlushSecondsInterval");
            xmeFlushSecondsInterval.InnerText = applicationConfig.FlushSecondsInterval.ToString();
            dme.AppendChild(xmeFlushSecondsInterval);

            if (applicationConfig.DebugMode)
            {
                var xmeDebug = xml.CreateElement("Debug");
                xmeDebug.InnerText = applicationConfig.DebugMode.ToString();
                dme.AppendChild(xmeDebug);
            }

            var xmeMetadata = xml.CreateElement("Metadata");
            xmeMetadata.InnerText = applicationConfig.Metadata.ToString();
            dme.AppendChild(xmeMetadata);

            var xmeMaxQueueSize = xml.CreateElement("MaxQueueSize");
            xmeMaxQueueSize.InnerText = applicationConfig.MaxQueueSize.ToString();
            dme.AppendChild(xmeMaxQueueSize);

            var xmlData = xml.ToFormattedString();

            _fileLoaderAgent.WriteAllText(applicationConfigFilePath, xmlData);
        }

        private static string Encrypt(string password)
        {
            if (string.IsNullOrEmpty(password)) return password;

            var crypto = new Crypto();
            var result = crypto.EncryptStringAes(password, Environment.MachineName);
            return result;
        }

        private static string Decrypt(string password)
        {
            if (string.IsNullOrEmpty(password)) return password;

            var crypto = new Crypto();
            try
            {
                var result = crypto.DecryptStringAes(password, Environment.MachineName);
                return result;
            }
            catch (FormatException)
            {
                return password;
            }
            catch (Exception)
            {
                return password;
            }
        }

        private IEnumerable<ITag> GetGlobalTags(XmlDocument document)
        {
            var tags = document.GetElementsByTagName("GlobalTag");
            foreach (XmlElement tag in tags)
            {
                yield return GetTag(tag);
            }
            
            foreach (XmlElement child in document.LastChild.ChildNodes)
            {
                if (child.Name == "Tag")
                {
                    yield return GetTag(child);
                }
            }
        }

        private IEnumerable<ICounterProviderConfig> GetCounterProviderConfig(XmlDocument document)
        {
            foreach (var providersElement in document.DocumentElement.ChildNodes.OfType<XmlElement>().Where(e => e.Name == "Providers"))
            {
                foreach (var providerElement in providersElement.ChildNodes.OfType<XmlElement>().Where(e => e.Name == "Provider"))
                {
                    var providerNameAttribute = providerElement.Attributes.GetNamedItem("Name");
                    if (providerNameAttribute == null)
                    {
                        throw new ArgumentOutOfRangeException("Required provider name attribute missing");
                    }

                    var providerName = providerNameAttribute.Value;

                    var providerTypeAttribute = providerElement.Attributes.GetNamedItem("Type");
                    if (providerTypeAttribute == null)
                    {
                        throw new ArgumentOutOfRangeException("Required provider type attribute missing");
                    }

                    var providerType = providerTypeAttribute.Value;

                    var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var attribute in providerElement.Attributes.Cast<XmlAttribute>().Where(attribute => attribute.Name != "Name"))
                    {
                        values[attribute.Name] = attribute.Value;
                    }

                    foreach (var element in providerElement.ChildNodes.OfType<XmlElement>())
                    {
                        values[element.Name] = element.InnerText;
                    }

                    yield return new CounterProviderConfig(providerName, providerType, values);
                }
            }
        }


        private IEnumerable<ICounterGroup> GetCounterGroups(XmlDocument document)
        {
            var counterGroups = document.GetElementsByTagName("CounterGroup");
            foreach (XmlElement counterGroup in counterGroups)
            {
                yield return GetCounterGroup(counterGroup);
            }
        }

        private IEnumerable<ICounterPublisher> GetPublishers(XmlDocument document)
        {
            var counterPublishers = document.GetElementsByTagName("CounterPublisher");
            foreach (XmlElement counterPublisher in counterPublishers)
            {
                yield return GetCounterPublisher(counterPublisher);
            }
        }

        private ITag GetTag(XmlElement tag)
        {
            var name = tag.GetElementsByTagName("Name")[0].InnerText;
            var value = tag.GetElementsByTagName("Value")[0].InnerText;
            return new Tag(name, value);
        }

        private ICounterPublisher GetCounterPublisher(XmlElement counterPublisher)
        {
            var name = GetString(counterPublisher, "Name");
            var secondsInterval = GetInt(counterPublisher, "SecondsInterval");

            switch (name)
            {
                case "TotalMemory":
                    return new TotalMemory(secondsInterval);
                case "Random":
                    return new Random(secondsInterval);
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Unknown counter publisher {0}.", name));
            }
        }

        private ICounterGroup GetCounterGroup(XmlElement counterGroup)
        {
            var name = GetString(counterGroup, "Name");
            var secondsInterval = GetInt(counterGroup, "SecondsInterval");
            var refreshInstanceInterval = GetInt(counterGroup, "RefreshInstanceInterval", 0);
            var collectorEngineType = GetString(counterGroup, "CollectorEngineType", "Safe");
            var providerName = GetString(counterGroup, "Provider", string.Empty);
            var allowDuplicateInstanceNames = GetBool(counterGroup, "AllowDuplicateInstanceNames", false);

            var counters = counterGroup.GetElementsByTagName("Counter");
            var cts = new List<ICounter>();
            foreach (XmlElement counter in counters)
            {
                cts.Add(GetCounter(counter));
            }

            var tags = new List<ITag>();
            var tagElements1 = counterGroup.GetElementsByTagName("CounterGroupTag");
            foreach (XmlElement tagElement in tagElements1)
            {
                tags.Add(GetTag(tagElement));
            }
            var tagElements2 = counterGroup.GetElementsByTagName("GroupTag");
            foreach (XmlElement tagElement in tagElements2)
            {
                tags.Add(GetTag(tagElement));
            }

            foreach (XmlElement child in counterGroup.ChildNodes)
            {
                if (child.Name == "Tag")
                {
                    tags.Add(GetTag(child));
                }
            }

            List<ICounterInstanceFilter> filters = null;
            var filtersElement = counterGroup.ChildNodes.OfType<XmlElement>().FirstOrDefault(n => n.Name == "InstanceFilters");
            if (filtersElement != null)
            {
                filters = filtersElement.ChildNodes
                    .OfType<XmlElement>()
                    .Where(e => e.Name == "Filter")
                    .Select(e => (ICounterInstanceFilter)new CounterInstanceFilter(e.GetAttribute("Pattern"), e.GetAttribute("Replacement")))
                    .ToList();
            }

            CollectorEngineType cet;
            if (!Enum.TryParse(collectorEngineType, out cet))
            {
                cet = CollectorEngineType.Safe;
            }

            return new CounterGroup(name, secondsInterval, refreshInstanceInterval, cts, tags, cet, filters, providerName, allowDuplicateInstanceNames);
        }

        private static string GetString(XmlElement element, string name, string defaultValue = null)
        {
            var attr = element.Attributes.GetNamedItem(name);
            if (attr == null || string.IsNullOrEmpty(attr.Value))
            {
                if (defaultValue == null)
                    throw new InvalidOperationException(string.Format("No {0} attribute specified for the CounterGroup.", name));
                return defaultValue;
            }

            return attr.Value;
        }

        private static int GetInt(XmlElement element, string name, int? defaultValue = null)
        {
            var stringValue = GetString(element, name, defaultValue != null ? defaultValue.ToString() : null);
            int value;
            if (!int.TryParse(stringValue, out value))
            {
                if (defaultValue == null)
                    throw new InvalidOperationException(string.Format("Cannot parse attribute {0} value to integer.", name));
                return defaultValue.Value;
            }

            return value;
        }

        private static bool GetBool(XmlElement element, string name, bool? defaultValue = null)
        {
            var stringValue = GetString(element, name, defaultValue != null ? defaultValue.ToString() : null);
            bool value;
            if (!bool.TryParse(stringValue, out value))
            {
                if (defaultValue == null)
                    throw new InvalidOperationException(string.Format("Cannot parse attribute {0} value to bool.", name));
                return defaultValue.Value;
            }

            return value;
        }

        private ICounter GetCounter(XmlElement counter)
        {
            string categoryName = null;
            string counterName = null;
            string instanceName = null;
            string instanceAlias = null;
            string fieldName = null;
            string machineName = null;
            float? max = null;

            var tags = new List<ITag>();
            
            var tagElements1 = counter.GetElementsByTagName("CounterTag");
            foreach (XmlElement tagElement in tagElements1)
            {
                tags.Add(GetTag(tagElement));
            }
            var tagElements2 = counter.GetElementsByTagName("Tag");
            foreach (XmlElement tagElement in tagElements2)
            {
                tags.Add(GetTag(tagElement));
            }

            foreach (XmlElement item in counter.ChildNodes.OfType<XmlElement>())
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
                        instanceAlias = item.GetAttribute("Alias");
                        break;
                    case "FieldName":
                        fieldName = item.InnerText;
                        break;
                    case "MachineName":
                        machineName = item.InnerText;
                        break;
                    case "Limits":
                        var maxText = item.GetAttribute("Max");
                        if (!string.IsNullOrEmpty(maxText))
                        {
                            float maxValue;
                            if (float.TryParse(maxText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out maxValue))
                            {
                                max = maxValue;
                            }
                        }
                        break;

                }
            }

            return new Counter(categoryName, counterName, instanceName, fieldName, instanceAlias, tags, max, machineName);
        }

        private static ApplicationConfig GetApplicationConfig(XmlDocument document)
        {
            var databases = document.GetElementsByTagName("Application");
            if (databases.Count == 0)
                return null;

            var flushSecondsInterval = 10;
            var debugMode = false;
            var metadata = true;
            var maxQueueSize = 20000;
            foreach (XmlElement item in databases[0].ChildNodes)
            {
                switch (item.Name)
                {
                    case "FlushSecondsInterval":
                        if (!int.TryParse(item.InnerText, out flushSecondsInterval))
                        {
                            flushSecondsInterval = 10;
                        }
                        break;
                    case "Debug":
                        if (!bool.TryParse(item.InnerText, out debugMode))
                        {
                            debugMode = false;
                        }
                        break;
                    case "Metadata":
                        if (!bool.TryParse(item.InnerText, out metadata))
                        {
                            metadata = false;
                        }
                        break;
                    case "MaxQueueSize":
                        if (!int.TryParse(item.InnerText, out maxQueueSize))
                        {
                            maxQueueSize = 20000;
                        }
                        break;
                    case "":
                        break;
                }
            }

            var database = new ApplicationConfig(flushSecondsInterval, debugMode, metadata, maxQueueSize);
            return database;
        }

        private static IEnumerable<IDatabaseConfig> GetDatabaseConfig(XmlDocument document)
        {
            var databases = document.GetElementsByTagName("Database");            
            foreach (XmlNode database in databases)
            {
                var databaseType = GetDatabaseType(database);
                switch (databaseType)
                {
                    case "null":
                        yield return GetNullDatabaseConfig(database);
                        break;
                    case "acc":
                        yield return GetAccDatabaseConfig(database);
                        break;
                    case "kafka":
                        yield return GetKafkaDatabaseConfig(database);
                        break;
                    case "influx":
                    case "influxdb":
                    case "":
                        yield return GetInfluxDatabaseConfig(database);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(string.Format("There is no database type with name {0}.", databaseType));
                }
            }
        }

        private static bool IsDatabaseEnabled(XmlNode database)
        {
            bool enabled;

            var attr = database.Attributes.GetNamedItem("Enabled");
            if (attr == null)
            {
                enabled = true;
            }
            else if (!bool.TryParse(attr.Value, out enabled))
            {
                enabled = true;
            }

            return enabled;
        }

        private static IDatabaseConfig GetNullDatabaseConfig(XmlNode database)
        {
            var enabled = IsDatabaseEnabled(database);

            return new NullDatabaseConfig(enabled);
        }

        private static IDatabaseConfig GetAccDatabaseConfig(XmlNode database)
        {
            var enabled = IsDatabaseEnabled(database);

            return new AccDatabaseConfig(enabled);
        }

        private static IDatabaseConfig GetKafkaDatabaseConfig(XmlNode database)
        {
            var enabled = IsDatabaseEnabled(database);

            string url = null;
            foreach (XmlElement item in database.ChildNodes)
            {
                switch (item.Name)
                {
                    case "Url":
                        url = item.InnerText;
                        break;
                    case "":
                        break;
                }
            }

            return new KafkaDatabaseConfig(enabled, url);
        }

        private static IDatabaseConfig GetInfluxDatabaseConfig(XmlNode database)
        {
            var enabled = IsDatabaseEnabled(database);

            string url = null;
            string username = null;
            string password = null;
            string name = null;
            TimeSpan? requestTimeout = null;
            foreach (XmlElement item in database.ChildNodes)
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
                        password = Decrypt(item.InnerText);
                        break;
                    case "Name":
                        name = item.InnerText;
                        break;
                    case "RequestTimeoutMs":
                        requestTimeout = new TimeSpan(0, 0, 0, 0, int.Parse(item.InnerText));
                        break;
                    case "":
                        break;
                }
            }

            var db = new InfluxDatabaseConfig(enabled, url, username, password, name, requestTimeout);
            return db;
        }

        private static string GetDatabaseType(XmlNode database)
        {
            if (database.Attributes != null)
            {
                var dt = database.Attributes.GetNamedItem("Type");
                if (dt != null)
                {
                    return dt.Value.ToLower();
                }
            }

            return string.Empty;
        }

        public IEnumerable<string> GetConfigFiles()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configFilesInCurrentDirectory = _fileLoaderAgent.GetFiles(currentDirectory, "*.xml");

            var applicationFolderPath = _fileLoaderAgent.GetApplicationFolderPath();
            var configFilesInProgramData = _fileLoaderAgent.GetFiles(applicationFolderPath, "*.xml");

            foreach (var configFile in configFilesInCurrentDirectory.Union(configFilesInProgramData))
            {
                var fileData = _fileLoaderAgent.ReadAllText(configFile);

                XmlDocument document = null;
                try
                {
                    document = new XmlDocument();
                    document.LoadXml(fileData);
                }
                catch (XmlException exception)
                {
                    OnInvalidConfigEvent(exception, configFile);
                }

                if (document != null)
                {
                    var db = GetDatabaseConfig(document);
                    var grp = GetCounterGroups(document).ToList();
                    var app = GetApplicationConfig(document);
                    var tgs = GetGlobalTags(document);

                    if (db != null || grp.Any() || app != null || tgs != null)
                    {
                        yield return configFile;
                    }
                }
            }
        }

        public bool CreateConfig(string fileName, List<ICounterGroup> counterGroups)
        {
            var document = new XmlDocument();
            var xme = document.CreateElement(Constants.ServiceName);
            document.AppendChild(xme);
            var groupElements = document.CreateElement("CounterGroups");
            xme.AppendChild(groupElements);
            foreach (var group in counterGroups)
            {
                var groupElement = document.CreateElement("CounterGroup");
                groupElement.SetAttribute("Name", group.Name);
                groupElement.SetAttribute("SecondsInterval", group.SecondsInterval.ToString());
                groupElement.SetAttribute("RefreshInstanceInterval", group.RefreshInstanceInterval.ToString());

                foreach (var counter in group.Counters)
                {
                    groupElements.AppendChild(groupElement);
                    var counterElement = document.CreateElement("Counter");
                    groupElement.AppendChild(counterElement);

                    var categoryName = document.CreateElement("CategoryName");
                    categoryName.InnerText = counter.CategoryName;
                    counterElement.AppendChild(categoryName);

                    var counterName = document.CreateElement("CounterName");
                    counterName.InnerText = counter.CounterName;
                    counterElement.AppendChild(counterName);

                    var instanceName = document.CreateElement("InstanceName");
                    instanceName.InnerText = counter.InstanceName;
                    counterElement.AppendChild(instanceName);

                    // we add the "FieldName" element only if a value is specified
                    if (!string.IsNullOrEmpty(counter.FieldName))
                    {
                        var fieldName = document.CreateElement("FieldName");
                        fieldName.InnerText = counter.FieldName;
                        counterElement.AppendChild(fieldName);
                    }
                }
            }

            var applicationFolderPath = _fileLoaderAgent.GetApplicationFolderPath();
            if (File.Exists(applicationFolderPath + "\\" + fileName))
                return false;

            var contents = document.ToFormattedString();

            File.WriteAllText(applicationFolderPath + "\\" + fileName, contents);

            return true;
        }

        protected virtual void OnInvalidConfigEvent(Exception exception, string configFileName)
        {
            var handler = InvalidConfigEvent;
            if (handler != null)
            {
                handler(this, new InvalidConfigEventArgs(exception, configFileName));
            }
        }

        protected virtual void OnInvalidConfigEvent(string groupName, string tagName, string configFileName)
        {
            var handler = InvalidConfigEvent;
            if (handler != null)
            {
                handler(this, new InvalidConfigEventArgs(groupName, tagName, configFileName));
            }
        }
    }
}
