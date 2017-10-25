""
"   This script generates portable 'MonoGame.Framework.dll' " 
"   from Windows version of 'MonoGame.Framework.dll'"
""
# check if source dll exists
$sourceDll = "MonoGame.Framework\bin\Windows\AnyCPU\Release\MonoGame.Framework.dll"
if (!(Test-Path $sourceDll))
{
    write-host "ERROR: '$sourceDll' not found!" -foreground "red"
    ""
    write-host "You have to build 'MonoGame.Framework.Windows' project " -foreground "red"
    write-host "in 'Release' configuration before running this script!" -foreground "red"
    ""
    Pause
    exit
}

write-host "INFO: Make sure you have build 'MonoGame.Framework.Windows' project " -foreground "yellow"
write-host "in 'Release' configuration before running this script!" -foreground "yellow"

# ensure target directory exists
$targetFolder = "MonoGame.Framework\bin\Portable\AnyCPU\"
mkdir -Force $targetFolder | Out-Null

""
while( -not ( ($choice= (Read-Host "Generate portable dll? [y/n]")) -match "y|n")){ "Y or N ?"}
if ($choice -eq "y")
{
    # call Piranha.exe
    .\ThirdParty\Dependencies\Piranha\Piranha.exe make-portable-skeleton -i MonoGame.Framework\bin\Windows\AnyCPU\Release\MonoGame.Framework.dll -o MonoGame.Framework\bin\Portable\AnyCPU\MonoGame.Framework.dll -p ".NETPortable,Version=v4.0,Profile=Profile328"
    ""
    "   End of Piranha output"
    ""
    write-host "Check '$targetFolder' folder for portable dll." -foreground "yellow"
    ""
    pause
}
