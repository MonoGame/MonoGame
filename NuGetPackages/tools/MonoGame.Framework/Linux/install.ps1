param($installPath, $toolsPath, $package, $project)
$file1 = $project.ProjectItems.Item("SDL_mixer.dll")
$file2 = $project.ProjectItems.Item("SDL.dll")
$file3 = $project.ProjectItems.Item("OpenTK.dll.config")
$file4 = $project.ProjectItems.Item("Tao.Sdl.dll.config")

// set 'Copy To Output Directory' to 'Copy if newer'
$copyToOutput1 = $file1.Properties.Item("CopyToOutputDirectory")
$copyToOutput1.Value = 2

$copyToOutput2 = $file2.Properties.Item("CopyToOutputDirectory")
$copyToOutput2.Value = 2

$copyToOutput3 = $file3.Properties.Item("CopyToOutputDirectory")
$copyToOutput3.Value = 2

$copyToOutput4 = $file4.Properties.Item("CopyToOutputDirectory")
$copyToOutput4.Value = 2