# MGCB Editor

MonoGame Content Builder (MGCB) Editor is the front-end GUI editor for MonoGame content builder projects.

![MCGB Editor](~/images/MGCB-editor.png)

The MGCB Editor has the following features:

* Create, open, and save MGCB projects.
* Import existing XNA .contentproj.
* Tree view showing content of project.
* Property grid for editing content settings.
* Full undo/redo support.
* Build, rebuild, and clean the project.
* Rebuild selected items.
* Create new content like fonts and xml.
* Support for custom importers/processors/writers.
* Template format for adding new custom content types.

## Installation Instructions

The MGCB Editor is published as a [.NET tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools), Make sure you have the .NET SDK installed if you wish to install the editor. You can download it [here](https://dotnet.microsoft.com/download).

In a command prompt/terminal window, run the following command to install the MGCB Editor:

```
dotnet tool install -g dotnet-mgcb-editor
```

> For development branches, you will need to ensure you have the dev channel NuGet registered and include the version number in the command, for example:
>
> ```
> dotnet nuget add source -n MonoGame http://teamcity.monogame.net/guestAuth/app/nuget/feed/_Root/default/v3/index.json
> 
> dotnet tool install --global dotnet-mgcb-editor --version 3.8.0.1476-develop
> ```

After installing `mgcb-editor` run the following to register the MGCB Editor as the default app for mgcb files.

```
mgcb-editor --register
```

See [Using MGCB Editor](~/articles/content/using_mgcb_editor.md) for more information.
