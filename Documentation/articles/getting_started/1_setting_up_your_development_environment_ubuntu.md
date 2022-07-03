# Setting up your development environment for Linux

This section provides a step-by-step guide for setting up your development environment on Linux.

The only development environment that MonoGame officially supports on Linux is [Visual Studio Code](https://code.visualstudio.com/).

## Install .NET 6 SDK

To install the .NET 6 SDK on your Linux distribution, please follow [Microsoft's instructions]([https://docs.microsoft.com/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website).

## Install Visual Studio Code

To install Visual Studio on your Linux distribution, please follow [Microsoft's instructions](https://code.visualstudio.com/docs/setup/linux).

## Install Visual Studio Code C# extension:

In order to code and build C# projects, you will also need to install a Visual Studio Code C# extension. You can do this with the following command within the Visual Studio Code command line interface:

```sh
code --install-extension ms-dotnettools.csharp
```
> Or alternatively, select the "Extensions" tab on the left hand side in VSCOde and search for the C# Extension published by Microsoft.

## Install MonoGame templates

The following command will install templates for the .NET CLI and Rider IDE. 

> There is no template support for MonoDevelop.

```sh
dotnet new --install MonoGame.Templates.CSharp
```

## [Optional] Set up Wine for effect compilation

Effect (shader) compilation requires access to DirectX, so it will not work natively on Linux systems, but it can be used through Wine. Here are instructions to get this working (providing that your distribution is using apt).

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
