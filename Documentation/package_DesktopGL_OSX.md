A DesktopGL game targeting Mac OS requires that Mono to be bundled with it. This section describes how to do it and how to create an app bundle.

## Runtime requirements

A DesktopGL game targeting Mac OS will require the following in order to run:
# Mac OS X Snow Leopard 10.6 or above
# OpenGL 3.0 compliant graphics card and driver (all Macs should meet this requirement)
# OpenAL
# SDL 2.0.4
# Mono 3.0 (or preferably 4.4+)

OpenGL is a core component of Mac OS, hence this documention will focus on packaging and distributing games with OpenAL, SDL, and Mono.

## MonoKickstart

MonoKickstart is a precompiled and embeddable version of Mono. It allows to run .Net application without having to install the Mono Framework. This documentation makes use of it in order to make a standalone version of your game.

Download MonoKickstart from here: https://github.com/flibitijibibo/MonoKickstart

Once you downloaded MonoKickstart, extract all the content from the precompiled folder to your output folder.

Rename the "Kick" file to match your game .exe name. Do the same with Kick.bin.osx. For instance, if your game is MyGame.exe, you should now have a MyGame and a MyGame.bin.osx files.

Edit the previously named Kick file and modify any mention to Kick.bin.osx to the correct file name.

Create a folder nammed "osx" and move the files libopenal.1.dylib and libSDL2-2.0.0.dylib to it.

Your game should now run by launching the kick script (e.g. "./MyGame").

### Useless files for Mac OS

In your output folder, there should be a few files that are useless for Mac OS.

You are free to delete the "x86" and "x64" folders, as well as the Kick.bin.x86 and Kick.bin.x64 files. These are for Windows and Linux.

## Preparing your game files

### Structuring your .app bundle

Mac OS applications are structured into what is called an .app bundle. It is nothing but a specific file hierarchy with a manifest and an icon.

The only trick is that your Content folder will be separated from the rest of the files.

An .app bundle should be structured like this:

YourGame.app
	Contents
		Resources
			Content 		<-- your game Content folder (i.e. xnb's and stuff)
			YourGame.icns   <-- this is your game icon (icns format)
		MacOS
			All your MonoKickstart files, your games files and folders, except the Content folder
		Info.plist   <-- Mac manifest file
		
And here is what the Info.plist should look like:

<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
    <key>CFBundleExecutable</key>
    <string>MyGame</string>
    <key>CFBundleIconFile</key>
    <string>MyGame</string>
    <key>CFBundleIdentifier</key>
    <string>com.mystudio.MyGame</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>MyGame</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0</string>
    <key>CFBundleSignature</key>
    <string>FONV</string>
    <key>CFBundleVersion</key>
    <string>1</string>
    <key>LSApplicationCategoryType</key>
    <string>public.app-category.games</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.6</string>
    <key>NSHumanReadableCopyright</key>
    <string>Copyright © 2015-2017 Flying Oak Games. All rights reserved.</string>
    <key>NSPrincipalClass</key>
    <string>NSApplication</string>
</dict>
</plist>

If your .app bundle is correctly structured, it will now appear as an executable bundle showing your game icon on Mac OS instead of a regular folder.

Your game is now ready to be published on Mac OS.

## Special note for Steam

There is nothing more to do, your game is Steam-ready.

## Special note for itch.io

For your app bundle to be compatible with the itch.io desktop application, you only have to zip it. Make sure to use the zip format (rar is forbidden by itch.io) and to have your .app bundle at the root of the zip archive.

## Special note for the Mac App Store

For your .app bundle to pass the certification, your icon file must be at least 4096x4096.