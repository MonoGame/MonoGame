Guidelines
==========

When contributing to the MonoGame project, please follow the [MonoGame Coding Guidelines][].  We are working to introduce this coding style to the project.  Please make your pull requests conform to these guidelines.

[MonoGame Coding Guidelines]: https://github.com/mono/MonoGame/wiki/Coding-Guidelines

Etiquette
=========

In general, we do not accept pull requests that merely shuffle code around, split classes in multiple files, reindent the code or are the result of running a refactoring tool on the source code.  This is done for three reasons:

* We have our own coding guidelines
* Some modules are imported from upstream sources and we want to respect their coding guidelines
* It destroys valuable history that is often used to investigate bugs, regressions and problems

License
=======

The MonoGame project uses the Microsoft Public License.  See `LICENSE.txt` for more details.  Some third-party libraries used by MonoGame may be under a different license.  Please refer to those libraries for details on the license they use.

Submitting Patches
==================

MonoGame consists of two primary branches:

* `master` is the stable branch from which releases are made.  Pull requests direct to the master branch will not be accepted.
* `develop` is the unstable development branch.  Pull requests must be made against the develop branch.

The process for making a pull request is generally as follows:

1. Make a feature branch from `develop` for the change.
2. Edit, build and test the feature.
3. Commit to your local repository.
4. Push the feature branch to your GitHub fork.
5. Create the pull request.

If you need to make changes to the pull request, simply repeat steps 2-4.  Adding commits to that feature branch in your fork will automatically add the change to the pull request.

The majority of code in MonoGame is cross-platform and must build and behave correctly on all supported platforms.  All pull requests to MonoGame are built using an automated system that compiles the pull request on all supported platforms and will report any build errors.

Once a pull request has been accepted, your feature branch can be deleted if desired.