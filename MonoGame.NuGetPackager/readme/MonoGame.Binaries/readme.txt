Welcome to MonoGame on NuGet

_Description
MonoGame is an open source implementation of the Microsoft XNA 4.x Framework.
The goal is to make it easy for XNA developers to create cross-platform games with extremely high code reuse. We currently support iOS, Android, Windows (both OpenGL and DirectX), Mac OS X, Linux, Windows 8 Store, Windows Phone 8, PlayStation Mobile, and the OUYA console.

This package is just the Binaries for MonoGame for easier platform dll updates.  

_Supported Platforms
	* Windows GL
	* Windows 8
	* Windows Phone
	* iOS
	* MacOS
	* Android

_Contents
This package contains only MonoGame DLL references for a solution.
For project templates either use the Monogame Project templates or the Main MonoGame NuGet package


_Install Instructions
If you have created your project using the installed MonoGame templates, you will need to remove any MonoGame references first.
Such references include:
	* OpenTK
	* TAO.sdl
	* SDL
	* Lidgren
**Do not remove any MONO references as these are NOT included

When applied, this NuGet will install all relevant dll's for MonoGame.
**Note You may not see MonoGame or it's dependant libraries in the project references, this is Normal as we use MSBuild configuration to refernece them

**Windows Phone 8 users
The MSBuild configuration will remove old XNA references at build time to avoid conflict.
This also applies to CLASS LIBRARIES, all projects in a Windows Phone project that are referencing MonoGame will need this package installed!

_Release Notes
3.2 release inline with the recent full 3.2 release
More details can be found at http://monogame.net

_Known Issues
None