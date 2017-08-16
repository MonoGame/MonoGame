## Runtime requirements

A DesktopGL game targeting Windows will require the following in order to run:
# Windows XP or above (XP is supported only if .Net Framework 4.0 is targeted; 4.5+ will require Windows Vista or above)
# OpenGL 3.0 compliant graphics card and driver
# .Net Framework 4.5
# SDL 2.0.4 or above
# OpenAL-Soft

When you build a DesktopGL project, you should have in your output folder two other folders named "x86" and "x64" containing SDL and OpenAL-Soft DLLs. There is therefore no need to package anything else since .Net is a system component since Windows Vista (unless you're targeting a Windows XP compatibility).

## Packaging

There is no need to package a DesktopGL game targeting Windows. The best practice for distribution is to simply zip your output folder and distribute it as-is.

### Useless files for Windows

In your output folder, there should be a few files that are useless for Windows.

You can delete any .dylib files, and inside the "x86" and "x64" folders, you a free to delete libopenal.so.1 and libSDL2-2.0.so.0.

Your game is now ready to be published on Linux.

### Mind the OpenAL version

OpenAL exists in two flavors on Windows:
# OpenAL, by Creative Labs (typically comes in an installer named oalinst.zip or a standalone dll named openal32.dll)
# OpenAL-Soft, an open-source project (named oal_soft.dll; MonoGame is built upon this version)

The Creative Labs implemention is now a legacy product and it is highly recommended to use only OpenAL-Soft. The correct dependency is shipped with MonoGame and should be in your output folder after you built your project.

## Special note for Steam

Make sure that your Steamworks settings are set to verify the presence of .Net 4.5 (or 4.0 if you're targeting Windows XP), Steam will then install it automatically before the first launch of your game.

Even though OpenAL is a requirement, do not set it in Steamworks. It is highly recommended to ship it with your game in order to make sure that OpenAL-Soft is used and no other version.

## Special note for itch.io

For your game to be compatible with the itch.io desktop application, you only have to zip it. Make sure to use the zip format (rar is forbidden by itch.io) and to have the .exe file at the root of the zip archive.

The itch.io desktop application doesn't check for software requirements yet (it is planned), hence it is recommended to document them in a readme file and on the installation instructions section of your itch.io game page.

