# MonoGame

MonoGame is a simple and powerful .NET framework for creating games for desktop PCs, video game consoles, and mobile devices using the C# programming language. It has been successfully used to create games such as [Streets of Rage 4](https://store.steampowered.com/app/985890/Streets_of_Rage_4/), [Carrion](https://store.steampowered.com/app/953490/CARRION/), [Celeste](https://store.steampowered.com/app/504230/Celeste/), [Stardew Valley](https://store.steampowered.com/app/413150/Stardew_Valley/), and [many others](https://monogame.net/showcase/).

It is an open-source re-implementation of the discontinued [Microsoft's XNA Framework](https://msdn.microsoft.com/en-us/library/bb200104.aspx).

[![Join the chat at https://discord.gg/monogame](https://img.shields.io/discord/355231098122272778?color=%237289DA&label=MonoGame&logo=discord&logoColor=white)](https://discord.gg/monogame)

* [Build Status](#build-status)
* [Supported Platforms](#supported-platforms)
* [Support and Contributions](#support-and-contributions)
* [Source Code](#source-code)
* [Helpful Links](#helpful-links)
* [License](#license)

## Build Status

We use [GitHub Actions](https://github.com/MonoGame/MonoGame/actions) to automate builds and packages distribution of the latest MonoGame changes. We also rely on a [build server](http://teamcity.monogame.net/?guest=1) to run tests in order to avoid regressions. The table below shows the current build status for the ```develop``` branch.

| Name | Status |
|:---- | ------ |
| Builds | [![Build](https://github.com/MonoGame/MonoGame/actions/workflows/main.yml/badge.svg?branch=develop)](https://github.com/MonoGame/MonoGame/actions/workflows/main.yml) |

## Supported Platforms

We support a growing list of platforms across the desktop, mobile, and console space. If there is a platform we don't support, please [make a request](https://github.com/MonoGame/MonoGame/issues) or [come help us](CONTRIBUTING.md) add it.

* Desktop PCs
  * Windows 8.1 and up (OpenGL & DirectX)
  * Linux (OpenGL)
  * macOS 10.15 and up (OpenGL)
* Mobile/Tablet Devices
  * Android 6.0 and up (OpenGL)
  * iPhone/iPad 10.0 and up (OpenGL)
* Consoles (for registered developers)
  * PlayStation 4
  * PlayStation 5
  * Xbox One (XDK only) (GDK coming soon)
  * Nintendo Switch
  * Google Stadia

## Support and Contributions

If you think you have found a bug or have a feature request, use our [issue tracker](https://github.com/MonoGame/MonoGame/issues). Before opening a new issue, please search to see if your problem has already been reported. Try to be as detailed as possible in your issue reports.

If you need help using MonoGame or have other questions we suggest you post on our [community forums](http://community.monogame.net). Please do not use the GitHub issue tracker for personal support requests.

If you are interested in contributing fixes or features to MonoGame, please read our [contributors guide](CONTRIBUTING.md) first.

### Subscription

If you'd like to help the project by supporting us financially, consider supporting us via a subscription for the price of a monthly coffee.

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
* [mgcb](Tools/MonoGame.Content.Builder) is the command line tool for content processing.
* [mgfxc](Tools/MonoGame.Effect.Compiler) is the command line effect compiler tool.
* The [mgcb-editor](Tools/MonoGame.Content.Builder.Editor) tool is a GUI frontend for content processing.

## Helpful Links

* The official website is [monogame.net](http://www.monogame.net).
* Our [issue tracker](https://github.com/MonoGame/MonoGame/issues) is on GitHub.
* Use our [community forums](http://community.monogame.net/) for support questions.
* You can [join the Discord server](https://discord.gg/monogame) and chat live with the core developers and other users.
* The [official documentation](https://docs.monogame.net/articles/index.html) is on our website.
* Download [release](https://github.com/MonoGame/MonoGame/releases) and [development](https://github.com/orgs/MonoGame/packages) packages.
* Follow [@MonoGameTeam](https://twitter.com/monogameteam) on Twitter.

## License

The MonoGame project is under the [Microsoft Public License](https://opensource.org/licenses/MS-PL) except for a few portions of the code. See the [LICENSE.txt](LICENSE.txt) file for more details. Third-party libraries used by MonoGame are under their own licenses. Please refer to those libraries for details on the license they use.
