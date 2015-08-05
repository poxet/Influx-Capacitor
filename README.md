# influxdb-collector
InfluxDB collector for windows. Uses performance counters to collect data from the machine and send them to InfluxDB to be viewable by grafana.

It is installed as a windows service named *InfluxDB.Net.Collector*. There is also a console application called *InfluxDB.Net.Collector.Console* that can be used for configuration, to check status and run commands manually.

Data is collected and sent to a [InfluxDB](https://github.com/influxdb/influxdb) database of your choice. This is great for viewing with [Grafana](https://github.com/grafana/grafana).

Install using chocolatey
https://chocolatey.org/packages/influxdb.collector

## Versions
- Version 1.0.0 - 1.0.3 (InfluxDB version 0.8)
- Version 1.0.5 (InfluxDB version 0.9)

## Performance Counters

By default two performance counters are provided. Setup for counters are stored in xml-files in the same folder as the executables.
- ProcessorCounterConfiguration.xml
- MemoryCounterConfiguration.xml

It is easy to configure new counters to be monitored. Just do so by creating a new xml-file using the following format, and restart the service.

```
<InfluxDB.Net.Collector>
  <CounterGroups>
    <CounterGroup Name="[YourCounterGroupName]" SecondsInterval="[SecondsInterval]">
      <Counter>
        <CategoryName>[CategoryName]</CategoryName>
        <CounterName>[CounterName]</CounterName>
        <InstanceName>[InstanceName]</InstanceName>
      </Counter>
    </CounterGroup>
  </CounterGroups>
</InfluxDB.Net.Collector>
```

- YourCounterGroupName - You own common name for the group of counters
- SecondsInterval - The interval that the group of counters is collected and sent to *InfluxDB*
- CategoryName - Category name of the performance counter (Ex. Processor)
- CounterName - Name of the counter (Ex. % Processor Time)
- InstanceName - Name of the instance (Ex. _Total)

If you want to get the name of the counters right, simply open *perfmon* and find the counter that you want there. The names to put in the config files are exactly the same as the ones in *perfmon*.

## Database connection settings
The settings are stored in the file database.xml located in hte ProgramData-folder. Propably here "C:\ProgramData\Thargelion\InfluxDB.Net.Collector". You can change settings directly in that file.
You will have to manually restart the service when the settings have been changed.

Another way of changing settings are to run the console version of the probram.

## Running the console application
The console version is named *InfluxDB.Net.Collector.Console.exe* and provided together with the installation. The program can be started with command parameters, or you can type the commands you want in the program.

### setup
- Auto - Will check if the connection works, if it does not then the user will be queried parameters needed for the setup.
- change - This is the command to use if you want to change configuration settings.
- show - Will show and test the connection.

### processor
- run - Will run just like the service does, and collect data from the performance counters and send the result to influxDB.

