# Part 1: Creating the GameManager class
In this tutorial section you will create a new class called a GameManager. This class will contain variables that control the scaling of sprites, and control the size of the application window.

You will also setup your preferred application window size as well within this section.

Later on the GameManager will hold properties of the Entities inside the game such as speed, health, firerate, etc.

## The GameManager class
To start, create a new folder within the same directory as your Game1.cs main file. Call this folder "Components". This folder will be used throughout the tutorial series to store all your CSharp files.

Next, create a new .cs file called "GameManager" inside the Components folders:

![](~/images/first_2d_shooter/1_FolderComponents.png)

Edit the GameManager class with the following code, where the namespace is your project name:

```csharp
//GameManager.cs
//Namespace should be your project name
namespace StarShooterDemo
{
    static class GameManager
    {
        //The SCALE variable allows you to 
        //uniformly scale sprites and movement with this value.
        public static float SCALE = 0.75f;
        //screenWidth and screenHeight will control the size of the window
        public static int screenWidth = 720;
        public static int screenHeight = 1080;
    }
}
```

GameManager is a static class, you can use the variables without initializing the GameManager as an object and that you don't need multiple of them in this case.

The scale variable will allow you to uniformly change how sprites and movement scaled with this value. We default this to scale 75% of the sprite's original size.

The screenWidth and screenHeight variables will allow you to change the application window size.

All these values can be changed to your liking so that it fits your own conditions.

## Implement Window sizes in Game1.cs
Using the values from GameManager you can implement a different window size to your game. Navigate to the Game1.cs file, and look for the Game1 constructor. Implement the following code:

```csharp
//Game1.cs
...
//This is the Game1 constructor
public Game1() 
{
    _graphics = new GraphicsDeviceManager(this);
    Content.RootDirectory = "Content";
    IsMouseVisible = true;
    
    //Window resize implementation here
    _graphics.PreferredBackBufferHeight = GameManager.screenHeight;
    _graphics.PreferredBackBufferWidth = GameManager.screenWidth;
}
```

You can use the graphics device manager and set their screen width and height, in this case you can set it using the GameManager values that you added in the last part.

Now you can run the game and see your changes to the window. In Visual Studio 2022, you can run the game by clicking on the Play button located on the top bar.

If you used the values that we used in the GameManager class, you should have a different sized window that is taller.

![](~/images/first_2d_shooter/1_WindowSize.png)

If your window is too small, or too large you can change the values in GameManager to fit your liking and those changes will be reflected when you rerun your game.

Once you have adjusted your window, you can move on to the next section, [Part 2: Handling Sprites](~/articles/tutorials/2_handling_sprites.md)
