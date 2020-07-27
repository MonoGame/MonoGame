# Migrating to MonoGame 3.8

Previously MonoGame installed on your machine through an installer, but from 3.8 onwards everything is installed through NuGet packages.

## WindowsDX and DesktopGL

WindowsDX and DesktopGL templates now use SDK-style projects.
To migrate old projects we recommend creating a new project with the 3.8+ templates and
copying the csproj to your project folder. Make sure you back up your old project.
For more information about SDK-style projects see the [docs](https://docs.microsoft.com/en-us/dotnet/core/tools/csproj).

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
    <PackageReference Include="MonoGame.Framework.{Platform}" Version="3.8.0" />
    <PackageReference Include="MonoGame.Content.Builder.Task" Version="3.8.0" />
</ItemGroup>
```

## Tooling

MonoGame tools are now distributed as [.NET Core Tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools).
You do not need the tools to build content for your games. The templates reference the `MonoGame.Content.Builder.Task`
NuGet package that automatically builds your content when building your game.

- [MonoGame Content Builder](~/articles/tools/mgcb.md) (MGCB): `dotnet tool install -g dotnet-mgcb`
- [MonoGame Effect Compiler](~/articles/tools/2mgfx.md) (MGFXC; previously 2MGFX): `dotnet tool install -g dotnet-mgfxc`
- [MGCB Editor](~/articles/tools/pipeline.md) (Previously Pipeline Tool): `dotnet tool install -g dotnet-mgcb-editor`

After installing `mgcb-editor` run `mgcb-editor --register` to register Pipeline Editor as the default app for mgcb
files.
