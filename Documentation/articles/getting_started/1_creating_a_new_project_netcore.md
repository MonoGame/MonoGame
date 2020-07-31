## .NET Core CLI

From MonoGame 3.8, the project supports creating solutions directly from the command-line and using lightweight tools such as [Visual Studio Code](https://code.visualstudio.com/) to edit the solution.

## Prerequisites

Please ensure your development environment meets the prerequisites listed on the [MonoGame requirements guide](~/articles/introduction/requirements.md).

## Download MonoGame Templates

MonoGame now publishes project templates for [dotnet new](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new) to the dotnet registry.

To install the C# templates run the following from the command-line or the Terminal window in your code editor:

```
dotnet new -i MonoGame.Templates.CSharp
```

> For development branches you will also need to include the version number in the command, e.g. 'dotnet new --install MonoGame.Templates.CSharp::3.8.0.1375-develop'

## Create a MonoGame Project

You can now create new MonoGame projects. To do that:

- Create a new directory for your project.

- Open a new terminal window or command prompt and navigate to your project directory.

- Run `dotnet new <TemplateID> -o <ProjectName>` to create your project, where `<TemplateID>` is a platform identifier, and `<ProjectName>` the name of your project.

For example:

```
dotnet new mgdesktopgl -o MyGame
```

> To know which platform identifier (short name) to use for your project, please refer to [Target Platforms](/articles/introduction/platforms.md) or type the following command to the command prompt to list the installed templates and their corresponding short names:
> 
> ```
> dotnet new -l
> ```

Once created, you can open your code editor of choice in the new folder and begin editing.

> To run your project, check the instructions for [packaging your game](~/articles/packaging_games.md) to build the executable using the .NET tooling.

## Next steps

Currently, when you do run your game, is just clearing the surface with blue color. For further information on creating your game, please look at the [Understanding the Code](2_understanding_the_code.md).
