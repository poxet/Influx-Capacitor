$packageName = 'windirstat'
$fileType = 'msi'
$url = 'http://influx-capacitor.com/Resources/Production/Influx-Capacitor.#Version#.msi'
$silentArgs = ''

Install-ChocolateyPackage $packageName $fileType "$silentArgs" "$url"