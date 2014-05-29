Welcome to MonoGame on NuGet

_Description
MonoGame is an open source implementation of the Microsoft XNA 4.x Framework.
The goal is to make it easy for XNA developers to create cross-platform games with extremely high code reuse. We currently support iOS, Android, Windows (both OpenGL and DirectX), Mac OS X, Linux, Windows 8 Store, Windows Phone 8, PlayStation Mobile, and the OUYA console.

This project is intended to update your Content Builder solution to the latest version and keep in line with other solutions using the MonoGame NuGets

_Supported Platforms
	* Windows


_Contents
This package contains the dll's for the MonoGame Content Processors
This is intended to update an existing MonoGame references in Content Projects.

_Install Instructions
Using an existing Content Builder project, remove the existing MonoGame.ContentProcessors reference in the Content Project itself.
**After installing this NuGet you will need to manually remove the packages.config NuGet files from the Content Project else you will get a content build error

_Release Notes
3.2.2 release in-line with the recent full 3.2 release
More details can be found at http://monogame.net

_Known Issues
NuGet will put a packages.config file in the solution, simply exclude this file from the Content Project
If left in you will get Content Build exceptions because the Content Builder does not know what a .package file is
