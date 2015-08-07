using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class CounterDisableCommand : ActionCommandBase
    {
        public CounterDisableCommand()
            : base("Disable", "Disables a counter.")
        {
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            //Have the user select the counter group to disable
            //Find what config-file the counter is in
            //Update the file
            //--> Do not load disabled counters

            //influxDbVersion = QueryParam("Counter Group", GetParam(paramList, index), ...);

            return true;
        }
    }
}