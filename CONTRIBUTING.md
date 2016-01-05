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

Generating Documentation
========================
Monogame generates its documentation using the [SharpDoc library] (https://github.com/xoofx/SharpDoc)

You can view the documentation for your changes both locally, and (after you have submitted a PR) on the build server.

### Local documentation generation
Local documentation generation is not automatic. You have to run the SharpDoc tool to generate it for you.
The commandline for doing this is: `ThirdParty\Dependencies\SharpDoc\SharpDoc.exe -config <config file> -output <output directory>`
e.g. `ThirdParty\Dependencies\SharpDoc\SharpDoc.exe -config Documentation\config.xml -output GeneratedDocs`
Run this command from the root folder of your local MonoGame repository clone. It will generate all the documentation under `Documentation\<output directory>` folder. You can go ahead and start browsing from the `index.html` file.

**Gotchas:**
- This assumes that all platforms (Windows, Linux, Mac, etc.) have been built. If you can't (or don't want to) build for all of the platforms, you can comment out the relevant sections in `Documentation\config.xml`.
- By default, SharpDoc looks for the build artifacts in the Release folder. Remember to build for Release.
- Browsing local documentation seems to work best with Internet Explorer. When you open up `index.html`, you might be prompted to give permission to an ActiveX control. Click "Allow", and you will be able to browse through the docs.

### Documentation Generation On The Build Server
When you submit a PR, the documentation for that PR gets generated, along with all the other build artifacts, on the build server.
You can browse this generated documentation.
The link for this will be: `http://teamcity.monogame.net/repository/download/MonoGame_DevelopWin/<YOUR_BUILD_ID>:id*/Documentation.zip%21/index.html?guest=1`.
You can figure out your build id by inspecting the link that mgbot gives you after a successful build.
![Example build success image](/Documentation/images/Example_Build_Success.png)

Inspect the link that reads `Build X.X.X.XXXX` and plug the build id into `<YOUR_BUILD_ID>`.
e.g. `http://teamcity.monogame.net/repository/download/MonoGame_DevelopWin/11957:id/Documentation.zip%21/index.html?guest=1`
 