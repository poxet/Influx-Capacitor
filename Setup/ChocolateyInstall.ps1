$packageName = 'windirstat'
$fileType = 'msi'
$url = 'http://thargelion.net/resources/Influx-Capacitor/Influx-Capacitor.1.0.6.2.msi'
$silentArgs = ''

Install-ChocolateyPackage $packageName $fileType "$silentArgs" "$url"