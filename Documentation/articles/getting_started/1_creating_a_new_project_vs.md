# Creating a Project with Visual Studio

> Note, MonoGame 3.8 projects are no longer compatible with earlier versions of MonoGame.  If you wish to work on or build older MonoGame projects, then you will still need to install [MonoGame 3.7.1](https://www.monogame.net/downloads/) or earlier to open them.

## Prerequisites

You will need a copy of [Visual Studio 2019](https://www.monogame.net/downloads/) or later installed (any edition, including Community) before installing MonoGame, with the following components (depending on your target platform):

![Visual Studio optional components](~/images/getting_started/1_installer_vs_components.png)

* .Net Desktop Development - For Windows GL / DX platforms
* Mobile Development with .NET - For Android / iOS platforms
* Universal Windows Platform development - For Windows 10 / Xbox UWP

When installing Visual Studio, it also is recommended to include the "**.NET Core cross-platform development" components:

![.NET Core component](~/images/getting_started/1_netcorecomponet.png)

> Alternatively, you can specifically install the [.NET Core SDK from here](https://dotnet.microsoft.com/download), SDK Versions 3.1 and above.

## Install MonoGame Templates

To create new projects from within Visual Studio, you will need to install the Visual Studio 2019 extension which can be found in "Extensions -> Manage Extensions" in the Visual Studio menu bar.

![Visual Studio Extension Manager](~/images/getting_started/1_VisualStudioExtensionManager.png)

Once open, simply search for **MonoGame** in the top right search window (as shown above) and install the "MonoGame project templates".  You now have the MonoGame templates installed ready to create new projects.

## Create Project

Start Visual Studio and select **New Project...** in the upper left corner.

![New Solution](~/images/getting_started/1_new_soulution_vs.png)

Now you should see a "New Project" dialog pop up, from here select **Templates > Visual C# > MonoGame** category, and then select **MonoGame Cross Platform Desktop Project**. Next type in the name that you wish to give your project, for this tutorial let's just use **ExampleGame** (*note* you should not use spaces when naming the project). After you've entered the name, click on the **Browse** button next to the location text field, and select where you wish to save your project. Finally click **OK** to create a new project.

![New Template](~/images/getting_started/1_template_dialog_vs.png)

If everything went correctly, you should see an **ExampleGame** project open up like in the picture below. To run your game simply press the big **Play Button** in the toolbar or press **F5**.

![Run Game](~/images/getting_started/1_run_game_vs.png)

You should now see your game window running.

![Game](~/images/getting_started/1_game_vs.png)

## Next steps

Currently, it is just clearing the surface with blue color. For further information on developing your game, please look at the [Understanding the Code](2_understanding_the_code.md).
