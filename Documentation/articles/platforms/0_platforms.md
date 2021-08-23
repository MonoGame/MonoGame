# Platforms

MonoGame supports the following systems:

- Windows
- Mac
- Linux
- Android
- iOS
- PlayStation 4
- Xbox One
- Nintendo Switch
- Google Stadia

There are different implementations of MonoGame that we call target platforms (or just platforms).
The platforms mostly correspond to the systems MonoGame supports but some platforms support multiple systems. Platforms for systems that require registration such as PlayStation and Xbox are not publicly available. To get access to those platforms, please contact your console account manager(s). MonoGame documentation for closed platforms is available in their respective repositories.

Below is a list of public platforms with the corresponding NuGet package, the `dotnet new` template identifier and an explanation of the platform.

- [WindowsDX](#windowsdx)
- [DesktopGL](#desktopgl)
- [Windows Universal](#windowsuniversal)
- [Android](#android)
- [iOS](#ios)

MonoGame provides additional templates for shared game logic and extensions to the MonoGame Content Pipeline.

- [.NET Standard Library](#net-standard-library)
- [Shared Project](#shared-project)
- [Content Pipeline Extension](#content-pipeline-extension)

## Platform details

### WindowsDX

|**Supported Systems**|**NuGet Package**|**Template ID**|
|-|-|-|
| Windows | MonoGame.Framework.WindowsDX | mgwindowsdx |

WindowsDX uses WinForms to manage the game window, DirectX (9.0c or newer) is used for graphics, and XAudio is used for audio. You can target Windows Vista and up with this platform.

### DesktopGL

|**Supported Systems**|**NuGet Package**|**Template ID**|
|-|-|-|
| Windows, macOS, Linux | MonoGame.Framework.DesktopGL | mgdesktopgl |

DesktopGL uses SDL for windowing. OpenGL is used for graphics, and OpenAL-Soft for audio. DesktopGL supports Windows (Vista and up), macOS (High Sierra 10.13 and up) and Linux (64bit-only).

DesktopGL requires at least OpenGL 2.0 with the ARB_framebuffer_object extension (or alternatively at least OpenGL 3.0).

DesktopGL is a convenient way to publish builds for Windows, macOS, and Linux from a single project and source code. It also allows to cross-compile any build from any of these operating systems.

DesktopGL currently does not have a `VideoPlayer` implementation.

### WindowsUniversal

|**Supported Systems**|**NuGet Package**|**Template ID**|
|-|-|-|
| Windows 10, Xbox One (UWP-only, not XDK) | MonoGame.Framework.WindowsUniversal | mguwpcore (core app, no xaml), mguwpxaml (xaml app) |

The WindowsUniversal platform runs on [Universal Windows Platform (UWP)](https://docs.microsoft.com/en-us/windows/uwp/get-started/universal-application-platform-guide).
WindowsUniversal uses DirectX for graphics, and XAudio for audio just like the WindowsDX platform.

This platform is meant to publish games on the Windows Store, for both Windows and Xbox One (through the [Xbox Live Creators Program](https://www.xbox.com/en-US/developers/creators-program)).

Note that UWP games running on Xbox One get [restricted access](https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/system-resource-allocation) to the console capabilities. To unlock those restrictions, MonoGame has a dedicated Xbox One platform for registered [ID@Xbox](https://www.xbox.com/en-US/Developers/id) developers targeting the XDK (this platform is private and requires you to contact your ID@Xbox manager).

### Android

|**Supported Systems**|**NuGet Package**|**Template ID**|
|-|-|-|
|Android | MonoGame.Framework.Android | mgandroid |

The Android platform uses [Xamarin.Android](https://docs.microsoft.com/en-us/xamarin/android/).
OpenGL is used for graphics, and OpenAL for audio.

### iOS

|**Supported Systems**|**NuGet Package**|**Template ID**|
|-|-|-|
| iOS | MonoGame.Framework.iOS | mgios |

The iOS platform uses [Xamarin.iOS](https://docs.microsoft.com/en-us/xamarin/ios/).
OpenGL is used for graphics, and OpenAL for audio.

## Other templates

### .NET Standard Library

**Template ID**: mgnetstandard

A project template with a [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) project to share game logic for cross-platform development.

### Shared Project

**Template ID**: mgshared

A project template with a [shared project](https://docs.microsoft.com/en-us/xamarin/cross-platform/app-fundamentals/shared-projects) library to support cross-platform development.

### Content Pipeline Extension

**Template ID**: mgpipeline

A project template for writing custom logic for handling content and building it into XNB files.

## Deprecated platforms

### PS Vita

MonoGame used to support targeting the PS Vita through the official SDK. The platform has been deprecated since Sony has ended all support for the console and closed new game/patch applications.

For reference, you can visit this [GitHub tag](https://github.com/MonoGame/MonoGame/tree/last_psvita) which points to the latest supported branch. However, the public repository only contains links and references. The actual implementation is private and limited to Sony's registered developers.

Please note that this deprecated support is not an homebrew support and can't be used for that purpose per Sony's term of use of the official SDK. It can't be made public either.
