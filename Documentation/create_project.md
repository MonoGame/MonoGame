# Create a MonoGame Project

MonoGame distributes templates to set up a new project in two ways:

- For Visual Studio
- For .NET Core CLI

## Visual Studio

MonoGame templates can be installed as a Visual Studio extension.

- In Visual Studio go to Extensions > Manage Extensions...
- Make sure you have Online selected on the left pane.
- Search for MonoGame.
- Select the MonoGame Project Templates extension and click Download.
- Restart Visual Studio to install the extension.

When Visual Studio restarts click Create a new project. The MonoGame templates should show up at the top, if not search for MonoGame in the search bar.

You'll see a few different templates were installed. For an overview of the different platforms see [Target Platforms](Platforms.md).

## .NET Core CLI

You can set up a project using the .NET Core CLI (Command Line Interface).
If you don't have the .NET Core SDK installed, go get the latest version [here](https://dotnet.microsoft.com/download).
After installation you can run `dotnet --info` in a terminal to make sure the installation was successful.

MonoGame publishes templates for [dotnet new](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new).
To install the C# templates run `dotnet new -i MonoGame.Templates.CSharp`.

In a terminal navigate to where you want to create the project.
Run `dotnet new <TemplateID> -o <ProjectName>` to create your project, for example: `dotnet new mgdesktopgl -o MyGame`.

There are templates in the package for different platforms. For an overview of the different platforms see [Target Platforms](Platforms.md).
