using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Sender
{
    internal class SenderQueueCommand : ActionCommandBase
    {
        private readonly CompositeRoot _compositeRoot;

        public SenderQueueCommand(CompositeRoot compositeRoot)
            : base("Queue", "Show information about the sender queue.")
        {
            _compositeRoot = compositeRoot;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var queueInfos = _compositeRoot.SendBusiness.GetQueueInfo();
            OutputInformation("Total\tQueue\tFail");
            foreach (var queueInfo in queueInfos)
            {
                OutputInformation("{0}\t{1}\t{2}", queueInfo.TotalQueueCount, queueInfo.QueueCount, queueInfo.FailQueueCount);
            }

            return true;
        }
    }
}