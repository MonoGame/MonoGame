Welcome to MonoGame on NuGet

_Description
MonoGame is an open source implementation of the Microsoft XNA 4.x Framework.
The goal is to make it easy for XNA developers to create cross-platform games with extremely high code reuse. We currently support iOS, Android, Windows (both OpenGL and DirectX), Mac OS X, Linux, Windows 8 Store, Windows Phone 8, PlayStation Mobile, and the OUYA console.

This package is just the Binaries for MonoGame for easier platform dll updates.  

_Supported Platforms
	* Windows GL
	* Windows 8 & 8.1
	* Windows Phone 8, 8.1 and universal projects
	* iOS
	* MacOS
	* Android

_Contents
This package contains only MonoGame DLL references for a solution.
For project templates either use the Monogame Project templates or the Main MonoGame NuGet package

*Note this package no longer contains the .Net and .GamerServices namespaces, please see the adjoining MonoGame.Net NuGet package if you require them


_Install Instructions
If you have created your project using the installed MonoGame templates, you will need to remove any MonoGame references first.
Such references include:
	* OpenTK
	* TAO.sdl
	* SDL
	* Lidgren
**Do not remove any MONO references as these are NOT included

When applied, this NuGet will install all relevant dll's for MonoGame.
**Note You may not see MonoGame or it's dependant libraries in the project references, this is Normal as we use MSBuild configuration to reference them

**Windows Phone 8.0 users
The MSBuild configuration will remove old XNA references at build time to avoid conflict.
This also applies to CLASS LIBRARIES, all projects in a Windows Phone project that are referencing MonoGame will need this package installed!

**Updating to use the new MonoGame Content Builder tool
To use the new content builder tool you will need the latest development MSI installer which can be located at http://www.monogame.net/downloads/ in the Development Builds section
This will install all the project templates for Visual Studio including the new Content Builder tool.
To upgrade your project you will need to edit your .CSPROJ file for your solution (either from windows explorer or using the VS power tools to edit the project file) and add the following entries:
	*  <MonoGamePlatform>WindowsGL</MonoGamePlatform>  <- Placed just before the 1st line with   </PropertyGroup> (in the initial project definition section)
**Note, the platform name should be one of the following: Android, iOS, Linux, MacOSX, NativeClient, Ouya, Playstation4, PlaystationMobile, RaspberryPi, Windows, WindowsGL, WindowsPhone, WindowsPhone8, WindowsStoreApp, XBOX360   (depending on your project platform)
	*  <Import Project="$(MSBuildExtensionsPath)\MonoGame\v3.0\MonoGame.Content.Builder.targets" />  <- Placed with the other IMPORT references at the end of the CSPROJ file
This will enable the new Content Builder to function an run.
To make use of the new Content Builder you will need to create a ".MGCB" text file in your Content folder and either add your content using the tool, or use the IMPORT feature to use an existing .ContentProject configuration
Finally, set the "Build Action" of the MGCB file to "MonoGameContentReference" (which is only available after updating your .CSPROJ file) and build your project
(Don't forget to exclude your existing .XNB / content files once you have upgraded)

_Release Notes
3.2.3 release
Updated to latest development build (10th December 2014) Which includes the new 8.1 builds.
More details can be found at http://monogame.net

_Known Issues
None