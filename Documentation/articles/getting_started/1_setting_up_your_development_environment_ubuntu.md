# Setting up your development environment for Ubuntu 20.04

In this section we will go over setting up your development environment for Ubuntu 20.04.

## Install .NET Core SDK

Add repository:

```sh
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
sudo dpkg -i /tmp/packages-microsoft-prod.deb
sudo apt update
```

Install packages:

```sh
sudo apt-get install -y apt-transport-https
sudo apt-get install -y dotnet-sdk-3.1
```

## [Optional] Install mono

Mono is a C# runtime, just like .NET Core, and it is not necessary to install it if you are just targeting Linux, however if you want to target some other platforms, like Android, it will be required.

Add repository:

```sh
sudo apt install gnupg ca-certificates
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb https://download.mono-project.com/repo/ubuntu stable-focal main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
sudo apt update
```

Install packages:

```sh
sudo apt install -y mono-devel
```

## Install Visual Studio Code

Add repository:

```sh
curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /tmp/packages.microsoft.gpg
sudo install -o root -g root -m 644 /tmp/packages.microsoft.gpg /etc/apt/trusted.gpg.d/
sudo sh -c 'echo "deb [arch=amd64 signed-by=/etc/apt/trusted.gpg.d/packages.microsoft.gpg] https://packages.microsoft.com/repos/vscode stable main" > /etc/apt/sources.list.d/vscode.list'
sudo apt update
```

Install packages:

```sh
sudo apt-get install code
```

Install C# extension:

```sh
code --install-extension ms-dotnettools.csharp
```

## Install MonoGame templates

This will install templates for .NET Core CLI and Rider IDE. There is no template support for MonoDevelop.

```sh
dotnet new --install MonoGame.Templates.CSharp
```

## Install MGCB Editor

MGCB Editor is a tool for editing the .mgcb files, which are used for building the content.

```sh
dotnet tool install --global dotnet-mgcb-editor
mgcb-editor --register
```

## [Optional] Set up Wine for effect compilation

Effect compilation requires access to some DirectX compiler stuff so it can't natively work on Linux systems, however we can use it through Wine.

Install wine64:

```sh
sudo apt install wine64 p7zip-full
```

Create wine prefix:

```sh
wget -qO- https://raw.githubusercontent.com/MonoGame/MonoGame/develop/Tools/MonoGame.Effect.Compiler/mgfxc_wine_setup.sh | bash
```

If you ever need to undo the script, simply delete the `.winemonogame` folder in your home directory.

**Next up:** [Creating a new project](2_creating_a_new_project_netcore.md)
