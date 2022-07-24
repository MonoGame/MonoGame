Depending on the [platform](Documentation/articles/platforms/0_platforms.md) that you are targeting, MonoGame has different sets of requirements.

For desktop platforms
====================

MonoGame requires a .NET 6 SDK installation.
You can either install it [independently](https://dotnet.microsoft.com/download/dotnet), or by selecting the .NET workload when installing Visual Studio 2022 (version 17.0 and up required).

If you are targeting WindowsDX, you are also going to need [the DirectX June 2010 runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109) for audio and gamepads to work properly.

When it comes to IDE, [Visual Studio 2022](https://visualstudio.microsoft.com/vs/), [Visual Studio Code](https://code.visualstudio.com/), and [Visual Studio 2022 for Mac](https://visualstudio.microsoft.com/vs/mac/preview/) are supported (alternatively, you can work directly from the CLI with your code editor of choice). [JetBrains Rider](https://www.jetbrains.com/rider/) should work but isn't officially supported.

Desktop development is possible from any operating system supporting the above mentioned software.

For UWP platforms
====================

MonoGame requires the latest Windows 10 SDK.
You can install it by selecting the Universal App workload when installing Visual Studio 2022.
Building and publishing for UWP is only supported with Visual Studio 2022.

UWP development is not possible from macOS or Linux.

For mobile platforms
====================

MonoGame requires either Xamarin.iOS or Xamarin.Android depending on the target.

In Visual Studio 2022 you can install both by selecting the "Mobile development with .NET" workload.
In Visual Studio 2022 for Mac you can install the iOS and Android workload separately.

Only Visual Studio 2022 or Visual Studio 2022 for Mac are supported in those contexts.

Mobile development is not possible from Linux.
