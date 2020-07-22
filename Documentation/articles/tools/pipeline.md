# MonoGame  Editor

MGCB Content Builder (MGCB) Editor  is the front-end GUI editor for MonoGame content builder projects.

![MCGB Editor](~/images/pipeline.png)

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

The MGCB Editor can be installed as a [.NET Core tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools).
Make sure you have the .NET Core SDK installed. You can download it [here](https://dotnet.microsoft.com/download).

In a terminal run the following to install the MGCB Editor:

```
dotnet tool install -g dotnet-mgcb-editor
```

> For development branches you will also need to include the version number in the command, e.g.
>
> ```
> dotnet tool install -g dotnet-mgcb-editor --version 3.8.0.1375-develop
> ```

After installing `mgcb-editor` run the following to register the MGCB Editor as the default app for .mgcb files.

```
dotnet mgcb-editor --register
```

> This currently does not work on macOS.

[Read detailed documentation](~/articles/content/using_pipeline_tool.md).
