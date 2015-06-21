using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading.Tasks;
using InfluxDB.Net.Collector.Entities;
using InfluxDB.Net.Collector.Interface;
using InfluxDB.Net.Models;
using Tharga.Toolkit.Console.Command.Base;

namespace InfluxDB.Net.Collector.Console.Commands
{
    internal class AutoSetupCommand : ActionCommandBase
    {
        private readonly IInfluxDbAgentLoader _influxDbAgentLoader;
        private readonly IConfigBusiness _configBusiness;

        public AutoSetupCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
            : base("Auto", "Automatically run full setup.")
        {
            _influxDbAgentLoader = influxDbAgentLoader;
            _configBusiness = configBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var index = 0;
            
            var url = await GetServerUrlAsync(paramList, index++);
            if (string.IsNullOrEmpty(url)) 
                return false;

            var logonInfo = await GetUsernameAsync(url, paramList, index++);
            if (logonInfo == null)
                return false;

            StartService();

            return true;
        }

        private void StartService()
        {
            try
            {
                var timeout = new TimeSpan(5000);
                var service = new ServiceController("InfluxDB.Net.Collector");
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception exception)
            {
                OutputError(exception.Message);
            }
        }

        private async Task<string> GetServerUrlAsync(string paramList, int index)
        {
            var url = _configBusiness.GetDatabaseFromRegistry().Url;

            IInfluxDbAgent client = null;
            if (!string.IsNullOrEmpty(url))
                client = _influxDbAgentLoader.GetAgent(new DatabaseConfig(url, "root", "qwerty", "qwert"));

            var connectionConfirmed = false;
            try
            {
                if (client != null)
                    connectionConfirmed = await client.CanConnect();
            }
            catch (Exception exception)
            {
                OutputError(exception.Message);
            }

            if (!connectionConfirmed)
            {
                while (!connectionConfirmed)
                {
                    try
                    {
                        url = QueryParam<string>("Url", GetParam(paramList, index));
                        client = _influxDbAgentLoader.GetAgent(new DatabaseConfig(url, "root", "qwerty", "qwert"));

                        connectionConfirmed = await client.CanConnect();
                    }
                    catch (CommandEscapeException)
                    {
                        return null;
                    }
                    catch (Exception exception)
                    {
                        OutputError(exception.Message);
                    }
                }

                _configBusiness.SetUrl(url);
            }
            OutputInformation("Connection to server {0} confirmed.", url);
            return url;
        }

        private async Task<IDatabaseConfig> GetUsernameAsync(string url, string paramList, int index)
        {
            var serie = new Serie.Builder("InfluxDB.Net.Collector").Columns("Machine").Values(Environment.MachineName).Build();

            var config = _configBusiness.GetDatabaseFromRegistry();

            IInfluxDbAgent client;
            InfluxDbApiResponse response = null;
            try
            {
                if (!string.IsNullOrEmpty(config.Name) && !string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
                {
                    client = _influxDbAgentLoader.GetAgent(config); 
                    response = await client.WriteAsync(TimeUnit.Milliseconds, serie);
                }
            }
            catch (Exception exception)
            {
                OutputError(exception.Message);
            }

            while (response == null || !response.Success)
            {
                var config1 = config;
                var database = QueryParam("DatabaseName", GetParam(paramList, index++), () => new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(config1.Name, config1.Name) });
                var user = QueryParam("Username", GetParam(paramList, index++), () => new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(config1.Username, config1.Username) });
                var password = QueryParam("Password", GetParam(paramList, index++), () => new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(config1.Password, config1.Password) });
                config = new DatabaseConfig(url, user, password, database);

                try
                {
                    client = _influxDbAgentLoader.GetAgent(config); 
                    response = await client.WriteAsync(TimeUnit.Milliseconds, serie);
                }
                catch (CommandEscapeException)
                {
                    return null;
                }
                catch (Exception exception)
                {
                    OutputError(exception.Message);
                }
            }

            OutputInformation("Access to database {0} confirmed.", config.Name);

            _configBusiness.SetDatabase(config.Name, config.Username, config.Password);

            return config;
        }
    }
}