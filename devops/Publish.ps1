param ([string] $PackageId)

. .\devops\BuildFunctions.ps1

if (-Not (Test-Should-Deploy)) {
	return
}

$nextVersion = Get-Next-Version-String $PackageId

if (Test-Package-Already-Published $PackageId $nextVersion) {
	return
}

$versionToUnlist = ""

if (-Not (Test-Version-Stable-Release $nextVersion)) {
	$publishedVersion = Get-Published-PreRelase-Package $PackageId
	if (-Not (Test-Version-Stable-Release $publishedVersion)) {
		$versionToUnlist = $publishedVersion
	}
}

& nuget push $PackageId.nupkg -Source https://www.nuget.org/api/v2/package
# Unlist previous Pre-Release packages.
if ($versionToUnlist -ne "") {
	& $nuget delete $versionToUnlist -Source https://www.nuget.org/api/v2/package
}