
Depending on the [platform](Platforms.md) that you are targeting, MonoGame has different sets of requirements.

## For desktop platforms

MonoGame requires a .NET Core SDK (3.1 or up) installation.

You can either install it [independently](https://dotnet.microsoft.com/download/dotnet-core), or by selecting the .NET Core payload when installing Visual Studio 2019 (version 15.4 and up required).

If you are targeting WindowsDX, you are also going to need [the DirectX June 2010 runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109) for audio and gamepads to work properly.

When it comes to IDE, Visual Studio 2019, Visual Studio Code, and Visual Studio 2019 for Mac are supported (alternatively, you can work directly from the CLI with your code editor of choice).

Desktop development is possible from any operating system.

## For UWP platforms

MonoGame requires the latest Windows 10 SDK.

You can install it by selecting the Universal App payload when installing Visual Studio 2019.

Building and publishing for UWP is only supported with Visual Studio 2019.

UWP development is not possible from macOS or Linux.

## For mobile platforms

MonoGame requires either Xamarin.iOS or Xamarin.Android depending on the target.

In Visual Studio you can install both by selecting the 'Mobile development with .NET' workload.
In Visual Studio for Mac you can install the iOS and Android workload separately.

Only Visual Studio 2019 or Visual Studio 2019 for Mac are supported in those contexts.

Mobile development is not possible from Linux.
