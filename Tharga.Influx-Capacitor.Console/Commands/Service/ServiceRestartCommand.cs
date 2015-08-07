using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Service
{
    class ServiceRestartCommand : ActionCommandBase
    {
        public ServiceRestartCommand()
            : base("Restart", "Restarts or starts the service.")
        {
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var result = await ServiceCommands.GetServiceStatusAsync();
            if (result == null)
            {
                OutputWarning("Service is not installed on this machine.");
                return true;
            }

            await ServiceCommands.RestartServiceAsync();

            return true;
        }
    }
}