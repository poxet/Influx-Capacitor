using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Publish
{
    internal class PublishCommands : ContainerCommandBase
    {
        public PublishCommands(CompositeRoot compositeRoot)
            : base("Publish")
        {
            RegisterCommand(new PublishStartCommand(compositeRoot.ConfigBusiness, compositeRoot.CounterBusiness, compositeRoot.PublisherBusiness, compositeRoot.SendBusiness, compositeRoot.TagLoader));
        }
    }
}
