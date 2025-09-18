# MonoGame Development Requirements

Depending on the [platform](https://docs.monogame.net/articles/getting_started/platforms.html) that you are targeting, MonoGame has different sets of requirements.

For desktop platforms
====================

MonoGame requires a .NET 8 SDK installation.
You can either install it [independently](https://dotnet.microsoft.com/download/dotnet), or by selecting the ".NET desketop development" workload when installing Visual Studio 2022 (version 17.8 and up required).

If you are targeting WindowsDX, you are also going to need [the DirectX June 2010 runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109) for audio and gamepads to work properly.

When it comes to IDE, [Visual Studio 2022](https://visualstudio.microsoft.com/vs/), [Visual Studio Code](https://code.visualstudio.com/), and [Visual Studio 2022 for Mac](https://visualstudio.microsoft.com/vs/mac/preview/) are supported (alternatively, you can work directly from the CLI with your code editor of choice). [JetBrains Rider](https://www.jetbrains.com/rider/) should work but isn't officially supported.

Desktop development is possible from any operating system supporting the above mentioned software.

::: info UWP Deprecation
The **UWP** platform is being deprecated as of the `3.8.2` release due to Microsoft reducing/removing support for this deployment mechanism.
:::

For mobile platforms
====================

MonoGame requires the dotnet workloads for iOS or Android depending on the target.

In Visual Studio 2022 you can install both by selecting the ".NET Multi-platform App UI Development" workload.
For VSCode, Rider or CLI development, you will need to manually install the [DotNet SDK](https://dotnet.microsoft.com/en-us/download) and then install the iOS and/or Android workloads using `dotnet workload install maui ios android` (delete either iOS or ANdroid if you are not intending to use).

> **MAUI** is required for either mobile platform as it includes debugging tools needed for mobile applications.

Visual Studio 2022, VSCode and Rider are supported in those contexts.

Mobile development is not possible from Linux.

For Modern platforms (Vulkan / DX12)
====================================

With the addition of the new development platforms, there are additional dependencies required to support these targets, namely:

* The [Vulkan SDK](https://vulkan.lunarg.com/) required for using Vulkan targets. (Make sure to run the setup script `setup_env.sh` on Mac/Linux)
* The [Java SDK](https://www.oracle.com/java/technologies/downloads/), min version 17.
* DirectX 12 (Windows Only) should be installed by default, [you can check here](https://support.microsoft.com/en-us/topic/how-to-install-the-latest-version-of-directx-d1f5ffa5-dae2-246c-91b1-ee1e973ed8c2).

Building from Source
====================

If you are intending to build MonoGame from source, there are a few extra build dependencies required:

* The [Premake5](https://premake.github.io/download/) executable, downloaded and added to your machines "Path".

> [!IMPORTANT]
> Make sure to download the "Latest" Premake 5 (`5.0.0-beta7` at time of writing) as earlier versions of Premake will not work.

* (Macos / Linux) Wine (for shader compilation) which can be downloaded from [WineHQ](https://www.winehq.org/) - [See here for more details](https://docs.monogame.net/articles/getting_started/1_setting_up_your_os_for_development_macos.html?tabs=android#setup-wine-for-effect-compilation)
