# Creating a Project with Visual Studio for Mac

MonoGame is available on Mac for development using the instructions below to get started with **MonoGame 3.7**.

> Note, MonoDevelop and Xamarin Studio are no longer available for download today, so these instructions only apply if you have a pre-existing installation of MonoDevelop.  
> For Visual Studio for Mac, please use [MonoGame 3.8 and above](../3.8/1_creating_a_new_project_vsm.md).

## Prerequisites

You will need a copy of MonoDevelop or Xamarin Studio for Mac installed before installing MonoGame.  

## [Download MonoGame](https://www.monogame.net/downloads/)

Visit the [MonoGame downloads](https://www.monogame.net/downloads/) page and chose the edition of MonoGame you wish to install.

![Versions Image](~/images/getting_started/1_MonoGameVersions.png)

From there select the edition for your Operating system and download them to your machine, in this case:

* MonoGame 3.7.1 for MacOS
* MonoGame 3.7.1 Pipeline GUI Tool for MacOS

![Releases Image](~/images/getting_started/1_Installers_md.png)

> When prompted, click OK, to allow downloading of the files from "https://community.monogame.net"

## Run Installer

Once downloaded, run the installers and install MonoGame to your preferred install location.

![Installer](~/images/getting_started/1_installer_md.png)

> You may get a warning that the package cannot be opened because "It is from an unidentified developer". This is perfectly normal as MonoGame is not distributed on the Apple Store. Click "Open" to continue. (If the Open button does not appear, you may need to open it in Finder)

Once both installs are complete, your MonoGame experience is ready on your Mac.

## Create Project

Start MonoDevelop / Xamarin Studio and select **New...** in the upper left corner.

![New Solution](~/images/getting_started/1_new_soulution_md.png)

Now you should see a "New Project" dialog pop up. From here select **MonoGame > App** category, then select **MonoGame Cross Platform Desktop Project** and click **Next**.

![New Template](~/images/getting_started/1_template_dialog_md.png)

On the following dialog, type in the name that you wish to give your project, for this tutorial let's just use **ExampleGame** (*note* you should not use spaces when naming the project). After you've entered the name, click on the **Browse** button next to the location text field, and select where you wish to save your project. Finally, click **Create** to create a new project.

![New Project](~/images/getting_started/1_project_dialog_md.png)

If everything went correctly, you should see an **ExampleGame** project open up like in the picture bellow. To run your game simply press the big **Play Button** in the upper left corner or press **F5**.

## Running your project

![Run Game](~/images/getting_started/1_run_game_md.png)

You should now see your game window running.

![Game](~/images/getting_started/1_game_md.png)

Currently, it is just clearing the surface with blue color. For further information on creating your game, please look at the [Understanding the Code](../2_understanding_the_code.md).
