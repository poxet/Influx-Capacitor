using System;
using log4net;

namespace Tharga.Influx_Capacitor
{
    internal class MyLogger
    {
        private readonly ILog _log;

        public MyLogger()
        {
            log4net.Config.XmlConfigurator.Configure();
            _log = LogManager.GetLogger(typeof(MyLogger));
        }

        public void Error(object msg)
        {
            _log.Error(msg);
        }

        public void Error(object msg, Exception ex)
        {
            _log.Error(msg, ex);
        }

        public void Error(Exception ex)
        {
            _log.Error(ex.Message, ex);
        }

        public void Warn(object msg)
        {
            _log.Warn(msg);
        }

        public void Info(object msg)
        {
            _log.Info(msg);
        }

        public void Debug(object msg)
        {
            _log.Debug(msg);
        }
    }
}