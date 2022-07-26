# Migrating from 3.8.0

Migrating from 3.8.0 should be straightforward for most platforms.

The major difference is that 3.8.1 now requires .NET 6 and Visual Studio 2022. You can follow the [environment setup tutorial](./getting_started/0_getting_started.md) to make sure that you are not missing any components.

The MGCB Editor is no longer a global .NET tool and we recommend that you use the new Visual Studio 2022 extension which helps accessing it without the need of CLI commands.

## WindowsDX, DesktopGL, and UWP

Upgrading from 3.8.0 should be as straightforward as upgrading your ```TargetFramework``` and MonoGame version.

Edit your csproj file to change your ```TargetFramework```:

```xml
<TargetFramework>net6.0</TargetFramework>
```

Then edit your MonoGame ```PackageReference``` to point to 3.8.1:

```xml
<PackageReference Include="MonoGame.Framework.{Platform}" Version="3.8.1.*" />
<PackageReference Include="MonoGame.Content.Builder.Task" Version="3.8.1.*" />
```

### Accessing MCGB Editor without a global tool

MGCB Editor is no longer a .NET global tool, and doesn't need to be installed or registered.

However, if you are migrating from 3.8.0, you will need to setup a configuration file. Next to your ```.csproj```, create a folder nammed ```.config``` and a file within it nammed ```dotnet-tools.json``` with this content:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "dotnet-mgcb": {
      "version": "3.8.1.*",
      "commands": [
        "mgcb"
      ]
    },
    "dotnet-mgcb-editor": {
      "version": "3.8.1.*",
      "commands": [
        "mgcb-editor"
      ]
    },
    "dotnet-mgcb-editor-linux": {
      "version": "3.8.1.*",
      "commands": [
        "mgcb-editor-linux"
      ]
    },
    "dotnet-mgcb-editor-windows": {
      "version": "3.8.1.*",
      "commands": [
        "mgcb-editor-windows"
      ]
    },
    "dotnet-mgcb-editor-mac": {
      "version": "3.8.1.*",
      "commands": [
        "mgcb-editor-mac"
      ]
    }
  }
}
```

With this file, .NET will automatically install the MGCB Editor for you when launching Visual Studio 2022. If you installed the Visual Studio extension, you should also be able to just double-click an ```.mgcb``` file to open the MGCB Editor.

## iOS/iPadOS, and Android

.NET 6 introduced breaking changes in how csproj are defined for iOS/iPadOS and Android. We recommand that you create new projects using the 3.8.1 templates and that you copy over your project files there.
