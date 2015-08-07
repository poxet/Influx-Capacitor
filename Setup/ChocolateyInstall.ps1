$packageName = 'windirstat'
$fileType = 'msi'
$url = 'http://Influx-Capacitor.com/resources/Influx-Capacitor.###VERSION###.msi'
$silentArgs = ''

Install-ChocolateyPackage $packageName $fileType "$silentArgs" "$url"