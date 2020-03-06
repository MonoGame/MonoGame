**This package only works if you are targeting .NET Core.** This package allows you to package up your MonoGame game into a flatpak installer for Linux.

## Requirements:
- netcoreapp as the target
- flatpak
  - org.freedesktop.Platform/x86_64/1.6
  - org.freedesktop.Sdk/x86_64/1.6

Flatpak install instructions: [https://flatpak.org/setup/](https://flatpak.org/setup/)

To install the required runtimes, simply do:
```sh
flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo
flatpak install flathub org.freedesktop.Platform/x86_64/1.6
flatpak install flathub org.freedesktop.Sdk/x86_64/1.6
```

## Usage:
Call the publish command as you normally would: `dotnet publish -r linux-x64`, the resulting flatpak should get generated in the output directory.

## Customization:
This nuget package offers the following properties for customizing the build of the flatpak. Simply set them in the csproj or pass them to msbuild to change their values:

```
| Variable Name            | Description / Default value            |
| ------------------------ | -------------------------------------- |
| MGFlatpakIntermediateDir | Folder for temporary files.            |
|                          | $(IntermediateOutputPath)              |
| ------------------------ | -------------------------------------- |
| MGFlatpakOutputPath      | The output folder for the flatpak.     |
|                          | $(OutputPath)                          |
| ------------------------ | -------------------------------------- |
| MGFlatpakProjectDir      | The current project directory.         |
|                          | $(ProjectDir)                          |
| ------------------------ | -------------------------------------- |
| MGFlatpakPublishDir      | The publish output folder.             |
|                          | $(PublishDir)                          |
| ------------------------ | -------------------------------------- |
| MGFlatpakAssemblyName    | The output assembly to run.            |
|                          | $(AssemblyName)                        |
| ------------------------ | -------------------------------------- |
| MGFlatpakTitle           | The game title.                        |
|                          | $(AssemblyTitle)                       |
| ------------------------ | -------------------------------------- |
| MGFlatpakId              | The game id.                           |
|                          | com.$(Company).$(AssemblyName)         |
| ------------------------ | -------------------------------------- |
| MGFlatpakIcon            | The icon file, needs to be png format. |
|                          | Icon.png                               |
```

