MGCB Editor is the front-end GUI editor for MonoGame content builder projects.

[!MGCB Editor](~/images/pipeline.png)

MGCB Editor has the following features:

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

MGCB Editor can be installed as a [.NET Core tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools).
Make sure you have the .NET Core SDK installed. You can download it [here](https://dotnet.microsoft.com/download).

In a terminal run `dotnet tool install -g dotnet-mgcb-editor` to install MGFXC.

After installing `mgcb-editor` run `dotnet mgcb-editor --register` to register MGCB Editor as the default app for .mgcb
files. This currently does not work on macOS.

[Read detailed documentation](~/articles/content/using_pipeline_tool.md).

