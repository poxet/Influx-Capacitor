using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Service
{
    internal class ServiceRestartCommand : ActionCommandBase
    {
        public ServiceRestartCommand()
            : base("Restart", "Restarts or starts the service.")
        {
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            var result = await ServiceCommands.GetServiceStatusAsync();
            if (result == null)
            {
                OutputWarning("Service is not installed on this machine.");
                return true;
            }

            var response = await ServiceCommands.RestartServiceAsync();
            OutputInformation("Service is {0}.", response);

            return true;
        }
    }
}