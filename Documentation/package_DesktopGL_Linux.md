DesktopGL games targeting Linux require that Mono is bundled with it. This section describes how to do it and how to create packages for standard distribution

## Runtime requirements

A DesktopGL game targeting Linux will require the following in order to run:
# libc 2.14 or above
# OpenGL 3.0 compliant graphics card and driver (or alternatively OpenGL 2.0 with the ARB_framebuffer_object extension)
# OpenAL
# SDL 2.0.4
# Mono 3.0 (or preferably 4.4+)

libc and OpenGL are core parts of Linux distributions, hence this documention will focus on packaging and distributing games with OpenAL, SDL, and Mono.

## MonoKickstart

MonoKickstart is a precompiled and embeddable version of Mono. It allows to run .Net application without having to install the Mono Framework. This documentation makes use of it in order to make a standalone version of your game.

Download MonoKickstart from here: https://github.com/flibitijibibo/MonoKickstart

Once you downloaded MonoKickstart, extract all the content from the precompiled folder to your output folder.

Rename the "Kick" file to match your game .exe name. Do the same with Kick.bin.x86 and Kick.bin.x64_64. For instance, if your game is MyGame.exe, you should now have a MyGame, a MyGame.bin.x86 and a MyGame.bin.x86_64 files.

Edit the previously named Kick file and modify any mention to Kick.bin.* to the correct file name.

Create a folder nammed "lib" and move the files libopenal.so.1 and libSDL2-2.0.so.0 from the "x86" forlder to it.

Create a folder nammed "lib64" and move the files libopenal.so.1 and libSDL2-2.0.so.0 from the "x64" forlder to it.

Your game should now run by launching the kick script (e.g. "./MyGame").

### Useless files for Linux

In your output folder, there should be a few files that are useless for Linux.

You are free to delete the "x86" and "x64" folders, as well as the Kick.bin.osx and Kick.bin.x64 files. These are for Windows and Mac OS.

Your game is now ready to be published on Linux.

## Special note for Steam

There is nothing more to do, your game is Steam-ready.

## Special note for itch.io

For your app bundle to be compatible with the itch.io desktop application, you only have to zip it. Make sure to use the zip format (rar is forbidden by itch.io) and to have your .app bundle at the root of the zip archive.
