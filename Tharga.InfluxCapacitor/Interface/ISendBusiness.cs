//using System;
//using System.Collections.Generic;
//using InfluxDB.Net.Models;
//using Tharga.InfluxCapacitor.Collector.Event;

//namespace Tharga.InfluxCapacitor.Collector.Interface
//{
//    public interface ISendBusiness
//    {
//        void Enqueue(Point[] points);
//        event EventHandler<SendBusinessEventArgs> SendBusinessEvent;
//        IEnumerable<Tuple<string, int>> GetQueueInfo();
//    }
//}