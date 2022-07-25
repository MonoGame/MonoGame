# Supported Platforms

MonoGame supports building games for the following **systems**:

| **Desktop PCs**             | **Mobiles**                | **Gaming consoles***                                                           |
| --------------------------- | -------------------------- | ------------------------------------------------------------------------------ |
| Windows<br/>macOS<br/>Linux | iOS<br/>iPadOS<br/>Android | Xbox<br/>PlayStation 4<br/>PlayStation 5<br/>Nintendo Switch<br/>Google Stadia |

**Gaming consoles are restricted to registered developers and are not publicly available nor publicly documented. To get access to those platforms, please contact your console account manager(s). MonoGame documentation for closed platforms is available in their respective repositories.*

## Understanding MonoGame's platform types

There are different implementations of MonoGame that we call **target platforms** (or just **platforms**).

The platforms mostly correspond to the systems MonoGame supports but some platforms support multiple systems. For instance, the *DesktopGL* platform can be used to build games that will run either on Windows, macOS, or Linux with the same base code and project.

Each platform comes with its own project template that you can choose when starting a project.

Below is a list of public platforms with their corresponding NuGet package, the `dotnet new` template identifier, and an explanation of the platform. 

- [WindowsDX](#windowsdx)
- [DesktopGL](#desktopgl)
- [Windows Universal](#windowsuniversal)
- [Android](#android)
- [iOS](#ios)

Beside these target platforms, MonoGame provides additional templates for shared game logic and extensions to the MonoGame Content Pipeline that can be used across all platforms.

- [.NET Standard Library](#net-standard-library)
- [Shared Project](#shared-project)
- [Content Pipeline Extension](#content-pipeline-extension)

## Platform details

### WindowsDX

| **Supported Systems** | **NuGet Package**            | **Template ID** |
| --------------------- | ---------------------------- | --------------- |
| Windows               | MonoGame.Framework.WindowsDX | mgwindowsdx     |

WindowsDX uses WinForms to manage the game window, **DirectX** (9.0c or newer) is used for graphics, and XAudio is used for audio.

You can target **Windows*** 8.1 and up with this platform.

WindowsDX requires the [DirectX June 2010](https://www.microsoft.com/en-us/download/details.aspx?id=8109) runtime to both build and run games. Make sure that your players have it installed (otherwise you might be missing sound and gamepad rumble support).

### DesktopGL

| **Supported Systems** | **NuGet Package**            | **Template ID** |
| --------------------- | ---------------------------- | --------------- |
| Windows, macOS, Linux | MonoGame.Framework.DesktopGL | mgdesktopgl     |

DesktopGL uses SDL for windowing, **OpenGL** for graphics, and OpenAL-Soft for audio. 

DesktopGL supports **Windows** (8.1 and up), **macOS** (Catalina 10.15 and up) and **Linux** (64bit-only).

DesktopGL requires at least OpenGL 2.0 with the ARB_framebuffer_object extension (or alternatively at least OpenGL 3.0).

DesktopGL is a convenient way to publish builds for Windows, macOS, and Linux from a single project and source code. It also allows to cross-compile any build from any of these operating systems (e.g. you can build a Linux game from Windows).

You can target Windows 8.1 (and up), macOS Catalina 10.15 (and up), and Linux with this platform.

DesktopGL currently does not have a `VideoPlayer` implementation.

### WindowsUniversal

| **Supported Systems**                | **NuGet Package**                   | **Template ID**                                     |
| ------------------------------------ | ----------------------------------- | --------------------------------------------------- |
| Windows 10, Xbox (UWP-only, not XDK) | MonoGame.Framework.WindowsUniversal | mguwpcore (core app, no xaml), mguwpxaml (xaml app) |

The WindowsUniversal platform runs on [Universal Windows Platform (UWP)](https://docs.microsoft.com/en-us/windows/uwp/get-started/universal-application-platform-guide).

WindowsUniversal uses **DirectX** for graphics, and XAudio for audio just like the WindowsDX platform.

UWP comes in two flavors, each with its own project template:

- **XAML app template**: an app in which your game will be hosted within an XAML page. This can be useful if you wish to offer a more complex UWP experience with multiple pages or XAML controls.

- **Core app template**: a raw app without any XAML, more straightforward if you don't need XAML controls.

This platform is meant to publish games on the **Windows Store**, for both **Windows** and **Xbox** (through the [Xbox Live Creators Program](https://www.xbox.com/en-US/developers/creators-program)).

Note that UWP games running on Xbox get [restricted access](https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/system-resource-allocation) to the console capabilities. To unlock those restrictions, MonoGame has a dedicated Xbox platform for registered [ID@Xbox](https://www.xbox.com/en-US/Developers/id) developers targeting the XDK (this platform is private and requires you to contact your ID@Xbox manager).

Building for UWP requires the Windows SDK version 19041 or better to be installed.

### Android

| **Supported Systems** | **NuGet Package**          | **Template ID** |
| --------------------- | -------------------------- | --------------- |
| Android               | MonoGame.Framework.Android | mgandroid       |

The Android platform uses [Xamarin.Android](https://docs.microsoft.com/en-us/xamarin/android/). **OpenGL** is used for graphics, and OpenAL for audio.

Building for Android requires the .NET Xamarin component to be installed. You can install it with the Visual Studio installer (if you're using Visual Studio) or with the CLI command ```dotnet workload install android``` (if you're working with Rider, VS Code, or the CLI).

Building for Android also requires the Java 11 JDK (we recommand that you use [the Microsoft's distribution](https://docs.microsoft.com/en-us/java/openjdk/download#openjdk-11)) as well as the Android SDK 31.

### iOS

| **Supported Systems** | **NuGet Package**      | **Template ID** |
| --------------------- | ---------------------- | --------------- |
| iOS, iPadOS           | MonoGame.Framework.iOS | mgios           |

The iOS platform uses [Xamarin.iOS](https://docs.microsoft.com/en-us/xamarin/ios/). **OpenGL** is used for graphics, and OpenAL for audio.

Building for Android requires the .NET Xamarin component to be installed. You can install it with the Visual Studio installer (if you're using Visual Studio) or with the CLI command `dotnet workload install ios` (if you're working with Rider, VS Code, or the CLI).

The latest version of Xcode will also be required.

You can test and deploy an iOS game on Windows by [pairing your Visual Studio 2022 with a mac on your local network](https://docs.microsoft.com/en-us/xamarin/ios/get-started/installation/windows/connecting-to-mac/). This feature is not avaible for Rider, Visual Studio Code, or the CLI.

## Other templates

### .NET Standard Library

**Template ID**: mgnetstandard

A project template to create [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) libraries to distribute code through a DLL. This can be used to redistribute libraries or to share code between multiple projects (like different platforms).

### Shared Project

**Template ID**: mgshared

A project template to create a [shared project](https://docs.microsoft.com/en-us/xamarin/cross-platform/app-fundamentals/shared-projects) which can be used to share code between multiple other projects. The difference with .NET Standard libraries is that shared projects don't produce an intermediate DLL and the code is directly shared and built into the other projects it reference.

### Content Pipeline Extension

**Template ID**: mgpipeline

A project template for writing custom logic for handling content and building it into XNB files.

# 
