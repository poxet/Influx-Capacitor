using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tharga.InfluxCapacitor
{
    internal static class SenderExceptionExtension
    {
        public static string IsExceptionValidForPutBack(this Exception exception)
        {
            var exceptionToUse = exception;
            if (exceptionToUse is HttpRequestException)
            {
                return null;
            }
            else if (exceptionToUse is TaskCanceledException)
            {
                return null;
            }

            return exceptionToUse.GetType().ToString();
        }
    }
}