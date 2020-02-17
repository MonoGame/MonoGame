param ([string] $PackageId)

. .\devops\BuildFunctions.ps1

if (-Not (Test-Should-Deploy)) {
	return
}

$nupkgFile = $PackageId +  '.nupkg'

& nuget push $nupkgFile -Source https://www.nuget.org/api/v2/package
