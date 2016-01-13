//using System;
//using System.Net.Http;

//namespace Tharga.InfluxCapacitor.Collector
//{
//    internal static class SenderExceptionExtension
//    {
//        public static string IsExceptionValidForPutBack(this Exception exception)
//        {
//            var exceptionToUse = exception;
//            //var exceptionToUse = GetInnerMostException(exception);
//            //else
//            //{
//            //    var agg = exceptionToUse as AggregateException;
//            //    if (agg != null)
//            //    {
//            //        exceptionToUse = agg.InnerException;
//            //    }
//            //    //return exception.GetType().ToString();
//            //}

//            //Allowed request types returns null
//            if (exceptionToUse is HttpRequestException)
//            {
//                return null;
//            }

//            return exceptionToUse.GetType().ToString();
//        }
//    }
//}