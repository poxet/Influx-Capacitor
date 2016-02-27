using System;
using log4net;

namespace Tharga.Influx_Capacitor
{
    public class Logger
    {
        private readonly ILog _log;

        public Logger()
        {
            _log = LogManager.GetLogger(typeof(Logger));
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

        public void Info(object msg)
        {
            _log.Info(msg);
        }
    }
}