# Contributing to MonoGame

We're happy that you have chosen to contribute to the MonoGame project.

You are joining a group of hundreds of volunteers that have helped build MonoGame since 2009.  To organize these efforts the MonoGame Team has written this simple guide to help you.

Please read this document completely before contributing.


## How To Contribute

MonoGame has a `master` branch for stable releases and a `develop` branch for daily development.  New features and fixes are always submitted to the `develop` branch.

If you are looking for ways to help you should start by looking at the [Help Wanted tasks](https://github.com/mono/MonoGame/issues?q=is%3Aissue+is%3Aopen+label%3A%22Help+Wanted%22).  Please let us know if you plan to work on an issue so that others are not duplicating work.

The MonoGame project follows standard [GitHub flow](https://guides.github.com/introduction/flow/index.html).  You should learn and be familiar with how to [use Git](https://help.github.com/articles/set-up-git/), how to [create a fork of MonoGame](https://help.github.com/articles/fork-a-repo/), and how to [submit a Pull Request](https://help.github.com/articles/using-pull-requests/).

After you submit a PR the [MonoGame build server](http://teamcity.monogame.net/?guest=1) will build your changes and verify all tests pass.  Project maintainers and contributors will review your changes and provide constructive feedback to improve your submission.

Once satisfied that your changes are good for MonoGame we will merge it.


## Quick Guidelines

Here are a few simple rules and suggestions to remember when contributing to MonoGame.

* :bangbang: **NEVER** commit code that you didn't personally write.
* :bangbang: **NEVER** use decompiler tools to steal code and submit them as your own work.
* :bangbang: **NEVER** decompile XNA assemblies and steal Microsoft's copyrighted code.
* **PLEASE** try keep your PRs focused on a single topic and of a reasonable size or we may ask you to break it up.
* **PLEASE** be sure to write simple and descriptive commit messages.
* **DO NOT** surprise us with new APIs or big new features. Open an issue to discuss your ideas first.
* **DO NOT** reorder type members as it makes it difficult to compare code changes in a PR.
* **DO** try to follow our [coding style](https://github.com/mono/MonoGame/wiki/Coding-Guidelines) for new code.
* **DO** give priority to the existing style of the file you're changing.
* **DO** try to add to our [unit tests](Test) when adding new features or fixing bugs.
* **DO NOT** send PRs for code style changes or make code changes just for the sake of style.
* **PLEASE** keep a civil and respectful tone when discussing and reviewing contributions.
* **PLEASE** tell others about MonoGame and your contributions via social media.


## Decompiler Tools

We prohibit tools like dotPeek, ILSpy, JustDecompiler, or .NET Reflector which convert compiled assemblies into readable code.

There has been confusion on this point in the past, so we want to make this clear.  It is **NEVER ACCEPTABLE** to decompile copyrighted assemblies and submit that code to the MonoGame project.

* It **DOES NOT** matter how much you change the code.
* It **DOES NOT** matter what country you live in or what your local laws say.  
* It **DOES NOT** matter that XNA is discontinued.  
* It **DOES NOT** matter how small the bit of code you have stolen is.  
* It **DOES NOT** matter what your opinion is of stealing code.

If you did not write the code, you do not have ownership of the code, and you shouldn't submit it to MonoGame.

If we find a contribution in violation of copyright it will be immediately removed.  We will bar that contributor from the MonoGame project.


## Licensing

The MonoGame project is under the [Microsoft Public License](https://opensource.org/licenses/MS-PL) except for a few portions of the code.  See the [LICENSE.txt](LICENSE.txt) file for more details.  Third-party libraries used by MonoGame are under their own licenses.  Please refer to those libraries for details on the license they use.

We accept contributions in "good faith" that it isn't bound to a conflicting license.  By submitting a PR you agree to distribute your work under the MonoGame license and copyright.

To this end when submitting new files include the following in the header if appropriate:
```csharp
// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
```

## Need More Help?

If you need help please ask questions on our [community forums](http://community.monogame.net/) or come [chat on Gitter](https://gitter.im/mono/MonoGame).


Thanks for reading this guide and helping make MonoGame great!

 :heart: The MonoGame Team
