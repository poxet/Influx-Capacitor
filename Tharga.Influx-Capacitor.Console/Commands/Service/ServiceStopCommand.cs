using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Service
{
    class ServiceStopCommand : ActionCommandBase
    {
        public ServiceStopCommand()
            : base("Stop", "Stops the service if it is running.")
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

            var response = await ServiceCommands.StopServiceAsync();
            OutputInformation("Service is {0}.", response);

            return true;
        }
    }
}