# Part 7: Creating the Enemy

In this section you will be creating the Enemy class using the inherited Entity class, and be able to instantiate them in the scene.

## Setup and Constructor
To start as always, begin by creating a new file called Enemy, and implement the following:
```csharp
//Enemy.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarShooterDemo
{
    class Enemy : Entity
    {
        public int health = 2;
        float dropSpeed = 1;
    }
}
```
> Same error as before with Player and Bullet, if there is an underline, it will be fixed in the next steps.

Here there are two variables, one to determine the health of the enemy and one that controls the move speed of the enemy.

Health and the functionality will be discussed in the collisions section.

Implement the constructor as such:
```csharp
//Enemy.cs
class Enemy : Entity
{
    public int health = 2;
    float dropSpeed = 1;
        
    public Enemy(int width, Texture2D image, float newSpeed, int healthPoints)
    {
        pos = new Vector2(width, 0);
        texture = image;
        dropSpeed = newSpeed;
        health = healthPoints;
    }
}
```
The constructor takes in four parameters and does the following:
* Set the current enemy position to a spot on top of the game scene where the enemy will move down from.
* Takes in a image to use as a texture
* Set a new move speed to the Enemy
* Set the maximum health points of the Enemy

The constructor is an important piece to the Enemy class as it allows you to create different types of enemies without having to rewrite code. 

## Update Method

Now that the constructor and variables are set we can look at how the Enemy object will update in the game scene.

For now, the Enemy should move down from the screen towards the player and delete itself when it reaches the edge of the screen.

The code snippet for the Update method should look like this:

```csharp
//Enemy.cs
...
public override void Update() 
 {
    //When Health is 0, remove itself
    if (health <= 0) 
    {
        isActive = false;
    }
    //When the Enemy reaches the edge of screen, remove itself
    if (pos.Y >= GameManager.screenHeight)
    {
        isActive = false;
    }
    //Apply movement to the Enemy by updating its position
    pos.Y += dropSpeed;
}
```
First we check if the current health is 0, if it is then remove itself. Health will be addressed in the Collisions section.

Next we check if the Enemy is out of the playing field as it moves down. If it is then it removes itself. More logic will be added to this to take player lives.

Finally it updates the movement of the Enemy to scroll down.

## Testing and Instantiating with Entity Manager

Just like the Bullet, you have to register the Enemy object with the Entity Manager. That means making a new list specifically for the Enemy. This will be important later as we handle collisions.

Navigate to the Entity Manager class again and from the top where all the lists are initalized, add the following new list:
```csharp
//EntityManager.cs
...
        //From previous sections
        static List<Entity> entities = new List<Entity>();
        static List<Bullet> bullets = new List<Bullet>();
        //The enemy list to add
        public static List<Enemy> enemies = new List<Enemy>();   
...
```

Once you've created the new enemy list, you need to reflect the same changes in update where it checks if the objects are active. Find the update method and add the following for the enemy list:
```csharp
//EntityManager.cs
public static void Update() 
{
        ...
        entities = entities.Where(obj => obj.isActive).ToList();
        bullets = bullets.Where(obj => obj.isActive).ToList();
        //Enemy list to update
        enemies = enemies.Where(obj => obj.isActive).ToList();
        ...
```

Now that the Entity Manager can check the list, you need to make sure new Enemies are added to the list. Find the Instantiate method and add the following snippet:
```csharp
//EntityManager.cs
public static void Instantiate(Entity entity) 
{
        entities.Add(entity);
        if (entity is Bullet) bullets.Add(entity as Bullet);
        else if (entity is Enemy) enemies.Add(entity as Enemy);
}
```

Any enemies added using the Instantiate method will be added to the entities list and the new enemies list.

To make sure the enemy class and entity manager are working together add the following temporary code to the Initialize method of the Entity Manager:

```csharp
//EntityManager.cs
public static void Initialize()
{
    if (hasInitialized == false)
    {
        player = new Player(SpriteArt.Player, new Vector2(GameManager.screenWidth / 2, GameManager.screenHeight - 200));
        Instantiate(player);
        //Manually Instantiate a new Enemy on startup
        Instantiate(new Enemy(
        GameManager.screenWidth / 2, //Spawn at the top center
        SpriteArt.Enemy1,            //Use the Enemy1 sprite from SpriteArt
        3f,                          //Set the new speed of Enemy to 3
        2                            //Health points of the Enemy
        ));
        hasInitialized = true;
    }
}
```

Now when you run the game you should have both the player and enemy on screen shown below:

![](https://github.com/AlexJeter17/MonoGameStarShooter/blob/main/Docs/Content/7_EnemyDemo.gif)

In the next section, you will create interactive collisions between the bullet and enemy: [Part 8: Collisions and Interactions](https://github.com/AlexJeter17/MonoGameStarShooter/blob/main/Docs/Articles/8_Part%208%20Collisions%20and%20Interactions.md)
