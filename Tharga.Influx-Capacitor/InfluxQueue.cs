using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Timers;
using InfluxDB.Net.Contracts;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;
using Tharga.Influx_Capacitor.Agents;
using Tharga.Influx_Capacitor.Interface;
using Timer = System.Timers.Timer;

namespace Tharga.Influx_Capacitor
{
    public class InfluxQueue
    {
        private const string MutexId = "InfluxQueue";
        private static readonly IInfluxDbAgent _agent;
        private static readonly IFormatter _formatter;
        private static readonly Queue<Point[]> _queue = new Queue<Point[]>();
        private static readonly MyLogger _logger = new MyLogger();
        private static Timer _sendTimer;
        private static MutexSecurity _securitySettings;
        private static bool? _enabled;

        static InfluxQueue()
        {
            try
            {
                if (Enabled)
                {
                    var influxVersion = InfluxVersion.Auto; //TODO: Move to settings
                    _logger.Info(string.Format("Initiating influxdb agent to address {0} database {1} user {2} version {3}.",Address, DatabaseName, UserName, influxVersion));
                    _agent = new InfluxDbAgent(Address, DatabaseName, UserName, Password, null, influxVersion);
                    _formatter = _agent.GetAgentInfo().Item1;
                }
            }
            catch(Exception exception)
            {
                _logger.Error(exception);
                _enabled = false;
            }
        }

        private static string Address
        {
            get
            {
                var influxDbAddress = ConfigurationManager.AppSettings["InfluxDbAddress"];
                if (influxDbAddress == null) throw new ConfigurationErrorsException("No InfluxDbAddress configured.");
                return influxDbAddress;
            }
        }

        private static string DatabaseName
        {
            get
            {
                var databaseName = ConfigurationManager.AppSettings["InfluxDbName"];
                if (databaseName == null) throw new ConfigurationErrorsException("No InfluxDbName configured.");
                return databaseName;
            }
        }

        private static string UserName
        {
            get
            {
                var influxDbUserName = ConfigurationManager.AppSettings["InfluxDbUserName"];
                if (influxDbUserName == null) throw new ConfigurationErrorsException("No InfluxDbUserName configured.");
                return influxDbUserName;
            }
        }

        private static string Password
        {
            get
            {
                var influxDbPassword = ConfigurationManager.AppSettings["InfluxDbPassword"];
                if (influxDbPassword == null) throw new ConfigurationErrorsException("No InfluxDbPassword configured.");
                return influxDbPassword;
            }
        }

        private static bool Enabled
        {
            get
            {
                if (_enabled == null)
                {
                    var enabledString = ConfigurationManager.AppSettings["InfluxDbEnabled"];

                    bool enabled;
                    if (!bool.TryParse(enabledString, out enabled))
                        enabled = true;

                    _enabled = enabled;
                }

                return _enabled ?? true;
            }
        }

        private static async void SendTimerElapsed(object sender, ElapsedEventArgs e)
        {
            //_logger.Debug("SendTimerElapsed.");

            var pts = new List<Point>();
            InfluxDbApiResponse result = null;
            bool createdNew;
            using (var mutex = new Mutex(false, MutexId, out createdNew, _securitySettings))
            {
                mutex.WaitOne();
                while (_queue.Count > 0)
                {
                    var points = _queue.Dequeue();
                    pts.AddRange(points);
                }
                mutex.ReleaseMutex();
            }

            if (pts.Count == 0)
            {
                //_logger.Debug("Nothing to send.");
                return;
            }

            try
            {
                _logger.Debug(string.Format("Sending {0} measurements.", pts.Count + 1));
                var data = new StringBuilder();
                foreach (var item in pts)
                {
                    data.AppendLine(_formatter.PointToString(item));
                }
                _logger.Debug(data.ToString());

                result = await _agent.WriteAsync(pts.ToArray());
                _logger.Info(result.StatusCode + ": " + result.Body);
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
                //TODO: Only re-enqueue points in certain situations
                //_queue.Enqueue(pts.ToArray());
            }
        }

        public static void Enqueue(Point point)
        {
            if (!Enabled)
            {
                _logger.Debug("Ignoreing enqueue becuase the queue is disabled.");
                return;
            }

            bool createdNew;
            using (var mutex = new Mutex(false, MutexId, out createdNew, _securitySettings))
            {
                try
                {
                    mutex.WaitOne();

                    _logger.Debug(string.Format("Enqueue measurement. There will be {0} items in the queue.", _queue.Count + 1));
                    _queue.Enqueue(new[] { point });

                    if (_sendTimer != null)
                        return;

                    if (_sendTimer == null)
                    {
                        _sendTimer = new Timer();
                        _sendTimer.Interval = 10000; //TODO: Move to settings
                        _sendTimer.Elapsed += SendTimerElapsed;
                        _sendTimer.Start();
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}