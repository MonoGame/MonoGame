# Setting up your development environment for Windows

This section provides a step-by-step guide for setting up your development environment for Windows.

## Install Visual Studio 2019

Before installing Monogame, you'll need to install [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) or later (any edition, including Community) with the following components, depending on your target platform:

* .NET cross-platform development - For Desktop OpenGL and DirectX platforms
* Mobile Development with .NET - For Android and iOS platforms
* Universal Windows Platform development - For Windows 10 and Xbox UWP platforms
* .Net Desktop Development - For Desktop OpenGL and DirectX platforms to target normal .NET Framework

![Visual Studio optional components](~/images/getting_started/1_installer_vs_components.png)

![.NET Desktop component](~/images/getting_started/1_netdesktopcomponet.png)

If you are targeting the standard Windows DirectX backend, you'll also need [the DirectX June 2010 runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109) for audio and gamepads to work properly.

## Install MonoGame extension for Visual Studio 2019

To create new projects from within Visual Studio, you will need to install the Visual Studio 2019 extension, which can be installed from "*Extensions -> Manage Extensions*" in the Visual Studio menu bar.

![Visual Studio Extension Manager](~/images/getting_started/1_VisualStudioExtensionManager.png)

Once it's open, simply search for **MonoGame** in the top right search window, as shown above, and install the "MonoGame project templates".  You now have the MonoGame templates installed, ready to create new projects.

## Install MGCB Editor

MGCB Editor is a tool for editing .mgcb files, which are used for building content.

To register the MGCB Editor tool with Windows and Visual Studio 2019, run the following from the Command Prompt.

```sh
dotnet tool install --global dotnet-mgcb-editor
mgcb-editor --register
```

## [Optional] Install MonoGame templates for .NET CLI or Rider IDE

```sh
dotnet new --install MonoGame.Templates.CSharp
```

**Next up:** [Creating a new project](2_creating_a_new_project_vs.md)
