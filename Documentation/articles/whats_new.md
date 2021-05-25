# What's New

The MonoGame 3.8 release marks some big changes in how we build and distribute.

> [!NOTE] Refer to the [Changelog](../../CHANGELOG.md) for a more complete list of changes.

## .NET Support

We now support [.NET 5](https://docs.microsoft.com/en-us/dotnet/core/introduction) in addition to .NET 4.5 target frameworks.  This brings us up to date with the latest improvements in the .NET ecosystem and allow for exciting new features like [.NET NativeAOT Runtime](https://github.com/dotnet/runtimelab/tree/feature/NativeAOT) and much easier distribution of your games for Windows, macOS and Linux.

## NuGet Distribution

With this release MonoGame has moved away from traditional installers and has opted for using [NuGet](https://www.nuget.org/profiles/MonoGame) for all distribution of assemblies and tools.  This also includes the new Visual Studio templates which are a VS extension.

## Visual Studio 2019 and .NET CLI templates

We now have templates for both Windows and macOS versions of Visual Studio 2019 as well as templates for the .NET CLI tools.

## SDK-Style Projects in the repository

[Protobuild](https://github.com/Protobuild/Protobuild) served us well in helping avoid manual synchronization of all our different platform projects.  With the new [SDK-style projects](https://docs.microsoft.com/en-us/dotnet/core/project-sdk/overview#project-files) supported in .NET, and VS2019 we can now easily maintain the projects and solutions in the repo.

## Removed Portable Assemblies

The [MonoGame.Framework.Portable](https://www.nuget.org/packages/MonoGame.Framework.Portable/) and [MonoGame.Framework.Content.Pipeline.Portable](https://www.nuget.org/packages/MonoGame.Framework.Content.Pipeline.Portable/) are no longer supported.  This is mainly because Microsoft changed the assembly replacement rules needed to make them work in the new project system.  We now recommend using [MonoGame.Framework.DesktopGL](https://www.nuget.org/packages/MonoGame.Framework.DesktopGL) and [MonoGame.Framework.Content.Pipeline](https://www.nuget.org/packages/MonoGame.Framework.Content.Pipeline) and disable the PrivateAssets to avoid copying any dependent assemblies.

An example on how to set it up can be found in the templates with "MonoGame NetStandard Library" and "MonoGame Pipeline Extension" templates respectively.

## Compile Effects via Wine

We've added a script and made fixes so that [MGFXC](tools/mgfxc.md) can run on Mac and Linux under Wine. For more information checkout the setting up the development environment for [macOS](getting_started/1_setting_up_your_development_environment_macos.md) and [Ubuntu](getting_started/1_setting_up_your_development_environment_ubuntu.md)
