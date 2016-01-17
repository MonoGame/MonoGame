## Runtime requirements

A DesktopGL game targeting Windows will require the following in order to run:
# Windows Vista or above (XP is unsupported)
# OpenGL 4.0 compliant graphics card and driver
# .Net Framework 4.5

## Placing the OpenAL dll

In order to run without any additionnal dependency, it is required to place openal32.dll next to your .exe file.

## Packaging

There is no need to package a DesktopGL game targeting Windows.

## Special note for Steam

Make sure that your Steamworks settings are set to verify the presence of .Net 4.5 and Direct X June 2010 redist. Steam will then install them automatically.

## Special note for itch.io

For your game to be compatible with the itch.io desktop application, you only have to zip it. Make sure to use the zip format (rar is forbidden by itch.io) and to have the .exe file at the root of the zip archive.

The itch.io desktop application doesn't check for software requirements yet (it is planned), hence it is recommended to document them in a readme file and on the installation instructions section of your itch.io game page.

