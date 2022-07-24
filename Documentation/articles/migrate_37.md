# Migrating from 3.7

Previously MonoGame installed on your machine through an installer, but from 3.8 onwards everything is installed through NuGet packages and Visual Studio Extensions.

> **Note** MonoGame 3.8 project templates are not compatible with earlier versions of MonoGame.  If you wish to work on or build older MonoGame projects, then you will still need to install [MonoGame 3.7.1](https://www.monogame.net/downloads/) or earlier to open them.

## WindowsDX and DesktopGL

WindowsDX and DesktopGL templates now use SDK-style projects.
To migrate old projects we recommend creating a new project with the 3.8+ templates and copying the csproj to your project folder.  Make sure you back up your old project.

> For more information about SDK-style projects see the [documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/csproj).

## Other Platforms

To migrate open up your project file in a text editor.
The reference to the MonoGame assembly looks like this:

```xml
</ItemGroup>
    <Reference Include="MonoGame.Framework">
        <HintPath>$(MonoGameInstallDirectory)\MonoGame\v3.0\Assemblies\{Platform}\MonoGame.Framework.dll</HintPath>
    </Reference>
</ItemGroup>
```

The task to build your content is imported at the end of the project file like this:

```xml
<Import Project="$(MSBuildExtensionsPath)\MonoGame\v3.0\MonoGame.Content.Builder.targets" />
```

You can remove these references and add a reference to the MonoGame NuGet packages instead.

```xml
<ItemGroup>
    <PackageReference Include="MonoGame.Framework.{Platform}" Version="3.8.1" />
    <PackageReference Include="MonoGame.Content.Builder.Task" Version="3.8.1" />
</ItemGroup>
```

## Tooling

MonoGame tools (MGCB, 2MGFX, and the Pipeline Tool) are now distributed as [.NET Tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) automatically when using any of the MonoGame 3.8.1 templates.

The templates also build your .mgcb files automatically thanks to the `MonoGame.Content.Builder.Task` NuGet package.

The Pipeline Tool has been renamed MonoGame Content Builder Editor (MGCB Editor) and does not require an installation anymore (providing that you are using the MonoGame 3.8.1 templates). The Visual Studio 2022 extensions make .mgcb files within your solution clickable and will open the MGCB Editor on them.
