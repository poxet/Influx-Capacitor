# influx-capacitor
Influx-capacitor collects metrics from windows machines using Performance Counters. Data is sent to influxDB to be viewable by grafana.

It is installed as a windows service named *Influx-Capacitor*. There is also a console application called *Influx-Capacitor.Console* that can be used for configuration, to check status and run commands manually.

Data is collected and sent to a [InfluxDB](https://github.com/influxdb/influxdb) database of your choice. [Grafana](https://github.com/grafana/grafana) is a great way of looking at the data.

Install using chocolatey
https://chocolatey.org/packages/Influx-Capacitor

## Performance Counters

By default configurations for a few Performance Counters are provided. Setup for counters are stored in xml-files in the same folder as the executables, or in the ProgramData folder (IE. C:\ProgramData\Thargelion\Influx-Capacitor) and are named with the file extension xml.

You can configure any Performance Counter available to be monitored. When you have added or changed a configuration file you need to restart the service for it to take effect. You can test and run manually using the console application.

```
<Influx-Capacitor>
  <CounterGroups>
    <CounterGroup Name="[YourCounterGroupName]" SecondsInterval="[SecondsInterval]">
      <Counter>
        <CategoryName>[CategoryName]</CategoryName>
        <CounterName>[CounterName]</CounterName>
        <InstanceName>[InstanceName]</InstanceName>
      </Counter>
    </CounterGroup>
  </CounterGroups>
</Influx-Capacitor>
```

- YourCounterGroupName - You own common name for the group of counters (This will be the table name in InfluxDB)
- SecondsInterval - The interval in seconds that the group of counters will be collected.
- CategoryName - Category name of the performance counter (Ex. Processor)
- CounterName - Name of the counter (Ex. % Processor Time). Wild cards such as * and ? can be used heres. Using * will use all counters.
- InstanceName - Name of the instance (Ex. _Total). Wild cards such as * and ? can be used heres. For counters that does not have any instances, this element can be left out or left empty. Using * will give all instances. The instances are refreshed on every read so that new instances are added and obsolete ones removed.

If you want to get the name of the counters right, simply open *perfmon* and find the counter that you want there. The names to put in the config files are exactly the same as the ones in *perfmon*.

## Database connection settings
The settings are typically stored in the file database.xml located in hte ProgramData folder (IE. C:\ProgramData\Thargelion\Influx-Capacitor). The settings can be located in any other xml configuration file, but then you will not be able to manage the settings using the management console.
You can change settings directly in the file and restert the service, or you can use the command "setup change" in the console application, and the service will be restarted for you.
It is also possible to have multiple database targets. Add another *Database* element in the config file and restart the service. When using multiple targets the console application cannot be used to change the confguration.

## Tags
You can add constant tags on a global level or at counter group level. This can be done in any of the configuration files. The name of the tag has to be unique.

Global tags that will be added to all points sent to the database can be added like this.
```
<Influx-Capacitor>
  <Tag>
	<Name>[TagName]</Name>
	<Value>[TagValue]</Value>
  </Tag>
</Influx-Capacitor>
```

It is also possible to add a tag for a specific counter group, these tags can be added like this.
```
<Influx-Capacitor>
  <CounterGroups>
    <CounterGroup Name="[YourCounterGroupName]" SecondsInterval="[SecondsInterval]">
      <Counter>
        <CategoryName>[CategoryName]</CategoryName>
        <CounterName>[CounterName]</CounterName>
        <InstanceName>[InstanceName]</InstanceName>
      </Counter>
	  <GroupTag>
		<Name>[TagName]</Name>
		<Value>[TagValue]</Value>
	  </GroupTag>
    </CounterGroup>
  </CounterGroups>
</Influx-Capacitor>
```


## Running the console application
The console version is named *Tharga.Influx-Capacitor.Console.exe* and provided together with the installation. The program can be started with command parameters, or you can type the commands you want in the program.

### Config
- List - Lists all available configuration files.
- Auto - Will check if the connection works, if it does not then the user will be queried parameters needed for the setup. This command will start (or restart) the service if installed on the machine.
- Change - This is the command to use if you want to change configuration settings. This command will start (or restart) the service if installed on the machine.
- Show - Will show and test the connection. You will also get the version number of the influxDB database connected.
- Database - Change what database is used but keep the same server.

### Service
- Status - Shows the status of the windows service.
- Stop - Stops the windows service if it is running.
- Start - Starts the windows service.
- Restart - Restarts (or starts) the windows service.

### Counter
- List - Lists all performance counter configurations that exists.
- Read - Reads the value from the performance counter to be displayed on screen.
- Collect - Reads the value from the performance counter once and send it to the database.
- Initiate - Initiates some default counters (If the files and counter group does not already exist).
- Create - Create a new performance counter config file for a counter.

## Versions
The currently supported version of InfluxDB is 0.9x.

## Thanks to
- [zeugfr(https://github.com/zeugfr)]
- [discoduck2x](https://github.com/discoduck2x)
- [ziyasal/InfluxDB.Net](https://github.com/ziyasal/InfluxDB.Net)