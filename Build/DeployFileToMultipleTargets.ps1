###
# This scripts takes a single file from one source and deploys it to one or more targets.
# If there is a  file with the same name in the target, it is overwritten.
#
# This script is built for copying a single nuget package from the build server to several web servers in a farm.
#
###

Param
(
    [parameter(Mandatory=$true)]
    [alias("s")]
    $ShareNames,
    [parameter(Mandatory=$false)]
    [alias("U")]
    $UserName,
    [parameter(Mandatory=$false)]
    [alias("P")]
    $password,
    [parameter(Mandatory=$true)]
    [alias("f")]
    $fromFile,
    [parameter(Mandatory=$true)]
    [alias("t")]
    $target
)
	
Try
{
	$shares = $ShareNames -split ";"

	foreach($ShareName in $shares)
	{
		write-host ('Connecting to share ''' + $ShareName + '''.') -f "green"
		
		if([string]::IsNullOrEmpty($UserName))
		{
			$result = net use $ShareName 2>&1 | Out-String
		}
		else
		{
			$result = net use $ShareName /u:$UserName $password 2>&1 | Out-String
		}

		if ($result -match 'System error')
		{
			write-host ($result) -f red
			exit 1
		}

		write-host ('Deploying file from ''' + $fromFile + ''' to ''' + $ShareName + '\' + $target + '\''.') -f "green"
		$result = xcopy $fromFile $ShareName\$target\ /Y 2>&1 | Out-String
		if ($result -match 'System error')
		{
			write-host ($result) -f red
			exit 1
		}

		write-host ('Disconnecting from share ''' + $ShareName + '''.') -f "green"
		$result = net use /DELETE $ShareName 2>&1 | Out-String
		if ($result -match 'System error')
		{
			write-host ($result) -f red
			exit 1
		}
	}

	exit 0
}
Catch
{
	write-host ($error[0]) -f "red"
	exit 1
}