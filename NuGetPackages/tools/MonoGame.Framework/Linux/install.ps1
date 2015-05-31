param($installPath, $toolsPath, $package, $project)
$file1 = $project.ProjectItems.Item("OpenTK.dll.config")

// set 'Copy To Output Directory' to 'Copy if newer'
$copyToOutput1 = $file1.Properties.Item("CopyToOutputDirectory")
$copyToOutput1.Value = 2
