echo NOTE: This batch file must be run with elevated privileges.
echo Regedit will prompt four times for permission to merge the code generator and file extension registration.
echo off

C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm.exe ..\..\bin\windows\monogame.tools.visualstudio.mgfxgenerator.dll

regedit MgfxGenerator32.reg

regedit FxFileExtension32.reg

echo Restart Visual Studio to use the tool.

