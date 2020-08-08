# Creating a Project with Visual Studio

This guide will walk you through building a starter game with MonoGame using a Windows and Visual Studio 2019.

> Note, Please check the [migration guide](~/articles/migrate38.md) for any projects still based on MonoGame 3.7 or below.

## Prerequisites

Please ensure your development environment meets the prerequisites listed on the [MonoGame requirements guide](~/articles/introduction/requirements.md).

## Install MonoGame Templates

To create new projects from within Visual Studio, you will need to install the Visual Studio 2019 extension which can be found in "Extensions -> Manage Extensions" in the Visual Studio menu bar.

![Visual Studio Extension Manager](~/images/getting_started/1_VisualStudioExtensionManager.png)

Once open, simply search for **MonoGame** in the top right search window (as shown above) and install the "MonoGame project templates".  You now have the MonoGame templates installed ready to create new projects.

## Create Project

Start Visual Studio and select **New Project...** in the upper left corner.

![New Solution](~/images/getting_started/1_new_soulution_vs.png)

Now you should see a "New Project" dialog pop up, from here select **Templates > Visual C# > MonoGame** category, and then select **MonoGame Cross Platform Desktop Project**. 

![New Template](~/images/getting_started/1_template_dialog_vs.png)

Next type in the name that you wish to give your project, for this tutorial let's just use **ExampleGame** (*note* you should not use spaces when naming the project). After you've entered the name, click on the **...** button next to the location text field, and select where you wish to save your project. Finally click **OK** to create a new project.

![New Template](~/images/getting_started/1_configure_project_vs.png)

If everything went correctly, you should see an **ExampleGame** project open up like in the picture below. To run your game simply press the big **Play Button** in the toolbar or press **F5**.

![Run Game](~/images/getting_started/1_run_game_vs.png)

You should now see your game window running.

![Game](~/images/getting_started/1_game_vs.png)

## Next steps

Currently, it is just clearing the surface with blue color. For further information on developing your game, please look at the [Understanding the Code](2_understanding_the_code.md).
