# Part 6: Bullets and Shooting

In this section you will create a Bullet class and modify the player and EntityManager to be able to fire projectiles

## Bullet Class Setup
Create a new class file inside components called "Bullet", and replace the default code with the following:

```csharp
//Bullet.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace SpaceShooterDemo 
{
    class Bullet : Entity
    {
        //Bullet speed
        public int BulletSpeed = 8;
    }
}
```
> Same error will occur that happened with player, it will be fixed in the next few steps.


The variable BulletSpeed will control how fast the bullet will travel. We set it to 8 but you can set this number to fit your needs

### Constructor

To make the constructor we will need a couple things, a Vector2 and an Texture2D as the parameters.

Inside of the constructor we will have use the entity class variables and we will show you how to access them.

We will be obtaining the image and gaining the spawn position for the bullet:

```csharp
//Bullet.cs
public Bullet(Vector2 inital, Texture2D image) 
{
    texture = image;   
    pos = inital;
    pos.X -= (texture.Width * GameManager.SCALE) / 2;
}
```

> (texture.Width * GameManager.SCALE) / 2 offsets the bullet so that it is centered when it is spawned by the player. This time we subtract the values instead off adding them since the spawn position for bullets on  the player are also offsetted which will be later discussed.

After we construct the bullet we will need to override the function from the Entity class. This is because the abstract method "Update()" is not declared in the base class, this means that in every inherited class we need to implement one.

### Bullet Updates

Now you want to make sure the bullet moves from the player to the top of the screen. Lets create the Update method for the Bullet and implement movement just below the constructor
```csharp
//Bullet.cs
public override void Update() 
{
    pos.Y -= BulletSpeed;
    
    if (pos.Y <= 0)
    {
        isActive = false;
    }
}
```
While the bullet is updating, it will continue to move in a upwards direction. It will also check if the bullet is beyond the window, it will set isActive to false. This will signal the EntityManager to remove it from the game which we will go over in the next section.

## Entity Manager and Bullets
Before you can give the player any shooting logic, you need to register the Bullet object in the EntityManager class. Lets make the following adjustments to EntityManager:

```csharp
//EntityManager.cs
static class EntityManager
{
    //From Part 4
    static List<Entity> entities = new List<Entity>();
    
    //Specialized Bullet list
    static List<Bullet> bullets = new List<Bullet>();
    ...
```

Creating a new list for just Bullets allow you to update specific entities without checking the entire list. Now lets go to the Instantiate method and add the following:

```csharp
public static void Instantiate(Entity entity)
{
    entities.Add(entity);
    if (entity is Bullet) bullets.Add(entity as Bullet);
}
```
Any Bullets that are instantiated into the game will not only be added to the entities list but into the bullet list as well.

Finally we need to adjust how these lists are handled in the update method:
```csharp
public static void Update()
{
    //From part 4
    for (int i = 0; i < entities.Count; i++)
    {
        entities[i].Update();
    }
            
    //If any entities have isActive set to false, remove them from the lists
    entities = entities.Where(obj => obj.isActive).ToList();
    bullets = bullets.Where(obj => obj.isActive).ToList();
}
```
The logic here checks if any of the entities isActive is true, then keep them in the list. If they are set to false remove them from the list, thus they will also be removed from the game when isActive is set to false.

> Whenever an object is not in use, the object is released from memory because C# is a managed language. That's not to say you shouldn't do optimizations in your code, there's always ways to optimize the way to handle objects in your game.

## Player Shooting and Controls

This final section will go over how to make your player shoot and give it a fire rate.

Navigate to the Player.cs file, right below the variable for sideSpeed add the following variable:
```csharp
//Player.cs
//From Part 5
float sideSpeed = GameManager.playerSpeed;
//Firerate values
int cooldownRemaining = 0;
int fireRate = 10;
```
Firerate will control the rate in which the player can shoot, and cooldownRemaining tracks how many frames has passed since firing.

Now navigate to the Update method, and below the keyboard controls implement the following:

```csharp
//Player.cs
    ...
    //Shooting Logic
    if (Keyboard.GetState().IsKeyDown(Keys.Space) && cooldownRemaining <= 0)
    {
        Vector2 projectileSpawn = new Vector2 (pos.X + ((texture.Width * GameManager.SCALE)/2), pos.Y);
        cooldownRemaining = fireRate;
        EntityManager.Instantiate(new Bullet(projectileSpawn, SpriteArt.Bullet));
    }
    if (cooldownRemaining > 0) 
    { 
        cooldownRemaining--; 
    }
```
Here this checks when the keyboard has the spacebar pressed down, as well as if the cooldown remaining has elapsed.

Then a new Vector2 is constructed to ensure that the bullet is spawned at the center of the player regardless of scaling.

After that it sets the cooldown to equal the firerate and then spawns the bullet using the EntityManager Instantiation method.

If there still is a cooldown remaining, then just subtract it by one until it hits 0.

Now when you run the game you should be able to move your player and shoot with spacebar:

![](https://github.com/AlexJeter17/MonoGameStarShooter/blob/main/Docs/Content/6_ShootingDemo.gif)

Once you are finished you can continue on to [Part 7: Creating the enemy](https://github.com/AlexJeter17/MonoGameStarShooter/blob/main/Docs/Articles/7_Part%207%20Creating%20the%20Enemy.md)
