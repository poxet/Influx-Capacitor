using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Sender
{
    internal class SenderCommands : ContainerCommandBase
    {
        public SenderCommands(CompositeRoot compositeRoot)
            : base("Sender")
        {
            RegisterCommand(new SenderQueueCommand(compositeRoot));
        }
    }

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
            foreach (var queueInfo in queueInfos)
            {
                OutputInformation("{0}\t{1}", queueInfo.Item1, queueInfo.Item2);
            }

            return true;
        }
    }
}