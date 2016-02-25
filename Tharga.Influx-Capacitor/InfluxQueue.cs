using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.AccessControl;
using System.Threading;
using System.Timers;
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
        private static readonly IInfluxDbAgent _agent;
        private static readonly Queue<Point[]> _queue = new Queue<Point[]>();
        private static Timer _sendTimer;

        private const string MutexId = "InfluxQueue";
        private static MutexSecurity _securitySettings;
        private static bool? _enabled;

        static InfluxQueue()
        {
            try
            {
                if (Enabled)
                {
                    var influxVersion = InfluxVersion.Auto; //TODO: Move to settings
                    _agent = new InfluxDbAgent(Address, DatabaseName, UserName, Password, influxVersion);
                }
            }
            catch
            {
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
                return;
            }

            try
            {
                result = await _agent.WriteAsync(pts.ToArray());
            }
            catch (Exception exception)
            {
                _queue.Enqueue(pts.ToArray());
                Logger.Error(exception);
            }
        }

        public static void Enqueue(Point point)
        {
            if (!Enabled)
                return;

            bool createdNew;
            using (var mutex = new Mutex(false, MutexId, out createdNew, _securitySettings))
            {
                try
                {
                    mutex.WaitOne();

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