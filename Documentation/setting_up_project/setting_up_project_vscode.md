# Setting up a new project with Visual Studio Code

Please read [Setting up a new project](setting_up_project.md) for an explanation of strengths and weaknesses between the different approaches shown below.

For convenience sake, we shall use `Pong` as our project name. Replace all occurrences of it with the name you want.

Visual Studio Code does not have any new project wizard. Instead, we will use the `dotnet` command-line tool to create our project. Follow the shell commands in either `powershell`, `bash` or `zsh`.

No matter which aproach you choose, you will need MonoGame templates installed, so first off run the following command:

```sh
dotnet new --install MonoGame.Templates.CSharp
```

## 1. Using .NET Standard library

First off we need to make a directory for our game:

```sh
mkdir Pong
cd Pong
```

Now that we have our directory, let us create our game library project which will possess all the code:

```sh
dotnet new sln
dotnet new mgnetstandard -n Pong
dotnet sln add Pong/Pong.csproj
```

Next up, we need to make a project for each platform we want our game to run on. The following steps apply to any platform, however for simplicity, let us make a project that can be run on Windows, Linux, and Mac:

```sh
dotnet new mgdesktopgl -n Pong.DesktopGL
dotnet sln add Pong.DesktopGL/Pong.DesktopGL.csproj
```

Now that we have created our platform project, we need to do a few tweaks to it.

First reference our game library from it:

```sh
cd Pong.DesktopGL
dotnet add reference ../Pong/Pong.csproj
```

Next, delete the generated `Game1.cs` and `Content` as they both exist in our library project:

```sh
rm -r Content
rm Game1.cs
```

And finally, we need to fix the link to our content project, simply replace `Content\Content.mgcb` with `..\Pong\Content\Content.mgcb` in `Pong.DesktopGL.csproj`:

Everything is ready, and you can now run the game :)

```
dotnet run
```

## 2. Using a Shared Library

First off we need to make a directory for our game:

```sh
mkdir Pong
cd Pong
```

Now that we have our directory, let us create our shared game library project which will possess all the code:

```sh
dotnet new sln
dotnet new mgshared -n Pong
```

As stated in the previous tutorial, shared project are not working properly with some newer stuff. So instead of using them lets make our own using wildcards:

```
rm Pong/Pong.projitems
rm Pong/Pong.shproj
```

Next up, we need to make a project for each platform we want our game to run on. The following steps apply to any platform, however for simplicity, let us make a project that can be run on Windows, Linux, and Mac:

```sh
dotnet new mgdesktopgl -n Pong.DesktopGL
dotnet sln add Pong.DesktopGL/Pong.DesktopGL.csproj
```

Now that we have created our platform project, we need to do a few tweaks to it.

First reference our game library from it:

```sh
cd Pong.DesktopGL
```

Next, delete the generated `Game1.cs` and `Content` as they both exist in our shared library project:

```sh
rm -r Content
rm Game1.cs
```

And finally, we need to reference our shared project. Simply replace the following text in `Pong.DesktopGL.csproj`:

```xml
<ItemGroup>
  <MonoGameContentReference Include="Content\Content.mgcb" Visible="false" />
</ItemGroup>
```

With:

```xml
<ItemGroup>
  <Compile Include="..\Pong\**\*.cs">
    <Link>Pong\%(RecursiveDir)%(Filename)%(Extension)</Link>
  </Compile>
</ItemGroup>

<ItemGroup>
  <MonoGameContentReference Include="..\Pong\**\*.mgcb">
    <Link>Pong\%(RecursiveDir)%(Filename)%(Extension)</Link>
  </MonoGameContentReference>
</ItemGroup>
```

Everything is ready, and you can now run the game :)

```
dotnet run
```
