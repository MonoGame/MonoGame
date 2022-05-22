Depending on the [platform](platforms.md) that you are targeting, MonoGame has different sets of requirements.

For desktop platforms
====================

MonoGame requires a .NET Core SDK (3.1 or up) installation.
You can either install it [independently](https://dotnet.microsoft.com/download/dotnet-core), or by selecting the .NET Core payload when installing Visual Studio 2019 (version 15.4 and up required).

If you are targeting WindowsDX, you are also going to need [the DirectX June 2010 runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109) for audio and gamepads to work properly.

When it comes to IDE, Visual Studio 2019, Visual Studio Code, and Visual Studio 2019 for Mac are supported (alternatively, you can work directly from the CLI with your code editor of choice).

Desktop development is possible from any operating system.

For UWP platforms
====================

MonoGame requires the latest Windows 10 SDK.
You can install it by selecting the Universal App payload when installing Visual Studio 2019.
Building and publishing for UWP is only supported with Visual Studio 2019.

UWP development is not possible from macOS or Linux.

For mobile platforms
====================

MonoGame requires either Xamarin.iOS or Xamarin.Android depending on the target.

In Visual Studio you can install both by selecting the 'Mobile development with .NET' workload.
In Visual Studio for Mac you can install the iOS and Android workload separately.

Only Visual Studio 2019 or Visual Studio 2019 for Mac are supported in those contexts.

Mobile development is not possible from Linux.

Linux Requirements
==================

The following is the list of packages needed to compile MonoGame from Linux:
 * monodevelop
 * libopenal-dev
 * referenceassemblies-pcl
 * ttf-mscorefonts-installer
 * gtk-sharp3

If on Ubuntu, you can install the packages with the following commands:
```Shell
sudo apt-key adv --keyserver keyserver.ubuntu.com --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
sudo apt-get update
sudo apt-get install -y monodevelop libopenal-dev referenceassemblies-pcl ttf-mscorefonts-installer gtk-sharp3
```

If on Fedora, you can install the packages with the following commands:
```Shell
sudo rpm --import "http://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
sudo dnf config-manager --add-repo http://download.mono-project.com/repo/centos/
sudo dnf update
sudo dnf install -y monodevelop referenceassemblies-pcl mscore-fonts gtk-sharp3
```
