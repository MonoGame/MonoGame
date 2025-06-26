# Contributing to MonoGame

We are happy that you have chosen to contribute to the MonoGame project.

You are joining a group of hundreds of volunteers who have helped build MonoGame since 2009.  To organize these efforts, the MonoGame Team has written this simple guide to help you.

Please read this document completely before contributing.

## How To Contribute

MonoGame has a `master` branch for stable releases and a `develop` branch for daily development.  New features and fixes are always submitted to the `develop` branch.

If you are looking for ways to help, you should start by looking at the [Help Wanted tasks](https://github.com/mono/MonoGame/issues?q=is%3Aissue+is%3Aopen+label%3A%22Help+Wanted%22).  Please let us know if you plan to work on an issue so that others are not duplicating your work.

> !! An Issue should proceed any PR to outline the change (Unless it is minor or a documentation update), whether it is an actual bug report, a proposal for correction, or additional functionality.

The MonoGame project follows standard [GitHub flow](https://guides.github.com/introduction/flow/index.html).  You should learn and be familiar with how to [use Git](https://help.github.com/articles/set-up-git/), how to [create a fork of MonoGame](https://help.github.com/articles/fork-a-repo/), and how to [submit a Pull Request](https://help.github.com/articles/using-pull-requests/).

After you submit a PR, the [MonoGame build server](http://teamcity.monogame.net/?guest=1) will build your changes and verify that all tests pass.  Project maintainers and contributors will review your changes and provide constructive feedback to improve your submission.

Once we are satisfied that your changes are good for MonoGame, we will merge your PR.

## Quick Guidelines

Here are a few simple rules and suggestions to remember when contributing to MonoGame.

* :bangbang: **NEVER** commit code that you did not personally write.
* :bangbang: **NEVER** use decompiler tools to steal code and submit it as your own work.
* :bangbang: **NEVER** decompile XNA assemblies and steal Microsoft's copyrighted code.
* **PLEASE** try to keep your PRs focused on a single topic and of a reasonable size or we may ask you to break it up.
* **PLEASE** be sure to write simple and descriptive commit messages.
* **DO NOT** surprise us with new APIs or big new features. Open an issue to discuss your ideas first.
* **DO NOT** reorder type members as it makes it difficult to compare code changes in a PR.
* **DO** try to follow our [coding style](CODESTYLE.md) for new code.
* **DO** give priority to the existing style of the file you are changing.
* **DO** try to add to our [unit tests](Tests) when adding new features or fixing bugs.
* **DO NOT** send PRs for code style changes or make code changes just for the sake of style.
* **PLEASE** keep a civil and respectful tone when discussing and reviewing contributions.
* **PLEASE** tell others about MonoGame and your contributions via social media.

## Extended guidelines

The current focus of the MonoGame Framework is to preserve the XNA API (at least for the 3.x and 4.x releases), and while we welcome contribution, the last thing we want is wasted effort that breaks or changes this standard. To this end, we want to clarify acceptable contributions:

* Changes that introduce new features should be preceded by a logged issue discussing the change.
* While changes to documentation that do not alter the functionality are welcome, changes that **only** update formatting/styling or modernize code will be rejected unless it improves the performance or we have previously agreed upon the change in an issue.
* Breaking changes should be pre-agreed through an issue log detailing specifically why it is critical. By Design, breaking changes should be scheduled for future major releases, or ideally for MonoGame 5.0 where the API is intentionally going to change.

There will always be exceptions to the rules above, but these must be pre-agreed and discussed with the wider community of MonoGame as we are all in this together.

## Decompiler Tools

We prohibit the use of tools like dotPeek, ILSpy, JustDecompiler, or .NET Reflector which convert compiled assemblies into readable code.

There has been confusion on this point in the past, so we want to make this clear.  It is **NEVER ACCEPTABLE** to decompile copyrighted assemblies and submit that code to the MonoGame project.

* It **DOES NOT** matter how much you change the code.
* It **DOES NOT** matter what country you live in or what your local laws say.
* It **DOES NOT** matter that XNA is discontinued.
* It **DOES NOT** matter how small the bit of code you have stolen is.
* It **DOES NOT** matter what your opinion of stealing code is.

If you did not write the code, you do not have ownership of the code and you should not submit it to MonoGame.

If we find a contribution to be in violation of copyright, it will be immediately removed.  We will bar that contributor from the MonoGame project.

## Code guidelines

Due to limitations on private target platforms, MonoGame enforces the use of C# 5.0 features.

It is however allowed to use the latest class library, but if contributions make use of classes that are not present in .NET 4.5, it will be required from the contribution to implement backward-compatible switches.

> These limitations will be lifted at some point in the near future.

## Licensing

The MonoGame project is under the [Microsoft Public License](https://opensource.org/licenses/MS-PL) except for a few portions of the code.  See the [LICENSE.txt](LICENSE.txt) file for more details.  Third-party libraries used by MonoGame are under their own licenses.  Please refer to those libraries for details on the licenses they use.

We accept contributions in "good faith" that it is not bound to a conflicting license.  By submitting a PR you agree to distribute your work under the MonoGame license and copyright.

To this end, when submitting new files, include the following in the header if appropriate:

```csharp
// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
```

## Need More Help?

If you need help, please ask questions on our [GitHub Discussion board](https://github.com/MonoGame/MonoGame/discussions) or come [chat on Discord](https://discord.gg/monogame).

Thanks for reading this guide and helping make MonoGame great!

 :heart: MonoGame Foundation, Inc
