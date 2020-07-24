# Creating a Project with Visual Studio for Mac

MonoGame is available on Mac for development using the instructions below to get started with MonoGame 3.7.

> Note, MonoDevelop and Xamarin Studio are no longer available for download today, so these instructions only apply if you have a pre-existing installation of MonoDevelop.  For Visual Studio for Mac, please use [MonoGame 3.8 and above](../3.8/1_creating_a_new_project_vsm.md).

## Prerequisites

You will need a copy of [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac/) installed before installing MonoGame.

If you also need the MGCB Editor to use a GUI to manage your content you will need to install the [.NET Core SDK from here](https://dotnet.microsoft.com/download), SDK Versions 3.1 and above.


## Install MonoGame Templates

From MonoGame 3.8, we no longer use Installers for deploying the framework and templates as this creates hard dependencies to specific machine setups or DLL locations.  Instead, MonoGame now uses NuGet and the DotNet deployment platform to reference a specific version of MonoGame online.

To create new projects from within Visual Studio, you will need to install the Visual Studio for Mac extension which can be found in "Visual Studio -> Extensions..." in the menu bar.

![Visual Studio Extension Manager](~/images/getting_started/1_VisualStudioMacExtensionManager.png)

Once open, simply search for **MonoGame** in the top right search window (as shown above) and install the "MonoGame project templates".  You now have the MonoGame templates installed ready to create new projects.

## MonoGame Content Pipeline Tool (MGCB Editor)

The MGCB Editor which was previously available inside the MonoGame installer is now published to the .NET tools library. To install the MGCB Editor, simply open a command prompt and run the following dotnet command (if you get an error, please ensure you have installed the .NETCore SDK listed above):

```
dotnet tool install -g dotnet-mgcb-editor
```

> For development branches, you will need to ensure you have the dev channel NuGet registered and include the version number in the command, for example:
>
> ```
> dotnet nuget add source -n MonoGame http://teamcity.monogame.net/guestAuth/app/nuget/feed/_Root/default/v3/index.json
> 
> dotnet tool install --global dotnet-mgcb-editor --version 3.8.0.1476-develop
> ```

***Currently, the MGCB Editor cannot be registered with Visual Studio for Mac, so the content project has to be opened manually outside of Visual Studio***

For more details on the [MonoGame tools check here](~/articles/tools/tools.md).

## Create Project

Start MonoDevelop / Xamarin Studio and select **New...** in the upper left corner.

![New Solution](~/images/getting_started/1_new_soulution_md.png)

Now you should see a "New Project" dialog pop up. From here select **MonoGame > App** category, then select **MonoGame Cross Platform Desktop Project** and click **Next**.

![New Template](~/images/getting_started/1_template_dialog_md.png)

On the following dialog, type in the name that you wish to give your project, for this tutorial let's just use **ExampleGame** (*note* you should not use spaces when naming the project). After you've entered the name, click on the **Browse** button next to the location text field, and select where you wish to save your project. Finally, click **Create** to create a new project.

![New Project](~/images/getting_started/1_project_dialog_md.png)

If everything went correctly, you should see an **ExampleGame** project open up like in the picture below. To run your game simply press the big **Play Button** in the upper left corner or press **F5**.

## Running your project

![Run Game](~/images/getting_started/1_run_game_md.png)

You should now see your game window running.

![Game](~/images/getting_started/1_game_md.png)

Currently, it is just clearing the surface with blue color. For further information on creating your game, please look at the [Understanding the Code](../2_understanding_the_code.md).
