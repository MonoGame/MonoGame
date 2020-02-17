param ([string] $PackageId)

. .\devops\BuildFunctions.ps1

$hash = Get-Current-Commit-Hash
$releaseNotes = "Release: $($hash)"
$NuSpecFile = $PackageId + '.nuspec'

Update-NuSpec-Release-Notes $NuSpecFile $releaseNotes

& nuget pack $NuSpecFile -NoPackageAnalysis