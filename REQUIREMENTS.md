Depending on the [platform](https://docs.monogame.net/articles/platforms.html) that you are targeting, MonoGame has different sets of requirements.

For desktop platforms
====================

MonoGame requires a .NET 8 SDK installation.
You can either install it [independently](https://dotnet.microsoft.com/download/dotnet), or by selecting the .NET workload when installing Visual Studio 2022 (version 17.8 and up required).

If you are targeting WindowsDX, you are also going to need [the DirectX June 2010 runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109) for audio and gamepads to work properly.

When it comes to IDE, [Visual Studio 2022](https://visualstudio.microsoft.com/vs/), [Visual Studio Code](https://code.visualstudio.com/), and [Visual Studio 2022 for Mac](https://visualstudio.microsoft.com/vs/mac/preview/) are supported (alternatively, you can work directly from the CLI with your code editor of choice). [JetBrains Rider](https://www.jetbrains.com/rider/) should work but isn't officially supported.

Desktop development is possible from any operating system supporting the above mentioned software.

::: info UWP Deprecation
The **UWP** platform is being deprecated as of the `3.8.2` release due to Microsoft reducing/removing support for this deployment mechanism.
:::

For mobile platforms
====================

MonoGame requires either Xamarin.iOS or Xamarin.Android depending on the target.

In Visual Studio 2022 you can install both by selecting the "Mobile development with .NET" workload.
In Visual Studio 2022 for Mac you can install the iOS and Android workload separately.

Only Visual Studio 2022 or Visual Studio 2022 for Mac are supported in those contexts.

Mobile development is not possible from Linux.
