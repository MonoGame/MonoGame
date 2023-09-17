# Setting up your development environment for Windows

This section provides a step-by-step guide for setting up your development environment on Windows.

MonoGame can work with most .NET compatible tools, but we recommend [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)

> [!IMPORTANT]
> Prior version of Visual Studio are not supported with MonoGame 3.8.1

Alternatively, you can use [JetBrains Rider](https://www.jetbrains.com/rider/) or [Visual Studio Code](https://code.visualstudio.com/).

## [Recommended] Install Visual Studio 2022

Before using MonoGame, you will need to install [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)

> [!NOTE]
> You can install any edition of Visual Studio, including the Community edition.

When installing Visual Studio, the following workloads are required depending on your desired [target platform(s)](~/platforms.md):

* Mandatory for all platforms:
    * **.Net desktop development**
* Optional
    * **.Net Multi-platform App UI Development** if you wish to target Android, iOS, or iPadOS.
    * **Universal Windows Platform development** if you wish to build for Windows store or Xbox.

![Visual Studio optional components](~/images/getting_started/1_installer_vs_components.png)

> [!IMPORTANT]
> If you are targeting the standard Windows DirectX backend, you will also need [the DirectX June 2010 runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109) for audio and gamepads to work properly.

### Install MonoGame extension for Visual Studio 2022

To create new MonoGame projects from within Visual Studio 2022, you will need to install the **MonoGame Framework C# project templates** extension.  The following steps demonstrate how to install the extension.

1. Launch Visual Studio 2022
2. Select **Continue without code**.  This will launch Visual Studio without any project or solution opened.

![Visual Studio Launcher Continue Without Code](~/images/getting_started/1_ContinueWithoutCode.png)

3. Click "*Extensions -> Manage Extensions* in the Visual Studio 2022 menu bar.  This will open the Manage Extensions dialog window.

![Extensions -> Manage Extensions Menu Selection](~/images/getting_started/1_VisualStudioExtensionMenu.png)

4. Use the search box in the top-right corner of the Manage Extensions dialog window to search for **MonoGame**, then click the **MonoGame Framework C# project templates** extension as shown below and download it to install it.

![Visual Studio Extension Manager](~/images/getting_started/1_VisualStudioExtensionManager.png)


5. After it is downloaded, an alert will appear at the bottom of the Manage Extensions window that states "Your changes will be scheduled.  The modifications will begin when all Microsoft Visual Studio windows are closed."  Click the **Close** button, then close Visual Studio 2022.

6. After closing Visual Studio 2022, a VSIX Installer window will open confirming that you want to install the **MonoGame Framework C# project templates** extension.  Click the **Modify** button to accept the install.

![VSIX Installer Window](~/images/getting_started/1_VSIXInstallerWindow.png)

You now have the MonoGame templates installed and are ready to create new projects.

**Next up:** [Creating a new project](2_creating_a_new_project_vs.md)

## [Alternative] Install the .NET 6 SDK (compatible with JetBrains Rider and Visual Studio Code)

If you prefer to use JetBrains Rider or Visual Studio Code, and after installing either of them, you will also need to [install the .NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

Once the .NET 6 SDK is installed, you can open a Command Prompt and install the MonoGame templates by typing the following command:

```sh
dotnet new install MonoGame.Templates.CSharp
```

**Next up:** [Creating a new project](2_creating_a_new_project_vs.md)
