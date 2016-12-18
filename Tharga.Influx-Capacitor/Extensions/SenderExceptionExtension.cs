using System;
using System.Net.Http;

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

            return exceptionToUse.GetType().ToString();
        }
    }
}