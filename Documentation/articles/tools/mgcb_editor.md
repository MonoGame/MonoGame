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

The MGCB Editor is automatically installed (if you are using MonoGame's templates) and accessible by double-clicking an .mgcb file from Visual Studio 2022 (if you have the extension installed).

Alternatively, you can open the MGCB Editor from the .NET command line. This will only work if you are using the MonoGame templates and executing the command from the root directory of your project:

```
dotnet mgcb-editor
```

If it is the first time you run the tool, you might need to restore tools first (.NET should invite you to do so if you try the above command):

```
dotnet tool restore
```

See [Using MGCB Editor](~/articles/content/using_mgcb_editor.md) for more information.
