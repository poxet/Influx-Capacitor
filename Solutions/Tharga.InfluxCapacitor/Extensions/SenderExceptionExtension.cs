using System;
using System.Net.Http;

namespace Tharga.InfluxCapacitor.Sender
{
    internal static class SenderExceptionExtension
    {
        public static string IsExceptionValidForPutBack(this Exception exception)
        {
            var exceptionToUse = exception;
            //var exceptionToUse = GetInnerMostException(exception);
            //else
            //{
            //    var agg = exceptionToUse as AggregateException;
            //    if (agg != null)
            //    {
            //        exceptionToUse = agg.InnerException;
            //    }
            //    //return exception.GetType().ToString();
            //}

            //Allowed request types returns null
            if (exceptionToUse is HttpRequestException)
            {
                return null;
            }

            return exceptionToUse.GetType().ToString();
        }

        //private static Exception GetInnerMostException(Exception exception)
        //{
        //    var inner = exception;
        //    while (inner.InnerException != null)
        //    {
        //        inner = inner.InnerException;
        //    }

        //    return inner;
        //}
    }
}