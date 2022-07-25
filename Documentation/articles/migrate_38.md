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
<PackageReference Include="MonoGame.Framework.{Platform}" Version="3.8.1" />
<PackageReference Include="MonoGame.Content.Builder.Task" Version="3.8.1" />
```

## iOS/iPadOS, and Android

.NET 6 introduced breaking changes in how csproj are defined for iOS/iPadOS and Android. We recommand that you create new projects using the 3.8.1 templates and that you copy over your project files there.
