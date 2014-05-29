Welcome to MonoGame.Net on NuGet

_Description
MonoGame is an open source implementation of the Microsoft XNA 4.x Framework.
The goal is to make it easy for XNA developers to create cross-platform games with extremely high code reuse. We currently support iOS, Android, Windows (both OpenGL and DirectX), Mac OS X, Linux, Windows 8 Store, Windows Phone 8, PlayStation Mobile, and the OUYA console.

This package is just the Binaries for MonoGame.Net for easier platform dll updates.  

_Supported Platforms
	* Windows GL
	* Windows 8
	* Windows Phone
	* iOS
	* MacOS
	* Android

_Contents
This package contains only MonoGame.Net DLL references for a solution containing the .Net and .GamerServices namespaces only
For project templates either use the Monogame Project templates or the Main MonoGame NuGet package


_Install Instructions
This package simply sits along side the existing MonoGame.Framework package
It has a dependancy on the MonoGame.Binaries NuGet package, please check the install instructions for that package if not yet installed

When applied, this NuGet will install all relevant dll's for MonoGame.Net including LidGren
**Note You may not see MonoGame or it's dependant libraries in the project references, this is Normal as we use MSBuild configuration to refernece them


_Release Notes
3.2.2 release
Updated to latest development build following MonoGame.Framework.Net separation and numerous bug fixes.
More details can be found at http://monogame.net

_Known Issues
None