namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface ITagLoader
    {
        ITag[] GetGlobalTags();
    }
}