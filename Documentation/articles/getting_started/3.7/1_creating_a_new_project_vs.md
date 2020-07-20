# Creating a Project with Visual Studio

MonoGame is available on Windows for development using the instructions below to get started with **MonoGame 3.7**.

> Note, For Visual Studio 2019 and above it is recommended to use [MonoGame 3.8 and above](../3.8/1_creating_a_new_project_vs.md).  While it is possible to copy over the project templates to VS 2019 from the VS 2017 installer, it is not advised.

## Prerequisites

You will need a copy of [Visual Studio 2017](https://visualstudio.microsoft.com/downloads/) or earlier installed (any edition, including Community) before installing MonoGame.  

## [Download MonoGame](https://www.monogame.net/downloads/)

Visit the [MonoGame downloads](https://www.monogame.net/downloads/) page and chose the edition of MonoGame you wish to install.

![Versions Image](~/images/getting_started/1_MonoGameVersions.png)

From there select the edition for your Operating system and download them to your machine, in ths case:

* MonoGame 3.7.1 for Visual Studio

![Releases Image](~/images/getting_started/1_Installers_vs.png)

## Run Installer

Once downloaded, run the installers and install MonoGame to your preferred install location.

![Installer](~/images/getting_started/1_installer_vs.png)

Once the installation is complete, your MonoGame experience is ready on your PC.

## Create Project

Start up Visual Studio and select **New Project...** in the upper left corner.

![New Solution](~/images/getting_started/1_new_soulution_vs.png)

Now you should see a "New Project" dialog pop up, from here select **Templates > Visual C# > MonoGame** category, and then select **MonoGame Cross Platform Desktop Project**. Next type in the name that you wish to give your project, for this tutorial let's just use **ExampleGame** (do note that you should not use space character for it). After you've entered the name, click on the **Browse** button next to the location text field, and select where you wish to save your project. Finally click **OK** to create a new project.

![New Template](~/images/getting_started/1_template_dialog_vs.png)

If everything went correctly, you should see an **ExampleGame** project open up like in the picture bellow. To run your game simply press the big **Play Button** in the toolbar or press **F5**.

![Run Game](~/images/getting_started/1_run_game_vs.png)

You should now see your game window running.

![Game](~/images/getting_started/1_game_vs.png)

Currently it's just clearing the surface with blue color. For further information on creating your game, please look at the [Understanding the Code](2_understanding_the_code.md).
