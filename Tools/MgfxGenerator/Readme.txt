MGFX VISUAL STUDIO HELPER CUSTOM TOOL
2012/05/31 - DAVID LIVELY - DAVIDLIVELY@GMAIL.COM

This is a Custom Tool that can be used within Visual Studio 2010 to convert XNA/DirectX .fx shaders
into MonoGame MGFX files. 

REQUIREMENTS:

- MonoGame develop3d branch. If you're reading this, you should already have that installed.
- ThirdParty/Libs submodule

Open the solution in visual studio and build it. The required assemblies will be copied to MonoGame\Tools\bin\windows.
In an elevated command prompt, switch to that folder, and run

regasm /codebase MonoGame.Tools.VisualStudio.MgfxGenerator.dll

To register the tool with visual studio, merge the MgfxGenerator\MgfxGeneratorXX.reg file into the registry. Use the 32 or 64-bit version depending on what operating system version you're using.

To automatically associate .fx extensions with this tool, merge the FxFileExtensionXX.reg file. 

Restart visual studio.

Add a new .fx file to your game project. Save it from within the IDE. A new .mgfxo file should appear under the .fx file in the solution explorer.

Open the Errors list and make sure that there are no errors. (This rev can't open the Error List automatically due to some licensing issues with the VS2010 SDK, and incomplete MSDN docs.)



