using System;
using System.Collections.Generic;
using InfluxDB.Net;
using Tharga.InfluxCapacitor.Collector.Event;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IConfigBusiness
    {
        IConfig LoadFile(string configurationFilename);
        IConfig LoadFiles();
        IConfig LoadFiles(string[] configurationFilenames);
        IDatabaseConfig OpenDatabaseConfig();
        void SaveDatabaseUrl(string url, InfluxDbVersion influxDbVersion);
        void SaveDatabaseConfig(string databaseName, string username, string password);
        IEnumerable<string> GetConfigFiles();
        bool CreateConfig(string fileName, List<ICounterGroup> counterGroups);
        event EventHandler<InvalidConfigEventArgs> InvalidConfigEvent;
    }
}