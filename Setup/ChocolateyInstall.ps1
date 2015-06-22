$packageName = 'windirstat'
$fileType = 'msi'
$url = 'http://thargelion.net/resources/InfluxDB.Net.Collector.Service.msi'
$silentArgs = ''

Install-ChocolateyPackage $packageName $fileType "$silentArgs" "$url"