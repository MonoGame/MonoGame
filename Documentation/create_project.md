MonoGame distributes templates to set up a new project in two ways:

- For Visual Studio
- For .NET Core CLI

Once a project is created through either of these methods, it can be opened and built from any of the [compatible environments](requirements.md).

## Visual Studio

MonoGame templates can be installed as a Visual Studio extension.

- In Visual Studio go to Extensions > Manage Extensions...
- Make sure you have Online selected on the left pane.
- Search for MonoGame.
- Select the MonoGame Project Templates extension and click Download.
- Restart Visual Studio to install the extension.

When Visual Studio restarts, click Create a new project. The MonoGame templates should show up at the top, if not search for MonoGame in the search bar.

You'll see a few different templates were installed. To know which one to use, please refer to overview of the [Target Platforms](Platforms.md).

## .NET Core CLI

You can set up a project using the .NET Core CLI (Command Line Interface).
If you don't have the .NET Core SDK installed, go get the latest version [here](https://dotnet.microsoft.com/download) (3.1 and up recommended).
After installation, you can run `dotnet --info` in a terminal to make sure the installation was successful.

MonoGame publishes templates for [dotnet new](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new). To install the C# templates run `dotnet new -i MonoGame.Templates.CSharp`.

You can now create new projects. To do that, create a new directory for your project. Then, open a terminal and navigate to your project directory.
Run `dotnet new <TemplateID> -o <ProjectName>` to create your project, where `<TemplateID>` is a platform identifier, and `<ProjectName>` the name of your project.

For example: `dotnet new mgdesktopgl -o MyGame`.

To know which platform identifier to use for your project, please refer to [Target Platforms](Platforms.md).
