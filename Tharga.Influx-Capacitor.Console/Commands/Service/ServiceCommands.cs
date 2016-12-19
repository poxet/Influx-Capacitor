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
    internal class ServiceCommands : ContainerCommandBase
    {
        public ServiceCommands(ICompositeRoot compositeRoot)
            : base("Service")
        {
            RegisterCommand(new ServiceStatusCommand());
            RegisterCommand(new ServiceStopCommand());
            RegisterCommand(new ServiceStartCommand());
            RegisterCommand(new ServiceRestartCommand());
            RegisterCommand(new ServiceConnectCommand(compositeRoot.SocketClient));
            RegisterCommand(new ServiceDisconnectCommand(compositeRoot.SocketClient));
            RegisterCommand(new ServiceSendCommand(compositeRoot.SocketClient));
            RegisterCommand(new ServiceStartListeningCommand(compositeRoot.SocketClient));
            RegisterCommand(new ServiceStopListeningCommand(compositeRoot.SocketClient));
        }

        public static async Task<ServiceControllerStatus?> GetServiceStatusAsync()
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

            if ((service.Status != ServiceControllerStatus.Stopped) && (service.Status != ServiceControllerStatus.Paused))
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

            if ((service.Status != ServiceControllerStatus.Running) && (service.Status != ServiceControllerStatus.Paused))
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
                        throw new ArgumentOutOfRangeException($"Unknown action {serviceAction}.");
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
            await Task.Run(() =>
            {
                service.WaitForStatus(status, new TimeSpan(0, 0, 15));
                if (service.Status != status)
                    throw new InvalidOperationException($"Waiting for the service state to change to {status} timed out.");
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
            await StopServiceAsync();
            return await StartServiceAsync();
        }
    }
}