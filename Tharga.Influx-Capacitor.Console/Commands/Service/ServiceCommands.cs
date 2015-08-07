using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Service
{
    class ServiceCommands : ContainerCommandBase
    {
        public ServiceCommands()
            : base("Service")
        {
            RegisterCommand(new ServiceStatusCommand());
            RegisterCommand(new ServiceStopCommand());
            RegisterCommand(new ServiceStartCommand());
            RegisterCommand(new ServiceRestartCommand());
        }

        public async static Task<ServiceControllerStatus?> GetServiceStatusAsync()
        {
            var service = new ServiceController(Constants.ServiceName);
            try
            {
                return service.Status;
            }
            catch (InvalidOperationException)
            {
                return null;    
            }
        }

        public static async Task StartServiceAsync()
        {
            var service = new ServiceController(Constants.ServiceName);
            if (service.Status != ServiceControllerStatus.Stopped && service.Status != ServiceControllerStatus.Paused)
            {
                var exp = new InvalidOperationException("Cannot start service because of current state.");
                exp.Data.Add("service.Status", service.Status);
                throw exp;
            }

            service.Start();
            await WaitForStatusAsync(service, ServiceControllerStatus.Running);
            //service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 15));

            //if (service.Status != ServiceControllerStatus.Running)
            //{
            //    throw new InvalidOperationException("Cannot start service.");
            //}
        }

        public static async Task StopServiceAsync()
        {
            var service = new ServiceController(Constants.ServiceName);
            if (service.Status != ServiceControllerStatus.Running && service.Status != ServiceControllerStatus.Paused)
            {
                var exp = new InvalidOperationException("Cannot start service because of current state.");
                exp.Data.Add("service.Status", service.Status);
                throw exp;
            }

            service.Start();
            await WaitForStatusAsync(service, ServiceControllerStatus.Stopped);
        }

        private static async Task WaitForStatusAsync(ServiceController service, ServiceControllerStatus status)
        {
            await Task.Factory.StartNew(async () =>
            {
                service.WaitForStatus(status, new TimeSpan(0, 0, 15));
                if (service.Status != status)
                {
                    throw new InvalidOperationException("Waiting for the service state to change to " + status + " timed out.");
                }
            });
        }

        public static async Task RestartServiceAsync()
        {
            var service = new ServiceController(Constants.ServiceName);
            if (service.Status == ServiceControllerStatus.Running || service.Status != ServiceControllerStatus.Paused)
            {
                await StopServiceAsync();
            }

            if (service.Status == ServiceControllerStatus.Stopped)
            {
                await StartServiceAsync();
            }
        }
    }
}