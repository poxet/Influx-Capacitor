###
#
###

Param
(
	[parameter(Mandatory=$true)]
    [alias("u")]
    $url,
	[parameter(Mandatory=$true)]
    [alias("f")]
    $msiFile,
	[parameter(Mandatory=$true)]
    [alias("n")]
    $nuspecFile
)

Try
{
	$filename = $msiFile.Substring($msiFile.LastIndexOf("\")+1)
	$arr = $filename.Split('.');
	$len = $arr.length;
	$version = $arr[$len-4] + '.' + $arr[$len-3] + '.' + $arr[$len-2];
	$p = $version.IndexOf("-")
	if ($p -eq -1) {} else {
		$prerelease = $version.Substring($version.IndexOf("-")+1)
	}
	$packageName = $arr[0..($len-5)] -join '.'

	$checksum = checksum -t md5 -f $msiFile	| Out-String
	$checksum = $checksum.Trim()

	$installScriptFile = "Chocolatey\tools\ChocolateyInstall.ps1"
	
	$urlS = "url        = '" + $url + "'"
	$pattern = "url        = '(.*)'"
	(Get-Content $installScriptFile) `
		-replace $pattern, $urlS ` |
		Out-File $installScriptFile
	
	$checksumS = "checksum      = '" + $checksum + "'"
	$pattern = "checksum      = '(.*)'"
	(Get-Content $installScriptFile) `
		-replace $pattern, $checksumS ` |
		Out-File $installScriptFile

	#$urlS = "url64      = '" + $url + "'"
	#$pattern = "url64      = '(.*)'"
	#(Get-Content $installScriptFile) `
	#	-replace $pattern, $urlS ` |
	#	Out-File $installScriptFile
    #
	#$checksumS = "checksum64    = '" + $checksum + "'"
	#$pattern = "checksum64    = '(.*)'"
	#(Get-Content $installScriptFile) `
	#	-replace $pattern, $checksumS ` |
	#	Out-File $installScriptFile
		
	choco pack $nuspecFile --version $version

	exit 0
}
Catch
{
	write-host ($error[0]) -f "red"
	exit 1
}