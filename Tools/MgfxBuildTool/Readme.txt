MGFX VISUAL STUDIO HELPER CUSTOM TOOL
2012/05/31 - DAVID LIVELY - DAVIDLIVELY@GMAIL.COM

This is a Custom Tool that can be used within Visual Studio 2010 to convert XNA/DirectX .fx shaders
into MonoGame MGFX files. 

REQUIREMENTS:

- MonoGame develop3d branch. If you're reading this, you should already have that installed.

- VISUAL STUDIO 2010 SP1 SDK
This tool relies on assemblies from the SDK and the Microsoft license is unclear on whether or not these items can be redistributed.

Microsoft.VisualStudio.OLE.Interop
Microsoft.VisualStudio.Shell.Interop
Microsoft.VisualStudio.Shell.Interop.10.0
Microsoft.VisualStudio.Shell.Interop.8.0
Microsoft.VisualStudio.TextTemplating.VSHost.10.0

It may be possible to remove these dependencies in the future. 

INSTALLATION:

1. BUILD THE PROJECT.

a. Run Visual Studio 2010 with elevated priveleges. This is necessary in order to 
register the tool after compilation.

b. Open the MgfxVisualStudioHelper.sln solution.

c. Ensure that the reference to the "2MGFX" assembly is correct 
in MgfxVsHelper/References. Note that the project references the 2MGFX.EXE executable, not a DLL.

d. Ensure that the references to the Microsoft.VisualStudio.* assemblies are correct. These paths 
may be slightly different for 32- and 64-bit machines.

e. Rebuild the solution. If you encounter errors related to the source line

                var result = TwoMGFX.Program.Main(new[] { inputFileName, tempOutputFile });

make sure that you are using the updated TwoMGFX.Program.cs file which makes the program class 
and Main method public.


f. RUN THE "TESTER" CONSOLE APPLICATION included in the solution. This attempts to instantiate the 
tool via COM (as Visual Studio does), and builds a simple XNA FX file.

g. Register the tool with Visual Studio.

In addition to registering the COM object, the DLL must be registered with Visual Studio as a code 
generator. The MgfxVSHelper project includes two .reg files. 

Open the project folder in Windows Explorer. Right-click on the appropriate registry file 
(MgfxVSHelper32.reg for 32-bit Windows, MgfxVSHelper64.reg for 64-bit Windows) and select "Merge." 
Answer "Yes" to the two dialog boxes that follow.

h. Restart Visual Studio.


USAGE:

1. Add a .fx file to your game project in Visual Studio

2. Right-click the .fx file in the Solution Explorer, and select "Properties"

3. Set the "Custom Tool" field to "MgfxVSHelper," without the quotes

4. Right-click on the .fx file in the solution explorer and select "Run Custom Tool." If installation has completed 
successfully, a ".mgfx" file should appear in the solution explorer under the ".fx" file

4. Open the .fx file in the editor, and introduce an error, such as inserting a random "ASDFASDFASD" anywhere in the file

5. Save the file. The Visual Studio error pane should appear. Double-click any of the errors to be taken to the relevant
line in the source

You may wish to set the "Build Action" for the generated .MGFX file to "Content," and set its 
"Copy To Output Directory" value to "Always" so that it will be available to your game at run-time.

I welcome questions and comments, which can be sent to my gmail address (davidlively).