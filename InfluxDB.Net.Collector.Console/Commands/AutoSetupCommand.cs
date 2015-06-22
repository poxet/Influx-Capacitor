using System;
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
            var service = new ServiceController("InfluxDB.Net.Collector");
            try
            {
                if (service.Status != ServiceControllerStatus.Running)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 15));
                }
            }
            catch (Exception exception)
            {
                OutputError(exception.Message);
            }
            finally
            {
                OutputInformation("Service " + service.Status);
            }
        }

        private async Task<string> GetServerUrlAsync(string paramList, int index)
        {
            var url = _configBusiness.OpenDatabaseConfig().Url;

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

                _configBusiness.SaveDatabaseUrl(url);
            }
            OutputInformation("Connection to server {0} confirmed.", url);
            return url;
        }

        private async Task<IDatabaseConfig> GetUsernameAsync(string url, string paramList, int index)
        {
            var serie = new Serie.Builder("InfluxDB.Net.Collector").Columns("Machine").Values(Environment.MachineName).Build();

            var config = _configBusiness.OpenDatabaseConfig();

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
                var database = QueryParam<string>("DatabaseName", GetParam(paramList, index++));
                var user = QueryParam<string>("Username", GetParam(paramList, index++));
                var password = QueryParam<string>("Password", GetParam(paramList, index++));
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

            _configBusiness.SaveDatabaseConfig(config.Name, config.Username, config.Password);

            return config;
        }
    }
}