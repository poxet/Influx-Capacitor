using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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

            IDatabaseConfig database = null;
            var groups = new List<ICounterGroup>();

            foreach (var configurationFilename in configurationFilenames)
            {
                var fileData = _fileLoaderAgent.ReadAllText(configurationFilename);

                var document = new XmlDocument();
                document.LoadXml(fileData);

                var db = GetDatabaseConfig(document);
                var grps = GetCounterGroups(document).ToList();

                if (db != null)
                {
                    if (database != null)
                    {
                        throw new InvalidOperationException("There are database configuration sections in more than one config file.");
                    }

                    database = db;
                }

                foreach (var grp in grps)
                {
                    if (groups.Any(x => x.Name == grp.Name))
                    {
                        var ex = new InvalidOperationException("There are more than one counter group in the config files.");
                        ex.Data.Add("GroupName", grp.Name);
                        ex.Data.Add("File", configurationFilename);
                        throw ex;
                    }

                    groups.Add(grp);
                }
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
            xmePassword.InnerText = Encrypt(newDbConfig.Password);
            dme.AppendChild(xmePassword);

            var xmeName = xml.CreateElement("Name");
            xmeName.InnerText = newDbConfig.Name;
            dme.AppendChild(xmeName);

            var xmlData = xml.ToFormattedString();

            _fileLoaderAgent.WriteAllText(databaseConfigFilePath, xmlData);
        }

        private static string Encrypt(string password)
        {
            var crypto = new Crypto(Environment.OSVersion.VersionString);
            var result = crypto.EncryptStringAes(password, Environment.MachineName);
            return result;
        }

        private static string Decrypt(string password)
        {
            var crypto = new Crypto(Environment.OSVersion.VersionString);
            try
            {
                var result = crypto.DecryptStringAes(password, Environment.MachineName);
                return result;
            }
            catch (FormatException exception)
            {
                return password;
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
                        password = Decrypt(item.InnerText);
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

            var applicationFolderPath = _fileLoaderAgent.GetApplicationFolderPath();
            var configFilesInProgramData = _fileLoaderAgent.GetFiles(applicationFolderPath, "*.xml");

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
                }
            }

            var applicationFolderPath = _fileLoaderAgent.GetApplicationFolderPath();
            if (File.Exists(applicationFolderPath + "\\" + fileName))
                return false;

            var contents = document.ToFormattedString();

            File.WriteAllText(applicationFolderPath + "\\" + fileName, contents);

            return true;
        }
    }

    public class Crypto
    {
        private readonly byte[] _salt;

        public Crypto(string salt)
        {
            _salt = Encoding.ASCII.GetBytes(salt ?? "i6113742kbB7c8");
        }

        public string EncryptStringAes(string plainText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            string outStr;
            RijndaelManaged aesAlg = null;

            try
            {
                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Create a decryptor to perform the stream transform.
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return outStr;
        }

        public string DecryptStringAes(string cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext;

            try
            {
                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create the streams used for decryption.                
                var bytes = Convert.FromBase64String(cipherText);
                using (var msDecrypt = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.
                    var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }
    }
}