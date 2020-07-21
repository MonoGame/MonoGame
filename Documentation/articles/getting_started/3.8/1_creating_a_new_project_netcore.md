## .NET Core CLI

From MonoGame 3.8, the project supports creating solutions directly from the command-line and using lightweight tools such as [Visual Studio Code](https://code.visualstudio.com/) to edit the solution.

## Prerequisites

You need to install the [.NET Core SDK from here](https://dotnet.microsoft.com/download) to be able to run the necessary commands to create and build projects.

After installation, you can run `dotnet --info` in a terminal to make sure the installation was successful.

## Download MonoGame Templates

MonoGame now publishes project templates for [dotnet new](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new) to the dotnet registry.

To install the C# templates run the following from the command-line or the Terminal window in your code editor:

```
dotnet new -i MonoGame.Templates.CSharp
```

> For development branches you will also need to include the version number in the command, e.g. 'dotnet new --install MonoGame.Templates.CSharp::3.8.0.1375-develop'

## MonoGame Content Pipeline Tool

The content pipeline tool is also now published to the .NET tools library. To install the pipeline tool simply open a command prompt and run the following dotnet command (if you get an error, please ensure you have installed the .[NETCore SDK](https://dotnet.microsoft.com/download) listed above):

```
dotnet tool install -g dotnet-mgcb
```

> For development branches you will also need to include the version number in the command, e.g. 'dotnet tool install -g dotnet-mgcb-editor --version 3.8.0.1375-develop'

For more details on the [MonoGame tools check here](/tools/tools.md).

## Create a MonoGame Project

You can now create new MonoGame projects. To do that:

- Create a new directory for your project.

- Open a new terminal window or command prompt and navigate to your project directory.

- Run `dotnet new <TemplateID> -o <ProjectName>` to create your project, where `<TemplateID>` is a platform identifier, and `<ProjectName>` the name of your project.

For example:

```
dotnet new mgdesktopgl -o MyGame
```

> To know which platform identifier (short name) to use for your project, please refer to [Target Platforms](/introduction/platforms.md) or type the following command to the command prompt to list the installed templates and their corresponding short names:
> 
> ```
> dotnet new -l
> ```

Once created, you can open your code editor of choice in the new folder and begin editing.
