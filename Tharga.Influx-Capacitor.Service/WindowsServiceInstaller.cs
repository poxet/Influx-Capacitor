using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using Tharga.InfluxCapacitor.Collector;

namespace Tharga.InfluxCapacitor.Service
{
    [RunInstaller(true)]
    public class WindowsServiceInstaller : Installer
    {
        public WindowsServiceInstaller()
        {
            var serviceProcessInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            //# Service Account Information
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            //# Service Information
            serviceInstaller.DisplayName = Constants.ServiceName;
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            //# This must be identical to the WindowsService.ServiceBase name
            //# set in the constructor of WindowsService.cs
            serviceInstaller.ServiceName = Constants.ServiceName;
            serviceInstaller.Description = "Influx Capacitor collects metrics from windows machines using Performance Counters. Data is sent to influxDB to be viewable by grafana.";

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}