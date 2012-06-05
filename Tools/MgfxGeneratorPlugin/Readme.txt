MGFX VISUAL STUDIO HELPER CUSTOM TOOL
(WINDOWS ONLY)
2012/05/31 - DAVID LIVELY - DAVIDLIVELY, GMAIL

This is a Custom Tool that can be used within Visual Studio 2010 to convert XNA/DirectX .fx shaders
into MonoGame MGFX files. 

I. REQUIREMENTS:

- MonoGame develop3d branch. If you're reading this, you should already have that installed.
- MonoGame-Dependencies submodule. From git bash prompt, enter "git init submodule" from your MonoGame root. The Samples, Starter Kits, and ThirdParty/Libs
modules will be installed. (This component only requires "ThirdParty/Libs.")


II. INSTALLATION:

Open the MgfxGenerator solution in Visual Studio 2010, and build it. The output will be copied to MonoGame\Tools\Bin\Windows.

Open an elevated command prompt in the (solution)\reg folder. 

If you are running 64-bit Windows, run Register64.bat, and accept the popups from RegEdit. This will register the code generator
with Visual Studio, and associate the .fx file extension with the tool. If you don't want to MgfxGenerator to be the default
custom tool for .fx files, you can manually register MonoGame.Tools.VisualStudio.MgfxGenerator.dll from the
MonoGame\Tools\bin\Windows folder, and merge the appropriate version of MgfxGenerator??.reg for your system.

Restart Visual Studio.

The "Custom Tool" property setting for an effect source file must be set to "MonoGame MGFX Generator" (without quotes). 

III. NOTES:

If properly installed, Visual Studio will run the 2MGFX tool whenever the effect source file is saved within the IDE. Errors will be reported
in the Error List panel. Note that the Error List panel will not automatically appear ( since it appears that this would require an 
assembly from the VS2010 SDK).

If the post-build xcopy generates an error, restart Visual Studio as this may be due to an attempt to overwrite a previously
installed version of the assembly.

