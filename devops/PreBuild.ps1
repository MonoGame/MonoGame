param ([string] $PackageId, [string] $NuSpecFile)

. .\devops\BuildFunctions.ps1

$nextVersion = Get-Next-Version-String $PackageId

Update-NuSpec-Version  $NuSpecFile $nextVersion
gitversion /updateassemblyinfo
