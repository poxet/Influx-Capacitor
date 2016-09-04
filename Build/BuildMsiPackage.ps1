###
#
###

Param(
	[parameter(Mandatory=$true)]
    [alias("b")]
    $build,
	[parameter(Mandatory=$true)]
    [alias("v")]
    $version,
	[parameter(Mandatory=$true)]
    [alias("f")]
    $wsxFile
)

Try
{
	$id = [guid]::NewGuid()
	#$path = $wsxFile.Substring(0, $wsxFile.LastIndexOf("\"))
	$name = $wsxFile.Substring($wsxFile.LastIndexOf("\")+1)
	$name = $name.Substring(0,$name.LastIndexOf("."))
		
	if ($version.LastIndexOf('-') -eq -1) { } else
	{
		$prerelease = $version.Substring($version.LastIndexOf('-')+1)
		$version = $version.Substring(0,$version.LastIndexOf('-'))
	}
		
	#write-host ("name:" + $name) -f green
	#write-host ("version:" + $version) -f green
	#write-host ("prerelease:" + $prerelease) -f green
		
	Push-Location $path
	
	write-host ("Running candle to create wixobj-file.") -f green
	& "C:\Program Files (x86)\WiX Toolset v3.10\bin\candle.exe" "$wsxFile" -dEnvironment="$build" -dVersion="$version" -dId="$id"
	
	write-host ("Running light to create the msi-file.") -f green
	& "C:\Program Files (x86)\WiX Toolset v3.10\bin\light.exe" -ext WixUIExtension -cultures:en-us "$name.wixobj"
	
	if ($prerelease)
	{
		$prerelease = '-' + $prerelease
	}
	
	Move-Item "$name.msi" "$name.$version$prerelease.msi" -Force
	
	#TODO: Push nuget to the correct location
	#TODO: Prepare the path in file ChocolateyInstall.ps1
	#TODO: Calculate checksum
	
	#write-host ("choco pack Influx-Capacitor.nuspec --version $version$prerelease") -f green
	#choco pack Influx-Capacitor.nuspec --version $version$prerelease
	
	#Pop-Location
	
	exit 0
}
Catch
{
	write-host ($error[0]) -f "red"
	exit 1
}