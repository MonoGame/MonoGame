function Get-Current-Branch-Name ()
{
	$branch = ([string](git rev-parse --abbrev-ref HEAD)).Trim()
	$parts = $branch.Split('/')
	$branch = $parts[$parts.Length - 1]
	return $branch
}

function Get-Current-Commit-Hash ()
{
	return ([string](git log -1 --pretty=%h)).Trim()
}

function Get-Current-Commit-Message ()
{
	return ([string](git log -1 --pretty=%B)).Trim()
}

function Test-Should-Deploy ()
{
	$nugetGitVersion   = Get-Git-Package-Version
	$buildMetaData = Get-Git-Build-MetaData
	$fullSemVer = Get-Git-Full-Sem-Ver

	if (Test-Tag-Build $nugetGitVersion $buildMetaData $fullSemVer) {
		return  $true
	}
	return $false
}

function Get-Git-Package-Version ()
{
	Update-Ensure-Git-Not-Detached 
	return [string](gitversion /showvariable MajorMinorPatch)
}

function Get-Git-Build-MetaData ()
{
	Update-Ensure-Git-Not-Detached 
	return [string](gitversion /showvariable BuildMetaData)
}

function Get-Git-Full-Sem-Ver () 
{
	Update-Ensure-Git-Not-Detached

	return [string](gitversion /showvariable FullSemVer)
}

function Update-NuSpec-Release-Notes($File, $releaseNotes) 
{
		$File = Resolve-Path $File
	
		[xml] $fileContents = Get-Content -Encoding UTF8 -Path $File
	
		$releaseNotesPath = "package.metadata.releaseNotes"
	
		if ($null -ne $releaseNotes -and $releaseNotes -ne "") {
			Set-XmlElementsTextValue -XmlDocument $fileContents -ElementPath $releaseNotesPath -TextValue $releaseNotes
		}
		$fileContents.Save($File)
}

function Update-NuSpec-Version($File, $Version)
{
	$File = Resolve-Path $File

	[xml] $fileContents = Get-Content -Encoding UTF8 -Path $File

	$versionPath = "package.metadata.version"

	if ($null -ne $Version -and $Version -ne "") {
		Set-XmlElementsTextValue -XmlDocument $fileContents -ElementPath $versionPath -TextValue $Version
	}

	$fileContents.Save($File)
}

function Get-XmlNamespaceManager([xml]$XmlDocument, [string]$NamespaceURI = "")
{
    # If a Namespace URI was not given, use the Xml document's default namespace.
	if ([string]::IsNullOrEmpty($NamespaceURI)) { $NamespaceURI = $XmlDocument.DocumentElement.NamespaceURI }	
	
	# In order for SelectSingleNode() to actually work, we need to use the fully qualified node path along with an Xml Namespace Manager, so set them up.
	[System.Xml.XmlNamespaceManager]$xmlNsManager = New-Object System.Xml.XmlNamespaceManager($XmlDocument.NameTable)
	$xmlNsManager.AddNamespace("ns", $NamespaceURI)
    return ,$xmlNsManager		# Need to put the comma before the variable name so that PowerShell doesn't convert it into an Object[].
}

function Get-FullyQualifiedXmlNodePath([string]$NodePath, [string]$NodeSeparatorCharacter = '.')
{
    return "/ns:$($NodePath.Replace($($NodeSeparatorCharacter), '/ns:'))"
}

function Get-XmlNode([xml]$XmlDocument, [string]$NodePath, [string]$NamespaceURI = "", [string]$NodeSeparatorCharacter = '.')
{
	$xmlNsManager = Get-XmlNamespaceManager -XmlDocument $XmlDocument -NamespaceURI $NamespaceURI
	[string]$fullyQualifiedNodePath = Get-FullyQualifiedXmlNodePath -NodePath $NodePath -NodeSeparatorCharacter $NodeSeparatorCharacter
	
	# Try and get the node, then return it. Returns $null if the node was not found.
	$node = $XmlDocument.SelectSingleNode($fullyQualifiedNodePath, $xmlNsManager)
	return $node
}

function Set-XmlElementsTextValue([xml]$XmlDocument, [string]$ElementPath, [string]$TextValue, [string]$NamespaceURI = "", [string]$NodeSeparatorCharacter = '.')
{
	# Try and get the node.	
	$node = Get-XmlNode -XmlDocument $XmlDocument -NodePath $ElementPath -NamespaceURI $NamespaceURI -NodeSeparatorCharacter $NodeSeparatorCharacter
	
	# If the node already exists, update its value.
	if ($node)
	{ 
		$node.InnerText = $TextValue
	}
	# Else the node doesn't exist yet, so create it with the given value.
	else
	{
		# Create the new element with the given value.
		$elementName = $ElementPath.Substring($ElementPath.LastIndexOf($NodeSeparatorCharacter) + 1)
 		$element = $XmlDocument.CreateElement($elementName, $XmlDocument.DocumentElement.NamespaceURI)		
		$textNode = $XmlDocument.CreateTextNode($TextValue)
		$element.AppendChild($textNode) > $null
		
		# Try and get the parent node.
		$parentNodePath = $ElementPath.Substring(0, $ElementPath.LastIndexOf($NodeSeparatorCharacter))
		$parentNode = Get-XmlNode -XmlDocument $XmlDocument -NodePath $parentNodePath -NamespaceURI $NamespaceURI -NodeSeparatorCharacter $NodeSeparatorCharacter
		
		if ($parentNode)
		{
			$parentNode.AppendChild($element) > $null
		}
		else
		{
			throw "$parentNodePath does not exist in the xml."
		}
	}
}

function Test-Tag-Build ($nugetGitVersion, $buildMetaData, $fullSemVer) {
	if ([string]::IsNullOrEmpty($buildMetaData) -and $fullSemVer -eq $nugetGitVersion) {
		return $true
	}
	return $false
}

function Test-Stable-Release ($nugetGitVersion, $buildMetaData, $fullSemVer)
{
	if (Test-Tag-Build $nugetGitVersion $buildMetaData $fullSemVer) {
		return $true
	}
	return $false
}

function Get-Prefix-Name ()
{
	$branchName = Get-Current-Branch-Name

	if ($branchName -ne "master") {
		return $branchName
	}
	return "beta"
}

function Get-Next-Version-String ($PackageId)
{
	$nugetGitVersion   = Get-Git-Package-Version
	$buildMetaData = Get-Git-Build-MetaData
	$fullSemVer = Get-Git-Full-Sem-Ver

	$prefix = Get-Prefix-Name
	$prefix = $prefix.Replace("-", "")
	$prefix = $prefix.Replace("_", "")
	$prefix = $prefix.Replace(".", "")
	$nextVersion = ""
	
	$stable = Test-Stable-Release $nugetGitVersion $buildMetaData $fullSemVer
	if ($stable){
		$nextVersion = $nugetGitVersion
	} else {
		$nextVersion = "$($nugetGitVersion)-$($prefix)-build$($buildMetaData)" 
	}
	return $nextVersion
}

function Test-Git-Detached ()
{
	$output = [string](git branch | head -1)
	$detached = $output.Contains("detached")
	return $detached
}

function Get-Git-Current-Tag
{
	return [string](git describe --tags | head -1)
}

# GitVersion only works in attached branches, if we try to build a tag from the commit hash gitversion will fail
# so we ensure we are always attached to a branch
function Update-Ensure-Git-Not-Detached ()
{
	$detached = Test-Git-Detached
	if (!$detached) {
		return
	}
	$currentTag = Get-Git-Current-Tag
	$result = [string](& git checkout -B $currentTag)
	# We are using -B because maybe the branch alrady exist
}