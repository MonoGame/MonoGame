# What's New

The MonoGame 3.8.1 release marks some big changes in how we build and distribute.

> [!NOTE] Refer to the [Changelog](../../CHANGELOG.md) for a more complete list of changes.

## .NET 6 Support

We now support [.NET 6](https://docs.microsoft.com/en-us/dotnet/core/introduction) exclusively.  This brings us up to date with the latest improvements in the .NET ecosystem and allows for exciting new features like [.NET NativeAOT Runtime](https://github.com/dotnet/runtimelab/tree/feature/NativeAOT) and much easier distribution of your games for Windows, macOS and Linux.

## Visual Studio 2022 extension

MonoGame 3.8.1 now comes with an optional Visual Studio extension which will install all the MonoGame project templates and will allow a quick access to the [MGCB Editor](./tools/mgcb_editor.md).

This extension is avaible for Visual Studio 2022, and Visual Studio 2022 for mac.

## Visual Studio 2019 and prior are no more supported

Because .NET 6 isn't supported by Visual Studio 2019, starting with MonoGame 3.8.1 it will no more be possible to build games with it.

Moving forward, we will only support Visual Studio 2022, and Visual Studio 2022 for mac.

If you need to use Visual Studio 2019, we encourage you to stick to MonoGame 3.8.0.

Rider and Visual Studio Code can be used regardless of the version of MonoGame.

## Apple M1 silicon support

Games built using the ```DesktopGL``` [platform](./platforms/0_platforms.md) and targeting ```osx-arm64``` will now run natively on Apple M1 silicon without Rosetta emulation.

However, it is not yet possible to use the [MGCB](./tools/mgcb.md) or the [MGCB Editor](./tools/mgcb_editor.md) on Apple M1 silicon, unless you are running the ```osx-x64``` variant of the .NET SDK (and therefore using Rosetta emulation).
