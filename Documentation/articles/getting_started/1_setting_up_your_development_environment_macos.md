# Setting up your development environment for macOS

In this section we will go over setting up your development environment for macOS.

## Install Visual Studio for Mac

Go to the following URL to download and install Visual Studio for Mac: https://visualstudio.microsoft.com/vs/mac/

## Install MonoGame extension for Visual Studio for Mac

Download the MonoGame extension for Visual Studio for Mac from the following page: https://www.monogame.net/downloads/

Open up Visual Studio for Mac and you should be able to see a window like so:

![VS for Mac installer](~/images/getting_started/vsmac-mg-install-1.png)

In the menu bar, click on **Visual Studio**, and then click on the **Extensions...** menu item.

![Launch Extensions manager](~/images/getting_started/vsmac-mg-install-2.png)

Next, click on the **Install from file...** button in the bottom left and select the extension file we downloaded in the previous step.

![Import VSM extension](~/images/getting_started/vsmac-mg-install-3.png)

And finally click on the Install button once again.

![Install VSM extension](~/images/getting_started/vsmac-mg-install-4.png)

## [Optional] Install MonoGame templates for .NET Core CLI or Rider IDE

```sh
dotnet new --install MonoGame.Templates.CSharp
```

## [Optional] Install MGCB Editor

MGCB Editor is a tool for editing the .mgcb files, which are used for building the content.

If you plan on using Visual Studio for Mac, the extension we installed already contains an integrated version of this tool so you can skip this step.

```sh
dotnet tool install --global dotnet-mgcb-editor
mgcb-editor --register
```

## [Optional] Set up Wine for effect compilation

Effect compilation requires access to some DirectX compiler stuff so it can't natively work on macOS systems, however we can use it through Wine.

Install brew

```sh
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/master/install.sh)"
```

Install wine64:

```sh
brew cask install xquartz
brew cask install wine-stable
brew install p7zip
```

Create wine prefix:

```sh
wget -qO- https://raw.githubusercontent.com/MonoGame/MonoGame/develop/Tools/MonoGame.Effect.Compiler/mgfxc_wine_setup.sh | bash
```

If you ever need to undo the script, simply delete the `.winemonogame` folder in your home directory.

**Next up:** [Creating a new project](2_creating_a_new_project_vsm.md)
