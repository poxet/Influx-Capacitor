# influx-capacitor
Influx-capacitor collects metrics from windows machines using Performance Counters. Data is sent to influxDB to be viewable by grafana.

It is installed as a windows service named *Influx-Capacitor*. There is also a console application called *Influx-Capacitor.Console* that can be used for configuration, to check status and run commands manually.

Data is collected and sent to a [InfluxDB](https://github.com/influxdb/influxdb) database of your choice. [Grafana](https://github.com/grafana/grafana) is a great way of looking at the data.

Install using chocolatey
[https://chocolatey.org/packages/Influx-Capacitor]()

Visit the main site for more information
[http://influx-capacitor.com/]()

## Performance Counters

By default configurations for a few Performance Counters are provided. Setup for counters are stored in xml-files in the same folder as the executables, or in the ProgramData folder (IE. `C:\ProgramData\Thargelion\Influx-Capacitor`) and are named with the file extension xml.

You can configure any Performance Counter available to be monitored. When you have added or changed a configuration file you need to restart the service for it to take effect. You can test and run manually using the console application.

### Configuration

```xml
<Influx-Capacitor>
  <CounterGroups>
    <CounterGroup Name="[YourCounterGroupName]" SecondsInterval="[SecondsInterval]" RefreshInstanceInterval="[RefreshInstanceInterval]" CollectorEngineType="[CollectorEngineType]">
      <Counter>
        <MachineName>[MachineName]</MachineName>
        <CategoryName>[CategoryName]</CategoryName>
        <CounterName>[CounterName]</CounterName>
        <InstanceName Alias="[Alias]">[InstanceName]</InstanceName>
        <FieldName>[FieldName]</FieldName>
        <Limits Max="[Max]" />
      </Counter>
    </CounterGroup>
  </CounterGroups>
</Influx-Capacitor>
```

- YourCounterGroupName - You own common name for the group of counters (This will be the table name in InfluxDB)
- SecondsInterval - The interval in seconds that the group of counters will be collected.
- RefreshInstanceInterval - An optional attribute for the interval used to refresh the list of instances for the counter. If set to 1, the counters are refreshed everytime. If set to 2, every other time and so on. To disable the refresh set this value to 0. The default value is 0, if the attribute is left out.
- CollectorEngineType - Specifies the strategy of collecting data. Can be set to Safe or Exact, default is Safe. The Safe collector engine collects all counters and waits the specified SecondsInterval until the next read (SecondsInterval is actually seconds in between reads). Exact setting will read counter on the exact same time each interval. (SecondsInterval is actually seconds from start to next start read) Because of this the Exact setting will have to drop reads if the previous read takes too long to perform, since it then cannot start when it is supposed to.
- MachineName - Optional element, used to specify the name of the machine where the counter should be read. If the element is left out or is empty, the current machine will be used.
- CategoryName - Category name of the performance counter (Ex. Processor)
- CounterName - Name of the counter (Ex. % Processor Time). Wild cards such as * and ? can be used heres. Using * will use all counters.
- InstanceName - Name of the instance (Ex. _Total). Wild cards such as * and ? can be used heres. Use | to provide several names that could not be specified via wild-cards. For counters that does not have any instances, this element can be left out or left empty. Using * will give all instances. The instances are refreshed on every read so that new instances are added and obsolete ones removed.
- Alias - This is an optional value that will be used as field name for the Instance specification. The value of this field will be the same as of the "instance" field.
- FieldName - This optional value will be used to name the field value instead of the default "value" one. You can use this value to merge points together. Note that when you use this option, counter tags are ignored (only countergroup tags are used).
- Max - This optional value will be used to fix the maximum value sent for this counter.

If you want to get the name of the counters right, simply open *perfmon* and find the counter that you want there. The names to put in the config files are exactly the same as the ones in *perfmon*.

### Advanced: filters

When you start using instance names, there are cases where you can't get the correct name, for example because the instance name is dynamic and uses process id.

In such cases, you can use `InstanceFilters` at the `CounterGroup` level to apply some transformations on instance names, using regular expressions.

An example of configuration with filters.

```xml
<Influx-Capacitor>
  <CounterGroups>
    <CounterGroup Name="[YourCounterGroupName]" SecondsInterval="[SecondsInterval]" RefreshInstanceInterval="[RefreshInstanceInterval]" CollectorEngineType="[CollectorEngineType]">
      <Counter>
        <MachineName>[MachineName]</MachineName>
        <CategoryName>[CategoryName]</CategoryName>
        <CounterName>[CounterName]</CounterName>
        <InstanceName Alias="[Alias]">[InstanceName]</InstanceName>
        <FieldName>[FieldName]</FieldName>
        <Limits Max="[Max]" />
      </Counter>
      ...
      <InstanceFilters>
        <Filter Pattern="[Pattern]" />
        <Filter Pattern="[ReplacementPattern]" Replacement="[Replacement]" />
      </InstanceFilters>
    </CounterGroup>
  </CounterGroups>
</Influx-Capacitor>
```

- `Pattern` - If you add a Filter element with only a Pattern attribute, then this pattern will be used as a filter to only include instances names matching this regular expression.
- `ReplacementPattern` - If you add a Filter element with both a Pattern and a Replacement attribute, this regular expression pattern will be used to replace a part of the instance name.
- `Replacement` - This expression will be used as a replacement for the pattern previously provided.

A filtering pattern will exclude counters where instance name does not match.
A replacement filter will only change the name, without excluding any instance.
All filters are executed sequentially on each instance name.

You can use the `InstanceName` element to choose counters for simple cases, then apply an advanced filter.

Here are some examples.

```xml
<InstanceFilters>
  <!-- this filter will only include counters names where ".NET" is present in the instance name -->
  <Filter Pattern="\.NET" />
  <!-- this filter will remove all pids from instance names for IIS apps. "1879_.NET v4.5" => ".NET v4.5" -->
  <Filter Pattern="^\d+_(.*)$" Replacement="$1" />
  <!-- this filter will replace ".NET v4.5" by "net45" -->
  <Filter Pattern="\.NET v4\.5" Replacement="net45" />
</InstanceFilters>
```

### Advanced: custom providers

By default, Influx-Capacitor includes one standard provider, which is able to collect counters from system performance counters, as presented above.

A additional mechanism is included to allow external contributors to add their own providers.
A provider is an assembly which references `Tharga.Influx-Capacitor.Collector` and expose a public class which implements `Tharga.InfluxCapacitor.Collector.Interface.IPerformanceCounterProvider`.
Then, you can configure this custom provider in your configuration files, in a `Provider` element.

```xml
<Influx-Capacitor>
  <Providers>
    <Provider Name="[ProviderName]" Type="[ProviderType]" />
  </Provider>
</Influx-Capacitor>
```

- ProviderName - A unique name for this provider. The same provider type can be registered multiple times with a different configuration.
  Each registration must use a different unique name.
- ProviderType - The assembly qualified type name for this provider. eg: `MyLibrary.MyCustomProvider, MyLibrary, version=1.0.0.0`.
  If the provider is an internal provider (included in `Tharga.Influx-Capacitor.Collector`), you just have to provide the type name.

Specific provider settings can be included in configuration. See each provider documentation for details about these settings.

To use your specific providers, you have to indicate the provider uniquename in `CounterGroup`.


```xml
<Influx-Capacitor>
  <CounterGroups>
    <CounterGroup Name="..." Provider="[ProviderName]">
	  ...
	</CounterGroup>
  </CounterGroups>
</Influx-Capacitor>
```

## Application configuration

There are some application settings that can be configured. The configuration can be made in any xml-config file in the programdata folder.
The default location of this configuration is *application.xml*.

- FlushSecondsInterval - The interval that data is sent to the database. This is an optional value, default is 10 seconds.
- Metadata - Metadata about Influx-Capacitor is sent to the measurement named *Influx-Capacitor*. This feature can be turned off by setting Metadata to false, by default this feature is on.
- MaxQueueSize - The largest size allowed for the queue. If Influx-Capacitor is not able to send the queued data to the server, it will not keep collecting forever. The limit can be set to an optional value, default is 20000.

```xml
<Influx-Capacitor>
  <Application>
    <FlushSecondsInterval>10</FlushSecondsInterval>
    <Metadata>true</Metadata>
    <MaxQueueSize>20000</MaxQueueSize>
  </Application>
</Influx-Capacitor>
```


## Database connection settings
The settings are typically stored in the file database.xml located in the ProgramData folder (IE. C:\ProgramData\Thargelion\Influx-Capacitor). The settings can be located in any other xml configuration file, but then you will not be able to manage the settings using the management console.
You can change settings directly in the file and restert the service, or you can use the command "setup change" in the console application, and the service will be restarted for you.
It is also possible to have multiple database targets. Add another *Database* element in the config file and restart the service. When using multiple targets the console application cannot be used to change the confguration.

There are several different types of databases supported. Each of them is configured differently. Set the type attribute in the Database element to select what database provider to use.
The attributes *Type* defaults to *InfluxDB*, and *Enabled* is default *true*.

Supported types are
- InfluxDB (default)
- Kafka

### InfluxDB

- Url - The location of the InfluxDB server
- Username - Login username
- Password - password
- Name - Name of the database
- RequestTimeoutMs - Optional value, describing the HTTP request timeout when sending data to influxDB.

```xml
<Influx-Capacitor>
  <Database Type="InfluxDB" Enabled="true">
    <Url>http://localhost:8086</Url>
    <Username>MyUser</Username>
    <Password>qwerty</Password>
    <Name>InfluxDbName</Name>
	<RequestTimeoutMs>15000</RequestTimeoutMs>
  </Database>
</Influx-Capacitor>
```

### Kafka

This is actually not a database, this type sends data to *Kafka* (http://kafka.apache.org/). The message is formatted for influxDB version 0.9.x.

- Url - Url to the Kafka server. It is possible to provide a list of servers with *;* as separator.

```xml
<Influx-Capacitor>
  <Database Type="Kafka">
	<Url>http://server1;http://server2</Url>
  </Database>
</Influx-Capacitor>
```

### Null

This type is for development only. It collects points and sends them to no where.

```xml
<Influx-Capacitor>
  <Database Type="null" />
</Influx-Capacitor>
```

### Acc

This type is for development only. It collects and accumulates points but never sends them anywhere.

```xml
<Influx-Capacitor>
  <Database Type="acc" />
</Influx-Capacitor>
```


## Tags
You can add constant tags on a global, counter group and counter level. This can be done in any of the configuration files. The name of the tag has to be unique.

Global tags that will be added to all points sent to the database can be added like this.
```xml
<Influx-Capacitor>
  <Tag>
	<Name>[TagName]</Name>
	<Value>[TagValue]</Value>
  </Tag>
</Influx-Capacitor>
```

It is also possible to add a tags for a specific counter group, these tags can be added like this.
```xml
<Influx-Capacitor>
  <CounterGroups>
    <CounterGroup Name="[YourCounterGroupName]" SecondsInterval="[SecondsInterval]">
      <Counter>
        <CategoryName>[CategoryName]</CategoryName>
        <CounterName>[CounterName]</CounterName>
        <InstanceName>[InstanceName]</InstanceName>
      </Counter>
	  <Tag>
		<Name>[TagName]</Name>
		<Value>[TagValue]</Value>
	  </Tag>
    </CounterGroup>
  </CounterGroups>
</Influx-Capacitor>
```

Tags for a specific counter is added like this.
```xml
<Influx-Capacitor>
  <CounterGroups>
    <CounterGroup Name="[YourCounterGroupName]" SecondsInterval="[SecondsInterval]">
      <Counter>
        <CategoryName>[CategoryName]</CategoryName>
        <CounterName>[CounterName]</CounterName>
        <InstanceName>[InstanceName]</InstanceName>
	    <Tag>
		  <Name>[TagName]</Name>
		  <Value>[TagValue]</Value>
	    </Tag>
      </Counter>
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
The currently supported versions of InfluxDB is from 0.9.x to 0.12.x.

## Metadata
By default metadata is sent fron Influx-Capacitor to influxDB. (There is an Application that can turn this off if you do not want it)
The data is register as measurement named *Influx-Capacitor-Metadata*.

Mainly the collecting of data and the status of the queue is what can be monitored.
Use the *counter* tag in the where statement to select the metadata you want to analyze.

####Tags that appears for all metadata measurements
- hostname - Name of the machine that collects the data
- version - Version of Influx-Capacitor
- counter - Name of the metadata counter (queueCount, readCount, readTime or configuration)
- action - Action that triggered metadata measurement

### queueCount
For measurements where *counter* = queueCount

Use this measurement to monitor the queue. How data is piled up and how it is sent to the server.
If you have more than one server you are sending data to, you can see all servers metadata on all servers.
This makes it easy to see if the queue is increasing because data cannot be sent to one of the servers.

####Tags
- action - Triggered when one of the following actions occurred (Enqueue, Send)
- targetServer - Url to the server where data is sent
- targetDatabase - Name of the database where data is sent
- failMessage - Error message when sending data. (Only messages that does not prevent metadata from beeing sent)

####Values
- value - Total number of items in queue
- queueCountChange - Number of items added or removed from the queue
- sendTimeMs - The time in milliseconds it took to send data to the server

### readCount
For measurements where *counter* = readCount

This one can be used to monitor the collection of data. Number of data and how long it takes.

####Tags
- action - always the value *collect*
- performanceCounterGroup - Name of the performace counter group
- engineName - name of the engine collecting data (SafeCollectorEngine or ExactCollectorEngine)

####Values
- value - Number of counters read
- elapseOffsetTimeMs - If the *ExactCollectorEngine* is used, the offset time from when the read was supposed to happen.
- totalTimeMs - Total time it took for all reader steps; synchronize, prepare, read, format, enque and cleanup.

### readTime
For measurements where *counter* = readTime

The collecting of data involves several steps. Here you can monitor the time it takes for each step.
- synchronize - When using the *ExactCollectorEngine*, Influx-Capacitor tries to read data at the exact same time. This value shows how long it takes to compensate for the actual time it takes to read the data.
- prepare - Shows the time it takes to prepare the counters. The counters are refreshed using *RefreshInstanceInterval*, the time it takes will show up here.
- read - This is the time it takes for the actual read of all counters. If there are many counters to read, this will take longer.
- format - The time it takes to format the data that is to be sent to influxDB.
- enque - This is the time it takes to put the data in the queue.
- cleanup - The final step times the removal of obsolete counters. An example of this could be a counter for a SqlDatabase that has been removed or a process that does no longer exist.

####Tags
- action - always the value *collect*
- performanceCounterGroup - Name of the performace counter group
- engineName - name of the engine collecting data (SafeCollectorEngine or ExactCollectorEngine)
- step - Name of the step (with order number) for the data collector (Hint. Use this in graphana for grouping and stack the values)

####Values
- value - Time in milliseconds for each step (Use the total sum to get the total time for the collector)

### configuration
For measurements where *counter* = configuration

####Tags
action - The action that performed the configuration test (config_auto, config_database, config_change)

####Values
value - The value 1

## Point format and measurements schema

By default, InfluxDB points are created with a "counter" and a "category" tag, and an unique field "value".
You have the possibility to use alias for instances, and assign specific tag to each counter.

```xml
<CounterGroup Name="perfmon.memory" SecondsInterval="5">
    <Counter>
		<CategoryName>Memory</CategoryName>
		<CounterName>Available Bytes</CounterName>
    </Counter>
    <Counter>
		<CategoryName>Memory</CategoryName>
		<CounterName>Committed Bytes</CounterName>
    </Counter>
</CounterGroup>
```
Datas send with this configuration will result in this schema in InfluxDB:

    > select * from perfmon.memory

    time              category  counter          instance  hostname     value
	20150203002300  Memory    Available Bytes  _Total    SERVER1   15766000
	20150203002300  Memory    Committed Bytes  _Total    SERVER1     460000
	20150203004300  Memory    Available Bytes  _Total    SERVER1   15966000
	20150203004300  Memory    Committed Bytes  _Total    SERVER1     440000

This schema has the advantage of being very flexible and powerfull, but has the disavantage of consuming more memory (see [official doc, when do I need more RAM](https://docs.influxdata.com/influxdb/v0.9/guides/hardware_sizing/#when-do-i-need-more-ram))
If you do not want to use counter's specific tags, or have simplier requirements, you can compact points and gain memory by using the FieldName config element:

```xml
<CounterGroup Name="perfmon.memory" SecondsInterval="5">
    <Counter>
		<CategoryName>Memory</CategoryName>
		<CounterName>Available Bytes</CounterName>
		<FieldName>free_bytes</FieldName>
    </Counter>
    <Counter>
		<CategoryName>Memory</CategoryName>
		<CounterName>Committed Bytes</CounterName>
		<FieldName>committed_bytes</FieldName>
    </Counter>
</CounterGroup>
```
Will give the following result in InfluxDB:

    > select * from perfmon.memory

    time              hostname  free_bytes  committed_bytes
	2015020311002300  SERVER1     15766000           460000
	2015020311004300  SERVER1     15966000           440000

This compact mode suffers from some limitations you have to be aware of:

* You can not use wildcards for instances or counters, since the field name must be unique
* You can not use counter tags, are they could conflict with other tags defined in other counters of the same group. Only counter group tags can be used.
* The instance alias is ignored, since the instance tag is ignored. But you can still use instance specific counter: you have to add a counter element for each of them, with a different fieldname.
* Schema is less self-descriptive, as you have to know which field name corresponds to which performance counter by yourself.
