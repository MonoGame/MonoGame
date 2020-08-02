# Development requirements

Depending on the [platform](platforms.md) that you are targeting, MonoGame has different sets of requirements.

## For desktop platforms on Windows

You will need a copy of [Visual Studio 2019](https://www.monogame.net/downloads/) or later installed (any edition, including Community) before installing MonoGame, with the following components (depending on your target platform):

![Visual Studio optional components](~/images/getting_started/1_installer_vs_components.png)

* .NET Core cross-platform development - For Windows GL (.NET Core) / DX (NetStandard) platforms
* Mobile Development with .NET - For Android / iOS platforms
* Universal Windows Platform development - For Windows 10 / Xbox UWP

> You can also optionally install the ".Net Desktop Development - For Windows GL / DX platforms" to support older MonoGame projects.  See the migration guide on the steps to upgrade projects.

When installing Visual Studio, it also is recommended to include the "**" components:

![.NET Core component](~/images/getting_started/1_netdesktopcomponet.png)

> Alternatively, you can specifically install the [.NET Core SDK from here](https://dotnet.microsoft.com/download), SDK Versions 3.1 and above.

If you are targeting WindowsDX, you are also going to need [the DirectX June 2010 runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109) for audio and gamepads to work properly.

## For desktop platforms on Mac

You will need a copy of [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac/) installed before installing MonoGame.

Additionally, you will need to install the [.NET Core SDK from here](https://dotnet.microsoft.com/download), SDK Versions 3.1 and above.

For editing content you also need the [MGCB Editor](~/articles/tools/mgcb_editor.md) GUI to manage your content.

## For desktop platforms using only the CLI (all supported operating systems)

You only need to install the [.NET Core SDK from here](https://dotnet.microsoft.com/download) to be able to run the necessary commands to create and build projects.

After installation, you can run `dotnet --info` in a terminal to make sure the installation was successful.

## For UWP platforms

MonoGame requires the latest Windows 10 SDK.
You can install it by selecting the Universal App payload when installing Visual Studio 2019.
Building and publishing for UWP is only supported with Visual Studio 2019.

UWP development is not possible from macOS or Linux.

## For mobile platforms

MonoGame requires either the iOS or Android components for Visual Studio depending on the target.

* In Visual Studio you can install both by selecting the 'Mobile development with .NET' workload.
* In Visual Studio for Mac you can install the iOS and Android workload separately.

> Only Visual Studio 2019 or Visual Studio 2019 for Mac are supported in those contexts, **mobile development for MonoGame is not possible from Linux.**
