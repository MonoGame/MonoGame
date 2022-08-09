# Part 3: Creating an Entity class
Continuing from [Part 2: Handling Sprites](https://github.com/AlexJeter17/MonoGameStarShooter/blob/main/Docs/Articles/2_Part%202%20Handling%20Sprites.md), this section will go over how to create a abstract Entity class that will represent all the objects in the game. This includes the player, enemies, and bullets.

Having a common class for all of the objects in the game will allow you to reuse code easily without having to rewrite it everytime you need it. You can run the same methods between all your objects in game and still have the flexibility of adding new and different types of objects into your game.

If you have some experience in developing in other game engines, this will look similar to things like the ```GameObject``` for Unity or ```UObject``` for Unreal, except you will be making this class from scratch.

## Setting Up the Entity class
To start, create a new class file inside the Components folder. Call this file ```Entity```
Inside this class file make these changes shown below:
```csharp
//Entity.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarShooterDemo
{
    abstract class Entity 
    {
        //Add common methods here
    }
}
```
The `abstract` keyword in the class allows us to indicate to other derived classes that there's some implementation needed. 

The Entity class will act as a template that you can build off of for other objects such as the player and enemy. It also allows you to reuse code without rewriting everytime.

For example, some of the common componenets between the player and enemy are that it contains a texture/sprite, a method to update every frame, and a value that acts as the position.

You can tie these common components together in the Entity class:
```csharp
//Entity.cs
abstract class Entity 
{
    //Variables
    protected Texture2D texture;
    public Vector2 pos;
    
    //Abstract Methods
    public abstract void Update();
```
Here the Entity class now contains information for texture and a new type of data called a Vector2. A Vector2 is a point in space in the game. Here we are using a Vector2 called pos to indicate the current position of the Entity.

There is also a method here called Update. It is an abstract method so the implementation of Update must be handled in other classes that inherit Entity, which you will see how to add these implementations when designing  the Player, Enemy, and Bullet objects.

The Player, Enemy, and Bullet entities also need a way to be drawn onto the game so adding a common Draw method is necessary as well as including the scaling of the images using the scale factor in the GameManager class:


```csharp
//Entity.cs    
    public abstract void Update();
    //Common Draw Method
    public virtual void Draw(SpriteBatch spriteBatch) 
    {
        spriteBatch.Draw(
                texture,                      //Texture 
                pos,                          //Position
                null,                         
                Color.White,                  
                0f,                           
                Vector2.Zero,                 
                GameManager.SCALE,     //Scale
                SpriteEffects.None,           
                0f);    
    }  
}
```

There are 3 important values in this draw method that we'll take a look at. The texture, position, and the scale. You can set the texture and position values to the values you made  for the Entity. 

Scaling on the other hand is handled by the GameManager. So whatever value you have in GameManager it will translate back here.

In the next section [Part 4: Creating the Entity Manager](https://github.com/AlexJeter17/MonoGameStarShooter/blob/c520d0341a6dcfdf8a2db12b0007155423c476e1/Docs/Articles/4_Part%204%20Creating%20the%20Entity%20Manager.md) you will create a managing class for all entities in the game.



