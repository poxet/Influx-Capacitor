using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
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

        public static async Task<string> StartServiceAsync()
        {
            var service = new ServiceController(Constants.ServiceName);

            if (service.Status == ServiceControllerStatus.Running)
                return service.Status.ToString();

            if (service.Status != ServiceControllerStatus.Stopped && service.Status != ServiceControllerStatus.Paused)
            {
                var exp = new InvalidOperationException("Cannot start service because of current state.");
                exp.Data.Add("service.Status", service.Status);
                throw exp;
            }

            ExecuteServiceAction(service, ServiceAction.Start);

            await WaitForStatusAsync(service, ServiceControllerStatus.Running);
            return service.Status.ToString();
        }

        public static async Task<string> StopServiceAsync()
        {
            var service = new ServiceController(Constants.ServiceName);

            if (service.Status == ServiceControllerStatus.Stopped)
                return service.Status.ToString();

            if (service.Status != ServiceControllerStatus.Running && service.Status != ServiceControllerStatus.Paused)
            {
                var exp = new InvalidOperationException("Cannot stop service because of current state.");
                exp.Data.Add("service.Status", service.Status);
                throw exp;
            }

            ExecuteServiceAction(service, ServiceAction.Stop);

            await WaitForStatusAsync(service, ServiceControllerStatus.Stopped);
            return service.Status.ToString();
        }

        private static void ExecuteServiceAction(ServiceController service, ServiceAction serviceAction)
        {
            var pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            var hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            if (hasAdministrativeRight)
            {
                switch (serviceAction)
                {
                    case ServiceAction.Start:
                        service.Start();
                        break;
                    case ServiceAction.Stop:
                        service.Stop();
                        break;
                    case ServiceAction.Restart:
                        service.Start();
                        service.Stop();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(string.Format("Unknown action {0}.", serviceAction));
                }
            }
            else
            {
                string arguments;
                switch (serviceAction)
                {
                    case ServiceAction.Start:
                        arguments = "\"service start\"";
                        break;
                    case ServiceAction.Stop:
                        arguments = "\"service stop\"";
                        break;
                    case ServiceAction.Restart:
                        arguments = "\"service restart\"";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(string.Format("Unknown action {0}.", serviceAction));
                }

                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = Assembly.GetExecutingAssembly().Location,
                    Verb = "runas",                    
                    Arguments = arguments, //start ? "\"service start\"" : "\"service stop\"",
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                var p = Process.Start(startInfo);
                p.WaitForExit();
            }
        }

        private static async Task WaitForStatusAsync(ServiceController service, ServiceControllerStatus status)
        {
            await Task.Factory.StartNew(async () =>
            {
                service.WaitForStatus(status, new TimeSpan(0, 0, 15));
                if (service.Status != status)
                {
                    throw new InvalidOperationException(string.Format("Waiting for the service state to change to {0} timed out.", status));
                }
            });
        }

        public static async Task<string> RestartServiceAsync()
        {
            //Check if rights are elevated. IF not Run the action
            var pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            var hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!hasAdministrativeRight)
            {
                var service = new ServiceController(Constants.ServiceName);
                ExecuteServiceAction(service, ServiceAction.Restart);
                return service.Status.ToString();
            }
            else
            {
                await StopServiceAsync();
                return await StartServiceAsync();
            }
        }
    }

    public enum ServiceAction { Start, Stop, Restart }
}