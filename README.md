# [MonoGame](http://www.monogame.net/) [![Build Status](http://teamcity.monogame.net/app/rest/builds/buildType:MonoGame_DevelopWin/statusIcon)](http://teamcity.monogame.net/project.html?projectId=MonoGame&guest=1)

[MonoGame](http://www.monogame.net/) is an open source implementation of the Microsoft XNA 4.x Framework.

Our goal is to make it easy for XNA developers to create cross-platform games with extremely high code reuse.

### Links

 * Forum can be found at: [community.monogame.net](http://community.monogame.net/)
 * Chat with the community at:&nbsp; [![Join the chat at https://gitter.im/mono/MonoGame](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/mono/MonoGame?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
 * Documentation is available at: [www.monogame.net/documentation/](http://www.monogame.net/documentation/)
 * [Download Binaries](http://www.monogame.net/downloads/).
 * Follow [@MonoGameTeam on Twitter](https://twitter.com/monogameteam).

### Supported Platforms

Desktop:
 * Windows Store Apps (8, 8.1 and 10)
 * Windows (OpenGL & DirectX)
 * Linux
 * Mac OS X

Mobile:
 * Android
 * iOS
 * Windows Phone (8, 8.1 and 10)

Console:
 * PlayStation 4 (contact your Sony account manager for access)
 * PlayStation Vita (contact your Sony account manager for access)
 * OUYA

### Source Code

Getting the source:
 * Clone the source: `git clone https://github.com/mono/MonoGame.git`
 * Run Protobuild.exe to generate project files and solutions.
   * If on Linux or Mac, run it with mono: `mono Protobuild.exe`

For pre-requirements for building please look at the [Requirements](REQUIREMENTS.md) file.

Component locations:
 * Content processors (including command line and GUI tools) are found in Windows/Mac/Linux solutions.
 * MonoDevelop addin is found in [IDE/MonoDevelop/](IDE/MonoDevelop)
 * Visual Studio templates are found in [ProjectTemplates/](ProjectTemplates)
 * NuGet packages are located in [NuGetPackages/](NuGetPackages)

You can generate solutions for platforms that are not buildable from the current OS with: `mono Protobuild.exe --generate $PLATFORM`.

### How to Engage, Contribute and Provide Feedback

If you have a bug or a feature request, [please open a new issue](https://github.com/mono/MonoGame/issues). Before opening any issue, please search for existing issues and read the [Issue Guidelines](https://github.com/necolas/issue-guidelines).

If you are interested in contributing code to MonoGame, please look at the [Contributing Guidelines](CONTRIBUTING.md). For the list of available tasks please look at [Help Wanted](https://github.com/mono/MonoGame/labels/Help%20Wanted) label.

### MonoGame Components

 * **MonoGame.Framework** - [Microsoft.Xna.Framework](https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.aspx) namespace components excluding .Net namespace.
 * **MonoGame.Framework.Net** - Contains [Microsoft.Xna.Framework.Net](https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.net.aspx) namespace components.
 * **MonoGame.Framework.Content.Pipeline** - MonoGame content processor.
 * **MonoGame.Tests** - MonoGame Framework NUnit tests.
 * **MGCB** - Command line tool for MonoGame content processing.
 * **Pipeline** - GUI tool for content processing.
 * **MonoDevelop.MonoGame.Addin** - MonoDevelop Addin.

### License

MonoGame is Licensed under the [Ms-PL and MIT Licenses](LICENSE.txt). Some third-party libraries used by MonoGame may be under a different license. Please refer to those libraries for details on the license they use.
