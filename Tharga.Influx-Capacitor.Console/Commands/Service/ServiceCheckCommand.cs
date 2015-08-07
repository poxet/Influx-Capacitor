using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Service
{
    class ServiceCheckCommand : ActionCommandBase
    {
        public ServiceCheckCommand()
            : base("Check", "Check the state of the service.")
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

            OutputInformation("{0}", result.Value);
            return true;
        }
    }
}