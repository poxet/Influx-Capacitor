//using System;
//using InfluxDB.Net.Collector.Interface;
//using Microsoft.Win32;

//namespace InfluxDB.Net.Collector.Business
//{
//    public class RegistryRepository : IRegistryRepository
//    {
//        private static RegistryKey GetKey(RegistryHKey environment, string path)
//        {
//            RegistryKey key;
//            switch (environment)
//            {
//                case RegistryHKey.CurrentUser:
//                    key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(path);
//                    break;
//                case RegistryHKey.LocalMachine:
//                    key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(path);
//                    break;
//                default:
//                    throw new InvalidOperationException(string.Format("Unknown environment {0}.", environment));
//            }

//            return key;
//        }

//        public T GetSetting<T>(RegistryHKey registryHKey, string path, string keyName, T defaultValue)
//        {
//            var key = GetKey(registryHKey, path);
//            if (key == null) throw new InvalidOperationException(string.Format("Cannot get key for registry path {0}.", path));

//            var value = key.GetValue(keyName);
//            if (value == null)
//            {
//                if (defaultValue == null) throw new InvalidOperationException(string.Format("Cannot find setting for registry path {0} and key {1} and there is no default value provided.", path, keyName));

//                key.SetValue(keyName, defaultValue);
//                return defaultValue;
//            }
//            return (T)Convert.ChangeType(value, typeof(T));
//        }

//        public void SetSetting<T>(RegistryHKey registryHKey, string path, string keyName, T value)
//        {
//            if (path == null) throw new ArgumentNullException("path", "Path cannot be null when saving to registry.");
//            if (keyName == null) throw new ArgumentNullException("keyName", string.Format("KeyName cannot be null when saving to registry path '{0}'.", path));
//            if (value == null) throw new ArgumentNullException("value", string.Format("Value cannot be null when saving to registry path '{0}' with key '{1}'.", path, keyName));

//            var key = GetKey(registryHKey, path);
//            if (key == null) throw new InvalidOperationException(string.Format("Cannot get key for registry path {0}.", path));

//            key.SetValue(keyName, value);
//        }
//    }
//}