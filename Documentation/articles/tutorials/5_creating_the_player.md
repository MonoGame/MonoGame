# Part 5: Creating the Player
In this section you will be using the Entity class from the previous section in [Part 3: Creating the Entity class](~/articles/tutorials/3_creating_entity_class.md) to create the Player class, implement keyboard player movement, and initilize the player in the Game Class.

## Create a new class file
Just like how you created the files for Entity and SpriteArt, under the Componenets folder add a new class and name it "Player"

Again, rewrite the current code to the following:
```csharp
//Player.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace SpaceShooterDemo 
{
    class Player : Entity
    {
        //Player code will go here
    }   
}
```
>In Part 3: Creating the Entity, you created the **abstract** class. By using inheritence you are able to gain the methods and variables from the Entity class in order to utilize them in the Player class. Using the code above, the player now contains the position, texture, and the other methods from Entity.




## Keyboard Controls

Lets create a new variable that controls the side by side speed of the player and the constructor.

```csharp
//Player.cs
namespace SpaceShooterDemo 
{
    class Player : Entity
    {
        //Class Variables go here
        int sideSpeed = 5;
        
        //Constructor
        public Player(Texture2D image, Vector2 initalPosition) 
        {
            texture = image;
            pos = initalPosition;
        }
    }
}
```

The constructor is a method that only runs once when creating the Player object. It takes a image and a initial position as a parameter and sets the variables that was inherited from the Entity class. 

Now you can implement the Update method inherited from the Entity class. Right below the constructor you will put the keyboard controls for the player:

```csharp
//Player.cs
        ...
        //The abstract method from Entity to override
        public override void Update()
        {
            //Keyboard Controls
            //Side to Side Movement
            
            //Moving Left
            if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A))
            {
                //Constraint so that it can't move outside the window
                if (pos.X > 0) 
                {
                    pos.X -= sideSpeed;
                }
            }
            //Moving Right
            if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D))
            {
                //Constraint
                if (pos.X < GameManager.screenWidth - (texture.Width * GameManager.SCALE)) 
                {
                    pos.X += sideSpeed;
                }
            }
        }
```
There's a few things going on inside the player controls. First you check if the keyboard currently has buttons pressed down. For left control, we check if the Left Arrow Key or the A key is down, and for right control the Right arrow key or the D Key.

Then there is the constraint, to make sure the player doesn't move outside the window of the game.


> Because the origin position of sprites is located on the top left of the image, you have to offset it by the width of the texture. Here we also added the scale multiplier so that it always matches the sprite size: "texture.Width * GameManager.SCALE"
## Initalization
Now that you have prepared the player class, you will spawn in the player using the EntityManager class from the last section.

To start, go back to the EntityManager class and implement the following:
```csharp
//EntityManager
static class EntityManager
{
    //From Part 4
    static List<Entity> entities = new List<Entity>();
    
    //New Variables and Method
    public static Player player;
    static bool hasInitialized = false;
    
    public static void Initialize() 
    {
        if (hasInitialized == false)
        {
            player = new Player(SpriteArt.Player, new Vector2(GameManager.screenWidth / 2, GameManager.screenHeight / 1.2f));
            Instantiate(player);
            hasInitialized = true;
        }
    }
    ...
```
There are two new variables now, one representing the player and one that is a boolean that controls the initialize method.

Then there is the initialize method. Since this is a static class it does not require a constructor like the player. The reason that you need the bool is that this method will go inside the update function of your game and only needs to run once. This is so that all the sprites are loaded before update and would be ready to use after it has loaded.

Inside the method, you first need to create the player object. Using the player constructor we gave it a position to be at the center and on the bottom of the screen. It will also use the Player sprite specified in the SpriteArt class.

Now lets place this method into the Game1.cs file. Look for the Update method and implement the following:
```csharp
//Game1.cs
...
    protected override void Update(GameTime gameTime)
    {    
        EntityManager.Initialize();
        EntityManager.Update();
        base.Update(gameTime);
    }
...
```
Run your game, and you should be able to see the player and be able to control it:
![](~/images/first_2d_shooter/5_Player.gif)

## GameManager Settings
A small addition you can make is to give the player properties to the GameManager so that you have one file that controls all the properties. You will be able to change these on a whim without having to go through the files.

Head to the GameManager class and add in the following variable:
```csharp
//GameManager.cs
...
public static int playerSpeed = 5;
```

Then you can change the Player speed variable to the following:
```csharp
//Player.cs
int sideSpeed = GameManager.playerSpeed;
```



To see how to make the bullet class follow this link: [Part 6: Shooting](~/articles/tutorials/6_bullets_and_shooting.md)

## References and community tutorials

[MonoGame - adding basic code](~/articles/getting_started/5_adding_basic_code.md): This section has a good idea of more user movement to enhacne oyur game and get a better understanding of what your sprite is doing.
