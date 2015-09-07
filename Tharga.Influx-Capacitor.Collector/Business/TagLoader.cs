using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class TagLoader : ITagLoader
    {
        private readonly IConfigBusiness _configBusiness;

        public TagLoader(IConfigBusiness configBusiness)
        {
            _configBusiness = configBusiness;
        }

        public ITag[] GetGlobalTags()
        {
            var result = _configBusiness.LoadFiles().Tags.ToArray();
            return result;
        }
    }
}