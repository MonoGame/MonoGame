# MonoGame

One framework for creating powerful cross-platform games.  The spiritual successor to XNA with thousands of titles shipped across desktop, mobile, and console platforms.  [MonoGame](http://www.monogame.net/) is a fully managed .NET open source game framework without any black boxes.  Create, develop and distribute your games your way.

[![Join the chat at https://discord.gg/tsuucV4](https://img.shields.io/discord/355231098122272778?color=%237289DA&label=MonoGame&logo=discord&logoColor=white)](https://discord.gg/tsuucV4) [![Join the chat at https://gitter.im/MonoGame/MonoGame](https://badges.gitter.im/MonoGame/MonoGame.svg)](https://gitter.im/MonoGame/MonoGame?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

 * [Build Status](#build-status)
 * [Supported Platforms](#supported-platforms)
 * [Support and Contributions](#support-and-contributions)
 * [Source Code](#source-code)
 * [Helpful Links](#helpful-links)
 * [License](#license)
 

## Build Status

Our [build server](http://teamcity.monogame.net/?guest=1) builds, tests, and packages the latest MonoGame changes.  The table below shows the current build status for the develop branch.

| Name  | Status |
|:---|--------|
| Build Windows, Web, and Android | [![Build Status](http://teamcity.monogame.net/app/rest/builds/buildType:MonoGame_DevelopWin/statusIcon)](http://teamcity.monogame.net/viewType.html?buildTypeId=MonoGame_DevelopWin&guest=1) |
| Build Mac, iOS, and Linux | [![Build Status](http://teamcity.monogame.net/app/rest/builds/buildType:MonoGame_DevelopMac/statusIcon)](http://teamcity.monogame.net/viewType.html?buildTypeId=MonoGame_DevelopMac&guest=1) |
| Windows Tests | [![Build Status](http://teamcity.monogame.net/app/rest/builds/buildType:MonoGame_TestWindows/statusIcon)](http://teamcity.monogame.net/viewType.html?buildTypeId=MonoGame_TestWindows&guest=1) |
| Mac Tests | [![Build Status](http://teamcity.monogame.net/app/rest/builds/buildType:MonoGame_TestMac/statusIcon)](http://teamcity.monogame.net/viewType.html?buildTypeId=MonoGame_TestMac&guest=1) |


## Supported Platforms

We support a growing list of platforms across the desktop, mobile, and console space.  If there is a platform we don't support, please [make a request](https://github.com/MonoGame/MonoGame/issues) or [come help us](CONTRIBUTING.md) add it.

 * Desktop PCs
   * Windows 10 Store Apps (UWP)
   * Windows Win32 (OpenGL & DirectX)
   * Linux (OpenGL)
   * Mac OS X (OpenGL)
 * Mobile/Tablet Devices
   * Android (OpenGL)
   * iPhone/iPad (OpenGL)
   * Windows Phone 10 (UWP)
 * Consoles (for registered developers)
   * PlayStation 4
   * Xbox One (both UWP and XDK)
   * Nintendo Switch
   * Google Stadia
 * Other
   * tvOS (OpenGL)


## Support and Contributions

If you think you have found a bug or have a feature request, use our [issue tracker](https://github.com/MonoGame/MonoGame/issues). Before opening a new issue, please search to see if your problem has already been reported.  Try to be as detailed as possible in your issue reports.

If you need help using MonoGame or have other questions we suggest you post on our [community forums](http://community.monogame.net).  Please do not use the GitHub issue tracker for personal support requests.

If you are interested in contributing fixes or features to MonoGame, please read our [contributors guide](CONTRIBUTING.md) first.

### Subscription

If you'd like to help the project by supporting us financially, consider supporting us via a subscription for the price of a monthly coffee.

Money goes towards hosting, new hardware and if enough people subscribe a dedicated developer.

There are several options on our [Donation Page](http://www.monogame.net/donate/).


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
 * You can [chat live](https://gitter.im/mono/MonoGame?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) with the core developers and other users.
 * The [official documentation](http://www.monogame.net/documentation/) is on our website.
 * Download release and development [installers and packages](http://www.monogame.net/downloads/).
 * Follow [@MonoGameTeam](https://twitter.com/monogameteam) on Twitter.

## License

The MonoGame project is under the [Microsoft Public License](https://opensource.org/licenses/MS-PL) except for a few portions of the code.  See the [LICENSE.txt](LICENSE.txt) file for more details.  Third-party libraries used by MonoGame are under their own licenses.  Please refer to those libraries for details on the license they use.
