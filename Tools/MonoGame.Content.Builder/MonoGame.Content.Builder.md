This package provides the MonoGame Content Builder project integration.

## Usage:
After adding the package simply add .mgcb file with `MonoGameContentReference` build action and it will automatically get built every time you build the project.

During the first build it will automatically detect your current platform and download the required per platform nuget package (~30 MB).

## Customization:
This nuget package offers the following properties for customizing the build of your content. Simply set them in the csproj or pass them to msbuild to change their values:

```
| Variable Name     | Description / Default value               |
| ----------------- | ----------------------------------------- |
| MonoGamePlatform  | The platform for the MGCB.                |
|                   | Gets set by MonoGame platform nugets.     |
| ----------------- | ----------------------------------------- |
| MgcbArguments     | Additional arguments to pass to the MGCB. |
|                   | /quiet                                    |
| ----------------- | ----------------------------------------- |
| MgcbPath          | Path to MGCB.exe                          |
|                   | The per platform MGCB nuget package.      |
| ----------------- | ----------------------------------------- |
```