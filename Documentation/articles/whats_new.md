# What's New

The MonoGame 3.8 release marks some big changes in how we build and distribute.

> [!NOTE] Refer to the [Changelog](../../CHANGELOG.md) for a more complete list of changes.

## .NET Core Support
We now support [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/introduction) in addition to .NET 4.5 target frameworks.  This brings us up to date with the latest improvements in the .NET ecosystem and allow for exciting new features like XXXX and XXXX.

## SDK-Style Projects
[Protobuild](https://github.com/Protobuild/Protobuild) served us well in helping avoid manual syncronzation of all our different platform projects.  With the new [SDK-style projects](https://docs.microsoft.com/en-us/dotnet/core/project-sdk/overview#project-files) supported in .NET Core, VS2017, and VS2019 we can now easily maintain the projects and solutions in the repo.

## NuGet Distribution
With this release MonoGame has moved away from tranditional installers and has opted for using [NuGet](https://www.nuget.org/profiles/MonoGame) for all distribution of assemblies and tools.  This also includes the new Visual Studio templates which are a VS extension.

## Removed Portable Assemblies
The [MonoGame.Framework.Portable](https://www.nuget.org/packages/MonoGame.Framework.Portable/) and [MonoGame.Framework.Content.Pipeline.Portable](https://www.nuget.org/packages/MonoGame.Framework.Content.Pipeline.Portable/) are no longer supported.  This is mainly because Microsoft changed the assembly replacement rules needed to make them work in the new project system.  We now recommend using [MonoGame.Framework.DesktopGL](https://www.nuget.org/packages/MonoGame.Framework.DesktopGL) and [MonoGame.Framework.Content.Pipeline](https://www.nuget.org/packages/MonoGame.Framework.Content.Pipeline) and disable the XXXXXXX to avoid copying any dependend assemblies.

## Compile Effects via Wine
We've added a script and made fixes so that [MGFXC](tools/mgfxc.md) can run on Mac and Linux under Wine.  LINK OR INSTRUCTIONS ON HOW TO DO THIS HERE!

