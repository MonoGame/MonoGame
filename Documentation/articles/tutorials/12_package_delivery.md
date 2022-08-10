# Part 12: Packaging and Delivery for Desktop

In this tutorial, you will be packaging the game for desktop and share the game to other people. In this tutorial we will be exporting for Windows. For other desktop systems such as Mac and Linux, the same steps are applied here. 

This section will be a step by step guide to packaging on Windows, however packaging to other desktop systems share the same steps in this guide.

You can find the system specific commands for packaging in other desktop systems here: [Package games for distribution](~/articles/packaging_games.html)

## Step 1

The first part is to navigate to your project directory with the project file. You can use your system's command line to go move to the project directory.

Since we are using visual studio, our project file is located at 
> ./source/repos/MonoGameStarShooter

<!-- ![](https://i.imgur.com/7SW8AIL.png) -->
![](~/images/first_2d_shooter/12_CLI_Navigate.png)

## Step 2
Now copy and paste this into your command line
```
dotnet publish -c Release -r win-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained
```

It will be building your project but it will take a while:

![](~/images/first_2d_shooter/12_CLI_Build.png)


## Step 3
Now you can navigate through your projects folder, and find the publish folder. Within our projects folder the publish is located under:
> \bin\Release\net6.0\win-x64

You can then compress the folder in a zip file. Now you can distribute your game to your friends.

To run the game, unzip the file and launch the executable inside the folder.

## Additional Notes

If you want to know more about packaging and what the commands will do, please see the [Package games for distribution](~/articles/packaging_games.md) article for more information.

## Congratulations!

You have finished making a game from start to finish using MonoGame! 

Want to learn more? Visit the [Community Tutorials](~/articles/tutorials.md) section for more resources.

For more Samples of great monogame ideas go to [Samples and Demos](~/articles/samples.md).

Interested in connecting with other members of the community and showcase your projects? Visit the [MonoGame Discord Server](https://discord.com/invite/monogame) here!
