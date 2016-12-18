using System.Net;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class AgentSendResponse : IAgentSendResponse
    {
        public AgentSendResponse(bool isSuccess, HttpStatusCode httpStatusCode, string body)
        {
            IsSuccess = isSuccess;
            StatusCode = (int)httpStatusCode;
            StatusName = httpStatusCode.ToString();
            Body = body;
        }

        public bool IsSuccess { get; }
        public int StatusCode { get; }
        public string StatusName { get; }
        public string Body { get; }
    }
}