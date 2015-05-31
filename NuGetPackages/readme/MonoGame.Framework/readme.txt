Welcome to MonoGame on NuGet

_Description
MonoGame is an open source implementation of the Microsoft XNA 4.x Framework.
The goal is to make it easy for XNA developers to create cross-platform games with extremely high code reuse. We currently support iOS, Android, Windows (both OpenGL and DirectX), Mac OS X, Linux, Windows 8 Store, Windows Phone 8, PlayStation Mobile, and the OUYA console.

This package contains only the Binaries for the MonoGame framework, to enable easier platform dll updates.  

_Supported Platforms
Each NuGet now targets an individual platform to make for easier management and targeting.  It also allows for a more varied target group and some of the specialised platforms.

The currently available packages are:
	* Windows GL (OpenGL)
	* Windows DX (DirectX)
	* Windows 8 & 8.1
	* Windows Phone 8
	* Windows Phone 8.1 
	* Windows Universal projects
	* iOS (Xamarin.iOS only)
	* MacOS (net4 / net45 / MonoMac and Xamarin.Mac)
	* Android
	* Ouya
	* Linux

_Contents
This package contains only MonoGame DLL references for a solution.
For project templates either use the MonoGame Project templates from the MonoGameSetup installer on windows or the MonoDevelop/Xamarin Studio Addin on Mac.
Both installers can be sourced from http://www.monogame.net/downloads/

**Note this package no longer contains the .Net and .GamerServices namespaces, please see the adjoining MonoGame.Net NuGet packages if you require them**


_Install Instructions
If you have created your project using the installed MonoGame templates, you will need to remove any MonoGame references first.
Such references include:
	* OpenTK
	* TAO.sdl
	* SDL
	* Lidgren
**Do not remove any MONO references as these are NOT included

When applied, this NuGet will install all relevant dll's for MonoGame.

**Updating to use the new MonoGame Content Builder tool
To use the new content builder tool you will need the latest development MonoGameSetup installer which can be located at http://www.monogame.net/downloads/ in the Development Builds section
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
For release details, please see the http://monogame.net site

_Known Issues
None