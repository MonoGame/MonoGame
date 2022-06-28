# Setting up your development environment for macOS

This section provides a step-by-step guide for setting up your development environment on macOS.

MonoGame can work with most .NET compatible tools, but we recommend [Visual Studio 2022 for Mac](https://visualstudio.microsoft.com/vs/mac/) (prior versions are not supported).

Alternatively, you can use [JetBrains Rider](https://www.jetbrains.com/rider/) or [Visual Studio Code](https://code.visualstudio.com/).

## Install Visual Studio for Mac

Go to the following URL to download and install Visual Studio 2022 for Mac: https://visualstudio.microsoft.com/vs/mac/

### Install MonoGame extension for Visual Studio for Mac

Download the MonoGame extension for Visual Studio 2022 for Mac from the following link: https://github.com/MonoGame/MonoGame/releases/tag/v3.8.1

Open up Visual Studio 2022 for Mac and you should be able to see a window as shown below:

![VS for Mac installer](~/images/getting_started/vsmac-mg-install-1.png)

In the menu bar, click on **Visual Studio**, and then click on the **Extensions...** menu item.

![Launch Extensions manager](~/images/getting_started/vsmac-mg-install-2.png)

Next, click on the **Install from file...** button in the bottom left and select the extension file you downloaded in the previous step.

![Import VSM extension](~/images/getting_started/vsmac-mg-install-3.png)

Finally, click on the Install button once again.

![Install VSM extension](~/images/getting_started/vsmac-mg-install-4.png)

## [Optional] Set up Wine for effect compilation

Effect (shader) compilation requires access to DirectX, so it will not work natively on macOS systems, but it can be used through Wine. Here are instructions to get this working.

Install brew

```sh
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/master/install.sh)"
```

Install wine64:

```sh
brew install xquartz
brew install wine-stable
brew install p7zip wget
```

Create wine prefix:

```sh
wget -qO- https://raw.githubusercontent.com/MonoGame/MonoGame/master/Tools/MonoGame.Effect.Compiler/mgfxc_wine_setup.sh | bash
```

If you ever need to undo the script, simply delete the `.winemonogame` folder in your home directory.

**Next up:** [Creating a new project](2_creating_a_new_project_vsm.md)

## [Alternative] Install the .NET 6 SDK (compatible with JetBrains Rider and Visual Studio Code)

If you prefer to use JetBrains Rider or Visual Studio Code, after installing any of them you will need to [install the .NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

Once the .NET 6 SDK is installed, you can open a terminal and install the MonoGame templates by typing the following command:

```sh
dotnet new --install MonoGame.Templates.CSharp
```

**Next up:** [Creating a new project](2_creating_a_new_project_vsm.md)
