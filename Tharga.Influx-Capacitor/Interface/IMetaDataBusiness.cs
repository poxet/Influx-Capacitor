using System.Threading.Tasks;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface IMetaDataBusiness
    {
        Task<InfluxDbApiResponse> TestWriteAccess(IInfluxDbAgent client, string action);
        Point BuildQueueMetadata(string action, ISendResponse sendResponse, ISenderAgent senderAgent, IQueueCountInfo queueCountInfo);
    }
}