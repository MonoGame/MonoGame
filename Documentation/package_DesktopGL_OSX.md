A DesktopGL game targeting OS X requires that Mono to be bundled with it. This section describes how to do it and how to create an app bundle.

## Runtime requirements

A DesktopGL game targeting OS X will require the following in order to run:
# OS X Snow Leopard 10.6 or above
# OpenGL 4.0 compliant graphics card and driver

## Preparing your game files

### Getting and configuring MonoKickstart

### Additional dependencies

If your game uses other libraries than MonoGame, verify if they make calls to native libraries. A good way to know it, is to try running the .app bundle on an OS X without the Mono SDK installed. If there is any native library missing, the game will run into a DllNotFoundException like this one:

Unhandled Exception: System.TypeInitializationException: An exception was thrown by the type initializer for  System.Drawing.GDIPlus ---> System.DllNotFoundException: gdiplus.dll

To add this native dependency to your MonoKickstart configuration, you will have to first edit the monoconfig file by adding the mapping to the OS X equivalent:

	<dllmap dll="gdiplus.dll" target="libgdiplus.dylib" os="osx"/>
	
This will tell Mono to stop looking for a Windows .dll file and to look for an OS X .dylib.

The final step is to place this .dylib in the osx folder. Your native dependencies will now load correctly.

### Structuring your .app bundle

OS X applications are structured into what is called an .app bundle. It is nothing but a specific file hierarchy with a manifest and an icon.

An .app bundle should be structured like this:

YourGame.app
	Contens
		Resources
			YourGame.icns   <-- this is your game icon
		MacOS
			All you MonoKickstart files and folders
		Info.plist   <-- Mac manifest file
		
And here is what the Info.plist should look like:



If your .app bundle is correctly structured, it will now appear as an executable bundle on OS X instead of a regular folder.

Your game is now ready to be published on OS X!

## Special note for Steam

There is nothing more to do, your game is Steam-ready!

## Special note for itch.io

For your app bundle to be compatible with the itch.io desktop application, you only have to zip it. Make sure to use the zip format (rar is forbidden by itch.io) and to have you .app bundle at the root of the zip archive.

## Special note for the Mac App Store

For your .app bundle to pass the certification, your icon file must be of at least 4096x4096 (yup, that's big).