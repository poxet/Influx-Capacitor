using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Service
{
    class ServiceStartCommand : ActionCommandBase
    {
        public ServiceStartCommand()
            : base("Start", "Starts the service if it is not running.")
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

            var response = await ServiceCommands.StartServiceAsync();
            OutputInformation("Service is {0}.", response);

            return true;
        }
    }
}