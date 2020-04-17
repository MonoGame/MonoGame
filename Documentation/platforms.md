# Target Platforms

MonoGame supports the following systems:

- Windows
- Mac
- Linux
- Android
- iOS
- PlayStation 4
- PlayStation Vita
- Xbox One
- Nintendo Switch
- Google Stadia

There are different implementations of MonoGame that we call target platforms (or just platforms).
The platforms mostly correspond to the systems MonoGame supports but some platforms
support multiple systems. Platforms for systems that require registration such as PlayStation
and Xbox are not publicly available. To get access to those platforms, contact Tom Spilman
at ???. MonoGame documentation for closed platforms is available in their respective repositories.

Below is a list of public platforms with the corresponding NuGet package, the `dotnet new` template identifier
and an explanation of the platform.

- [WindowsDX](#windowsdx)
- [DesktopGL](#desktopgl)
- [Windows Universal](#windowsuniversal)
- [Android](#android)
- [iOS](#ios)

## WindowsDX

**Supported Systems**: Windows
**NuGet Package**: MonoGame.Framework.WindowsDX
**Template ID**: mgwindowdx

WindowsDX uses WinForms to manage the game window. DirectX (9.0c or newer) is used for graphics
and XAudio is used for audio. You can target Windows Vista or up with this platform.

## DesktopGL

**Supported Systems**: Windows, Mac, Linux
**NuGet Package**: MonoGame.Framework.DesktopGL
**Template ID**: mgdesktopgl

DesktopGL uses SDL for windowing. OpenGL is used for graphics and
OpenAL Soft for audio. DesktopGL supports Windows (Vista or up),
Mac (version ???) and Linux.

DesktopGL requires at least OpenGL 2.0 with the ARB_framebuffer_object extension.

DesktopGL currently does not have a `VideoPlayer` implementation.

## WindowsUniversal

**Supported Systems**: Windows 10, Xbox One
**NuGet Package**: MonoGame.Framework.WindowsUniversal
**Template ID**: mguwpcore, mguwpxaml

The WindowsUniversal platform runs on [UWP](https://docs.microsoft.com/en-us/windows/uwp/get-started/universal-application-platform-guide).
WindowsUniversal uses DirectX for graphics and XAudio for audio, just like the WindowsDX platform.

WindowsUniversal games can be tested on your Xbox one and published for the Xbox One without approval through the
[Xbox Live Creators Program](https://www.xbox.com/en-US/developers/creators-program).
UWP games get [restricted access](https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/system-resource-allocation)
to the Xbox One capabilities. MonoGame has a dedicated Xbox One platform for registered [ID@Xbox](https://www.xbox.com/en-US/Developers/id)
developers.

## Android

**Supported Systems**: Android
**NuGet Package**:  MonoGame.Framework.Android
**Template ID**: mgandroid

The Android platform uses [Xamarin.Android](https://docs.microsoft.com/en-us/xamarin/android/).
OpenGL is used for graphics.

## iOS

**Supported Systems**: iOS
**NuGet Package**:  MonoGame.Framework.iOS
**Template ID**: mgios

The iOS platform uses [Xamarin.iOS](https://docs.microsoft.com/en-us/xamarin/ios/).
OpenGL is used for graphics.
