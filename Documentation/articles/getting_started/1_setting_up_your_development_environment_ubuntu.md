# Setting up your development environment for Linux

This section provides a step-by-step guide for setting up your development environment for Linux.

The only development environment that MonoGame officially supports on Linux is Visual Studio Code.

## Install .NET 6 SDK

To install the .NET 6 SDK on your Linux distribution, please follow [Microsoft's instructions]([https://docs.microsoft.com/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website).

## Install Visual Studio Code

To install Visual Studio on your Linux distribution, please follow [Microsoft's instructions](https://code.visualstudio.com/docs/setup/linux).

## Install Visual Studio Code C# extension:

In order to code and build C# projects, you will need to install Visual Studio Code C# extension. You can do this with the following command within the Visual Studio Code command line interface:

```sh
code --install-extension ms-dotnettools.csharp
```

## Install MonoGame templates

This will install templates for .NET CLI and the Rider IDE. There is no template support for MonoDevelop.

```sh
dotnet new --install MonoGame.Templates.CSharp
```

## [Optional] Set up Wine for effect compilation

Effect (shader) compilation requires access to DirectX, so it won't work natively on Linux systems, but it can be used through Wine. Here are instructions to get this working (providing that your distribution is using apt).

Install wine64:

```sh
sudo apt install wine64 p7zip-full
```

Create wine prefix:

```sh
wget -qO- https://raw.githubusercontent.com/MonoGame/MonoGame/master/Tools/MonoGame.Effect.Compiler/mgfxc_wine_setup.sh | bash
```

If you ever need to undo the script, simply delete the `.winemonogame` folder in your home directory.

**Next up:** [Creating a new project](2_creating_a_new_project_netcore.md)
