using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading.Tasks;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Setup
{
    abstract class SetupCommandBase : ActionCommandBase
    {
        private readonly IInfluxDbAgentLoader _influxDbAgentLoader;
        private readonly IConfigBusiness _configBusiness;

        protected SetupCommandBase(string name, string description, IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness)
            : base(name, description)
        {
            _influxDbAgentLoader = influxDbAgentLoader;
            _configBusiness = configBusiness;
        }

        protected async Task<Tuple<string, InfluxDbVersion>> GetServerUrlAsync(string paramList, int index, string defaultUrl, InfluxDbVersion influxDbVersion)
        {
            var url = defaultUrl;

            IInfluxDbAgent client = null;
            if (!string.IsNullOrEmpty(url))
            {                
                client = _influxDbAgentLoader.GetAgent(new DatabaseConfig(url, "root", "qwerty", "qwerty", influxDbVersion));
            }

            var connectionConfirmed = false;
            try
            {
                if (client != null)
                {
                    connectionConfirmed = await client.CanConnect();
                }
            }
            catch (Exception exception)
            {
                OutputError("{0}", exception.Message);
            }

            if (!connectionConfirmed)
            {
                OutputInformation("Enter the url to the InfluxDB to use.");
                OutputInformation("Provide the correct port, typically 8086. (Ex. http://tharga.net:8086)");
                while (!connectionConfirmed)
                {
                    try
                    {
                        url = QueryParam<string>("Url", GetParam(paramList, index));
                        influxDbVersion = QueryParam("Version", GetParam(paramList, index), new Dictionary<InfluxDbVersion, string> { { InfluxDbVersion.Ver0_8X, "0.8x" }, { InfluxDbVersion.Ver0_9X, "0.9x" }, { InfluxDbVersion.Auto, "Auto" } });
                        client = _influxDbAgentLoader.GetAgent(new DatabaseConfig(url, "root", "qwerty", "qwert", influxDbVersion));

                        connectionConfirmed = await client.CanConnect();
                    }
                    catch (CommandEscapeException)
                    {
                        return null;
                    }
                    catch (Exception exception)
                    {
                        OutputError("{0}", exception.Message.Split('\n')[0]);
                    }
                }

                _configBusiness.SaveDatabaseUrl(url, influxDbVersion);
            }
            OutputInformation("Connection to server {0} confirmed.", url);
            return new Tuple<string, InfluxDbVersion>(url, influxDbVersion);
        }

        protected async Task<IDatabaseConfig> GetUsernameAsync(string paramList, int index, IDatabaseConfig config)
        {
            var points = new[] { new Point { Name = Constants.ServiceName, Fields = new Dictionary<string, object> { { "Machine", Environment.MachineName } }, }, };
            var dataChanged = false;

            var url = config.Url;
            var influxDbVersion = config.InfluxDbVersion;

            IInfluxDbAgent client;
            InfluxDbApiResponse response = null;
            try
            {
                if (!string.IsNullOrEmpty(config.Name) && !string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
                {
                    client = _influxDbAgentLoader.GetAgent(config);
                    response = await client.WriteAsync(points);
                }
            }
            catch (Exception exception)
            {
                OutputError(exception.Message);
            }

            if (response == null || !response.Success)
                OutputInformation("Enter the database, username and password for the InfluxDB.");

            while (response == null || !response.Success)
            {
                var database = QueryParam<string>("DatabaseName", GetParam(paramList, index++));
                var user = QueryParam<string>("Username", GetParam(paramList, index++));
                var password = QueryParam<string>("Password", GetParam(paramList, index++));
                config = new DatabaseConfig(url, user, password, database, influxDbVersion);

                try
                {
                    client = _influxDbAgentLoader.GetAgent(config);
                    response = await client.WriteAsync(points);
                    dataChanged = true;
                }
                catch (CommandEscapeException)
                {
                    return null;
                }
                catch (Exception exception)
                {
                    OutputError("{0}", exception.Message);
                }
            }

            OutputInformation("Access to database {0} confirmed.", config.Name);

            if (dataChanged)
                _configBusiness.SaveDatabaseConfig(config.Name, config.Username, config.Password);

            return config;
        }

        protected void StartService(bool restartIfAlreadyRunning)
        {
            OutputInformation("Trying to restart service...");

            var service = new ServiceController(Constants.ServiceName);
            var serviceControllerStatus = "not found";
            try
            {
                if (service.Status == ServiceControllerStatus.Running && restartIfAlreadyRunning)
                {
                    if (!service.CanStop)
                    {
                        OutputWarning("The service cannot be stopped.");
                    }

                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 15));
                }

                if (service.Status != ServiceControllerStatus.Running)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 15));
                }

                serviceControllerStatus = service.Status.ToString();
            }
            catch (Exception exception)
            {
                OutputError("{0}", exception.Message);
            }
            finally
            {
                OutputInformation("Service is " + serviceControllerStatus + ".");
            }
        }
    }
}