param ([string]$version, [string]$environment)

$original_file = 'Influx-Capacitor.nuspec'
$destination_file =  '..\Influx-Capacitor.nuspec'

(Get-Content $original_file) | Foreach-Object {
    $_ -replace '#Version#', $version
    } | Set-Content $destination_file

$original_file = 'ChocolateyInstall.ps1'
$destination_file =  '..\ChocolateyInstall.ps1'

(Get-Content $original_file) | Foreach-Object {
    $_ -replace '#Version#', $version
    } | Set-Content $destination_file

(Get-Content $destination_file) | Foreach-Object {
    $_ -replace '#Environment#', $environment
    } | Set-Content $destination_file
