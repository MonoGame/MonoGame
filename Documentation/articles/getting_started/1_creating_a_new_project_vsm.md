# Creating a Project with Visual Studio for Mac

This guide will walk you through building a starter game with MonoGame using a Mac and Visual Studio for Mac.

> Note, Please check the [migration guide](~/articles/migrate38.md) for any projects still based on MonoGame 3.7 or below.

## Prerequisites

Please ensure your development environment meets the prerequisites listed on the [MonoGame requirements guide](~/articles/introduction/requirements.md).

## Install MonoGame Templates

From MonoGame 3.8, we no longer use Installers for deploying the framework and templates as this creates hard dependencies to specific machine setups or DLL locations.  Instead, MonoGame now uses NuGet and the DotNet deployment platform to reference a specific version of MonoGame online.

To create new projects from within Visual Studio, you will need to install the Visual Studio for Mac extension which can be found in "Visual Studio -> Extensions..." in the menu bar.

![Visual Studio Extension Manager](~/images/getting_started/1_VisualStudioMacExtensionManager.png)

Once open, simply search for **MonoGame** in the top right search window (as shown above) and install the "MonoGame project templates".  You now have the MonoGame templates installed ready to create new projects.

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

## Next steps

Currently, it is just clearing the surface with blue color. For further information on developing your game, please look at the [Understanding the Code](2_understanding_the_code.md).
