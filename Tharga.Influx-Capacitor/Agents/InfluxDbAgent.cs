using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using InfluxDB.Net;
using InfluxDB.Net.Contracts;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Agents
{
    public class QueueSettings : IQueueSettings
    {
        public int FlushSecondsInterval { get; }
        public bool DropOnFail { get; }
        public int MaxQueueSize { get; }
    }

    public class QueueEvents : IQueueEvents
    {
        public void DebugMessageEvent(string message)
        {
            throw new NotImplementedException();
        }

        public void ExceptionEvent(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void SendEvent(ISendEventInfo sendCompleteEventArgs)
        {
            throw new NotImplementedException();
        }

        public void QueueChangedEvent(IQueueChangeEventInfo queueChangeEventInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class AccSenderAgent : ISenderAgent
    {
        public string TargetDescription { get; }

        public Task<ISendResponse> SendAsync(Point[] points)
        {
            throw new NotImplementedException();
        }

        public string PointToString(Point point)
        {
            throw new NotImplementedException();
        }
    }

    public class NullSenderAgent : ISenderAgent
    {
        public string TargetDescription { get; }

        public Task<ISendResponse> SendAsync(Point[] points)
        {
            throw new NotImplementedException();
        }

        public string PointToString(Point point)
        {
            throw new NotImplementedException();
        }
    }

    public class KafkaSenderAgent : ISenderAgent
    {
        public string TargetDescription { get; }

        public Task<ISendResponse> SendAsync(Point[] points)
        {
            throw new NotImplementedException();
        }

        public string PointToString(Point point)
        {
            throw new NotImplementedException();
        }
    }

    public class InfluxDbSenderAgent : ISenderAgent
    {
        public string TargetDescription { get; }

        public Task<ISendResponse> SendAsync(Point[] points)
        {
            throw new NotImplementedException();
        }

        public string PointToString(Point point)
        {
            throw new NotImplementedException();
        }
    }

    [ExcludeFromCodeCoverage]
    public class InfluxDbAgent : IInfluxDbAgent
    {
        private readonly string _databaseName;
        private readonly string _userName;
        private readonly string _password;
        private readonly InfluxDb _influxDb;

        public InfluxDbAgent(string url, string databaseName, string userName, string password, TimeSpan? requestTimeout, InfluxVersion influxVersion = InfluxVersion.Auto)
        {
            _databaseName = databaseName;
            _userName = userName;
            _password = password;

            Uri result;
            if (!Uri.TryCreate(url, UriKind.Absolute, out result))
            {
                var exp = new InvalidOperationException("Unable to parse provided connection as url.");
                exp.Data.Add("Url", url);
                throw exp;
            }

            try
            {
                _influxDb = new InfluxDb(url, userName, password, influxVersion, requestTimeout);
            }
            catch (Exception exception)
            {
                var exp = new InvalidOperationException("Could not establish a connection to the database.", exception);
                exp.Data.Add("Url", url);
                exp.Data.Add("Username", userName);
                throw exp;
            }
        }

        public async Task<InfluxDbApiResponse> WriteAsync(Point[] points)
        {
            return await _influxDb.WriteAsync(_databaseName, points);
        }

        public async Task CreateDatabaseAsync(string databaseName)
        {
            await _influxDb.CreateDatabaseAsync(databaseName);
        }

        public Tuple<IFormatter, InfluxVersion> GetAgentInfo()
        {
            return new Tuple<IFormatter, InfluxVersion>(_influxDb.GetFormatter(), _influxDb.GetClientVersion());
        }

        public IFormatter GetFormatter()
        {
            return _influxDb.GetFormatter();
        }

        public async Task<InfluxDbApiResponse> AuthenticateDatabaseUserAsync()
        {
            return await _influxDb.AuthenticateDatabaseUserAsync(_databaseName, _userName, _password);
        }

        public async Task<bool> CanConnect()
        {
            var pong = await _influxDb.PingAsync();
            return pong.Success;
        }

        public async Task<Pong> PingAsync()
        {
            return await _influxDb.PingAsync();
        }

        public async Task<string> VersionAsync()
        {
            var pong = await _influxDb.PingAsync();
            return pong.Version;
        }
    }
}