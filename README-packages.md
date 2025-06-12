# MonoGame

![MonoGame](https://raw.githubusercontent.com/MonoGame/MonoGame.Logo/refs/heads/master/FullColorOnLight/LogoOnly_128px.png)

 [![Join the chat at https://discord.gg/monogame](https://img.shields.io/discord/355231098122272778?style=flat-square&color=%237289DA&label=Discord%20server&logo=discord&logoColor=white)](https://discord.gg/monogame) 
 ![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/monogame/monogame/main.yml?style=flat-square)
 [![Donate](https://img.shields.io/badge/donate-F1465A?style=flat-square&logo=monogame&logoColor=FFFFFF)](https://monogame.net/donate/) 

 One framework for creating powerful cross-platform games

[Supported Platforms](#supported-platforms) • 
[Resources](#resources) • 
[Samples](#samples) • 
[Support and Contributions](#support-and-contributions) • 
[Source Code](#source-code) • 
[Helpful Links](#helpful-links) • 
[License](#license)

## Overview

**MonoGame** is a simple and powerful .NET framework for creating games for desktop PCs, video game consoles, and mobile devices using the C# programming language. It has been successfully used to create games such as [Streets of Rage 4](https://store.steampowered.com/app/985890/Streets_of_Rage_4/), [Carrion](https://store.steampowered.com/app/953490/CARRION/), [Celeste](https://store.steampowered.com/app/504230/Celeste/), [Stardew Valley](https://store.steampowered.com/app/413150/Stardew_Valley/), and [many others](https://monogame.net/showcase/). 

It is an open-source re-implementation of the discontinued [Microsoft's XNA Framework](https://msdn.microsoft.com/en-us/library/bb200104.aspx).

## Supported Platforms

We support a growing list of platforms across the desktop, mobile, and console space. If there is a platform we do not support, please [make a request](https://github.com/MonoGame/MonoGame/issues) or [come help us](CONTRIBUTING.md) add it.

* Desktop PCs
  * Windows 8.1 and up (OpenGL & DirectX)
  * Linux (OpenGL)
  * macOS 10.15 and up (OpenGL)
* Mobile/Tablet Devices
  * Android 6.0 and up (OpenGL)
  * iPhone/iPad 10.0 and up (OpenGL)
* [Consoles (for registered developers)](https://docs.monogame.net/articles/console_access.html)
  * PlayStation 4
  * PlayStation 5
  * Xbox One (XDK only) (GDK coming soon)
  * Nintendo Switch

## Resources

* [Getting started →](https://docs.monogame.net/articles/tutorials/building_2d_games/)
* ["How To" Guides →](https://docs.monogame.net/articles/getting_to_know/howto/)
* [Documentation Hub →](https://docs.monogame.net/)
* [API Reference →](https://docs.monogame.net/api/index.html)
* [Community Tutorials →](https://docs.monogame.net/articles/tutorials/)

## Samples

Check out the awesome [game samples](https://github.com/MonoGame/MonoGame.Samples) maintained by the MonoGame team:

|[Platformer 2D Sample](https://github.com/MonoGame/MonoGame.Samples/blob/3.8.2/Platformer2D/README.md) | [NeonShooter](https://github.com/MonoGame/MonoGame.Samples/blob/3.8.2/NeonShooter/README.md)|
|-|-|
|Supported on all platforms | Supported on all platforms |
|[![Platformer 2D Sample](https://raw.githubusercontent.com/MonoGame/MonoGame.Samples/refs/heads/3.8.2/Images/Platformer2D-Sample.png)](https://github.com/MonoGame/MonoGame.Samples/blob/3.8.2/Platformer2D/README.md) | [![NeonShooter Sample](https://raw.githubusercontent.com/MonoGame/MonoGame.Samples/refs/heads/3.8.2/Images/NeonShooter-Sample.png)](https://github.com/MonoGame/MonoGame.Samples/blob/3.8.2/NeonShooter/README.md) |
|The [Platformer 2D](https://github.com/MonoGame/MonoGame.Samples/blob/3.8.2/Platformer2D/README.md) sample is a basic 2D platformer pulled from the original XNA samples and upgraded for MonoGame.| [Neon Shooter](https://github.com/MonoGame/MonoGame.Samples/blob/3.8.2/NeonShooter/README.md) Is a graphically intensive twin-stick shooter with particle effects and save data from Michael Hoffman |
|||

| [Auto Pong Sample](https://github.com/MonoGame/MonoGame.Samples/blob/3.8.2/AutoPong/README.md) | [Ship Game 3D](https://github.com/MonoGame/MonoGame.Samples/blob/3.8.2/ShipGame/README.md) |
|-|-|
| Supported on all platforms | GL / DX / iOS / Android |
| [![Auto Pong Sample](https://raw.githubusercontent.com/MonoGame/MonoGame.Samples/refs/heads/3.8.2/Images/AutoPong_1.gif)](https://github.com/MonoGame/MonoGame.Samples/blob/3.8.2/AutoPong/README.md) | [![ShipGame 3D Sample](https://raw.githubusercontent.com/MonoGame/MonoGame.Samples/refs/heads/3.8.2/Images/ShipGame.png)](https://github.com/MonoGame/MonoGame.Samples/blob/3.8.2/ShipGame/README.md) |
| A short [sample project](https://github.com/MonoGame/MonoGame.Samples/blob/3.8.2/AutoPong/README.md) showing you how to make the classic game of pong, with generated soundfx, in 300 lines of code. | 3D Ship Game (Descent clone) sample, pulled from the XNA archives and updated for MonoGame |
|||

## Support and Contributions

If you think you have found a bug or have a feature request, use our [issue tracker](https://github.com/MonoGame/MonoGame/issues). Before opening a new issue, please search to see if your problem has already been reported. Try to be as detailed as possible in your issue reports.

If you need help using MonoGame or have other questions we suggest you post on [GitHub discussions](https://github.com/MonoGame/MonoGame/discussions) page or [Discord server](https://discord.gg/monogame). Please do not use the issue tracker for personal support requests.

If you are interested in contributing fixes or features to MonoGame, please read our [contributors guide](CONTRIBUTING.md) first.

## Subscription

If you would like to help the project by supporting us financially, consider supporting us via a subscription for the price of a monthly coffee.

Money goes towards hosting, new hardware and if enough people subscribe a dedicated developer.

There are several options on our [Donation Page](https://monogame.net/donate/).

## Source Code

The full source code is available here from GitHub:

* Clone the source: `git clone https://github.com/MonoGame/MonoGame.git`
* Set up the submodules: `git submodule update --init`
* Open the solution for your target platform to build the game framework.
* Open the Tools solution for your development platform to build the pipeline and content tools.

For the prerequisites for building from source, please look at the [Requirements](REQUIREMENTS.md) file.

A high level breakdown of the components of the framework:

* The game framework is found in [MonoGame.Framework](MonoGame.Framework).
* The content pipeline is located in [MonoGame.Framework.Content.Pipeline](MonoGame.Framework.Content.Pipeline).
* Project templates are in [Templates](Templates).
* See [Tests](Tests) for the framework unit tests.
* See [Tools/Tests](Tools/MonoGame.Tools.Tests) for the content pipeline and other tool tests.
* The [mgcb](Tools/MonoGame.Content.Builder) is a command line tool for content processing.
* The [mgfxc](Tools/MonoGame.Effect.Compiler) is a command line effect compiler tool.
* The [mgcb-editor](Tools/MonoGame.Content.Builder.Editor) tool is a GUI frontend for content processing.

## Helpful Links

* The official website is [monogame.net](http://www.monogame.net).
* Our [issue tracker](https://github.com/MonoGame/MonoGame/issues) is on GitHub.
* You can [join the Discord server](https://discord.gg/monogame) and chat live with the core developers and other users.
* The [official documentation](https://docs.monogame.net/articles/index.html) is on our website.
* Download [release](https://github.com/MonoGame/MonoGame/releases) and [development](https://github.com/orgs/MonoGame/packages) packages.
* Follow [@MonoGameTeam](https://twitter.com/monogameteam) on Twitter.
* Get premium content on [Patreon](https://www.patreon.com/bePatron?u=3142012) (coming soon)

## License

The MonoGame project is under the [Microsoft Public License](https://opensource.org/licenses/MS-PL) except for a few portions of the code. See the [LICENSE.txt](LICENSE.txt) file for more details. Third-party libraries used by MonoGame are under their own licenses. Please refer to those libraries for details on the license they use.
