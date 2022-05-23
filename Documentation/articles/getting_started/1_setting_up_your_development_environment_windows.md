# Setting up your development environment for Windows

This section provides a step-by-step guide for setting up your development environment on Windows.

MonoGame can work with most .NET compatible tools, but we recommend [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (prior versions are not supported with MonoGame 3.8.1).

Alternatively, you can use [JetBrains Rider](https://www.jetbrains.com/rider/) or [Visual Studio Code](https://code.visualstudio.com/).

## [Recommended] Install Visual Studio 2022

Before using MonoGame, you will need to install [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or later (any edition, including Community) with the following workloads, depending on your desired [target platform(s)](~/platforms.md):

* .NET desktop development (mandatory for all platforms)
* Mobile Development with .NET (optional, if you wish to target Android, iOS, or iPadOS)
* Universal Windows Platform development (optional, if you wish to build for the Windows Store, or Xbox)

![Visual Studio optional components](~/images/getting_started/1_installer_vs_components.png)

If you are targeting the standard Windows DirectX backend, you will also need [the DirectX June 2010 runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109) for audio and gamepads to work properly.

### Install MonoGame extension for Visual Studio 2022

To create new projects from within Visual Studio 2022, you will need to install the MonoGame extension, which can be installed from "*Extensions -> Manage Extensions*" in the Visual Studio 2022 menu bar.

![Visual Studio Extension Manager](~/images/getting_started/1_VisualStudioExtensionManager.png)

Once it is open, simply search for **MonoGame** in the top right search window, as shown above, and install the "MonoGame Framework C# project templates" (make sure that it is version 3.8.1 or above).  You now have the MonoGame templates installed, ready to create new projects.

**Next up:** [Creating a new project](2_creating_a_new_project_vs.md)

## [Alternative] Install the .NET 6 SDK (compatible with JetBrains Rider and Visual Studio Code)

If you prefer to use JetBrains Rider or Visual Studio Code, and after installing any of them, you will also need to [install the .NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

Once the .NET 6 SDK is installed, you can open a Command Prompt and install the MonoGame templates by typing the following command:

```sh
dotnet new --install MonoGame.Templates.CSharp
```

**Next up:** [Creating a new project](2_creating_a_new_project_vs.md)
