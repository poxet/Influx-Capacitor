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
            RegisterCommand(new ServiceCheckCommand());
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
                throw new InvalidOperationException(string.Format("Cannot start service because of current state, {0}.", service.Status));
            }

            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 15));

            if (service.Status != ServiceControllerStatus.Running)
            {
                throw new InvalidOperationException("Cannot start service.");
            }
        }

        public static async Task StopServiceAsync()
        {
            var service = new ServiceController(Constants.ServiceName);
            if (service.Status != ServiceControllerStatus.Running && service.Status != ServiceControllerStatus.Paused)
            {
                throw new InvalidOperationException(string.Format("Cannot start service because of current state, {0}.", service.Status));
            }

            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 15));

            if (service.Status != ServiceControllerStatus.Running)
            {
                throw new InvalidOperationException("Cannot stop service.");
            }
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