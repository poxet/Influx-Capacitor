###
# This scripts takes pushes nuget packages to nuget.org
# - push to nuget.org
# - tag a git repo
#
# This script is built for pusling "non prerelease" nuget packages to nuget.org and when doing so, setting a VCS label.
#
###

Param
(
    [parameter(Mandatory=$true)]
    [alias("p")]
    $package,
    [parameter(Mandatory=$true)]
    [alias("k")]
    $apiKey,
	[parameter(Mandatory=$false)]
    [alias("l")]
    $label
)
	
Try
{
	#C:\TeamCity\buildAgent\work\8056a26431335114

	$filename = $package.Substring($package.LastIndexOf("\")+1)
	$arr = $filename.Split('.');
	$len = $arr.length;
	$version = $arr[$len-4] + '.' + $arr[$len-3] + '.' + $arr[$len-2];
	$p = $version.IndexOf("-")
	if ($p -eq -1) {} else {
		$prerelease = $version.Substring($version.IndexOf("-")+1)
	}
	$packageName = $arr[0..($len-5)] -join '.'
	
	
	write-host ("version '" + $version + "'.") -f "green"
	write-host ("prerelease '" + $prerelease + "'.") -f "green"
	write-host ("packageName '" + $packageName + "'.") -f "green"
	
	#Push to nuget
	if (-NOT $prerelease)
	{
		if ([System.Convert]::ToBoolean($label)) 
		{
			#Tag git repository
			write-host ("Creating git tag '" + $version + "'.") -f "green"
			git tag $version
			
			write-host ("Pushing tag '" + $version + "' to origin.") -f "green"
			git push origin $version
		}
		
		write-host ("Pushing chocolatey package '" + $filename + "'.") -f "green"
		choco push $package -ApiKey $apiKey -Source https://chocolatey.org
	}
	else 
	{
		write-host ("Will not push chocolatey package '" + $filename + "', it is a prerelease.") -f "yellow"
	}

	exit 0
}
Catch
{
	write-host ($error[0]) -f "red"
	exit 1
}