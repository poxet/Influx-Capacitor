using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Processor
{
    internal class RunProcessorCommand : ActionCommandBase
    {
        private readonly ICompositeRoot _compositeRoot;

        public RunProcessorCommand(ICompositeRoot compositeRoot)
            : base("Run", "Run the processor.")
        {
            _compositeRoot = compositeRoot;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            await _compositeRoot.Processor.RunAsync(new string[] { }, true);
            return true;
        }
    }
}