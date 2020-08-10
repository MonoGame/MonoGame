# Setting up your development environment for Windows

This guide will walk you through building a starter game with MonoGame using a Windows and Visual Studio 2019.

## Install Visual Studio 2019

You will need a copy of [Visual Studio 2019](https://www.monogame.net/downloads/) or later installed (any edition, including Community) before installing MonoGame, with the following components (depending on your target platform):

![Visual Studio optional components](~/images/getting_started/1_installer_vs_components.png)

* .NET Core cross-platform development - For Windows GL (.NET Core) / DX (NetStandard) platforms
* Mobile Development with .NET - For Android / iOS platforms
* Universal Windows Platform development - For Windows 10 / Xbox UWP

> You can also optionally install the ".Net Desktop Development - For Windows GL / DX platforms" to support older MonoGame projects.  See the migration guide on the steps to upgrade projects.

When installing Visual Studio, it also is recommended to include the "**" components:

![.NET Desktop component](~/images/getting_started/1_netdesktopcomponet.png)

> Alternatively, you can specifically install the [.NET Core SDK from here](https://dotnet.microsoft.com/download), SDK Versions 3.1 and above.

If you are targeting WindowsDX, you are also going to need [the DirectX June 2010 runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109) for audio and gamepads to work properly.

## For UWP platforms

MonoGame requires the latest Windows 10 SDK.
You can install it by selecting the Universal App payload when installing Visual Studio 2019.
Building and publishing for UWP is only supported with Visual Studio 2019.

## Install MonoGame extension for Visual Studio 2019

To create new projects from within Visual Studio, you will need to install the Visual Studio 2019 extension which can be found in "*Extensions -> Manage Extensions*" in the Visual Studio menu bar.

![Visual Studio Extension Manager](~/images/getting_started/1_VisualStudioExtensionManager.png)

Once open, simply search for **MonoGame** in the top right search window (as shown above) and install the "MonoGame project templates".  You now have the MonoGame templates installed ready to create new projects.

## [Optional] Install MonoGame templates for .NET Core CLI or Rider IDE

```sh
dotnet new --install MonoGame.Templates.CSharp
```

## [Optional] Install MGCB Editor

MGCB Editor is a tool for editing the .mgcb files, which are used for building the content.

To register the MGCB Editor tool with Windows and Visual Studio 2019, run the following from the Command-Prompt.

```sh
dotnet tool install --global dotnet-mgcb-editor
mgcb-editor --register
```

**Next up:** [Creating a new project](2_creating_a_new_project_vs.md)
