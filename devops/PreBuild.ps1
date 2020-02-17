param ([string] $PackageId)

. .\devops\BuildFunctions.ps1

$nextVersion = Get-Next-Version-String $PackageId
$NuSpecFile = $PackageId + '.nuspec'

Update-NuSpec-Version  $NuSpecFile $nextVersion
gitversion /updateassemblyinfo
