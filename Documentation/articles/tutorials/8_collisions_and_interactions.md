# Part 8: Collisions and Interactions
In this section of the tutorial you will be going over how to add collisions and add interactions between the Bullet and Enemy class. This includes a knockback effect and a tint when an enemy is hit with a bullet.

## Overview
This is a basic idea of how collisions will work and how the hitboxes look:
![](~/images/first_2d_shooter/8_CollisionsVisual.png)

The idea of adding collisions is by creating boxes around the sprite that determines the hitbox. You can then use these boxes and calculate if they are touching. If they are touching then you can call some logic that handles those interactions.

The red and purple boxes around the sprites are the hitboxes you will create in this tutorial. Whenever these two boxes intersect with each other you can call a function and apply any logic regarding collisions.

This article will go through some of the classes that you have made and modify them to include collisions and interactions. This will focus solely on deleting the bullet when it finds an enemy, and apply a on hit-effect to the enemy with a red tint color and knockback effect as well as removing hitpoints.

## Modifiying the Entity class

Navigate to the Entity class file in the Components section. You will need to add a new variable to the Entity class that represents the hitbox of the entity:

```csharp
//Entity.cs
abstract class Entity 
{
        protected Texture2D texture;
        public Vector2 pos;
        public bool isActive = true;
    
        public Rectangle hitbox;
        ...
```

The Rectangle struct takes in a integer values of x and y coordinates indicating where the box should be made starting from the top left corner of the rectangle, and the size of the rectangle which will be the texture with the scaling factor.

Create a new function below the draw method called createHitbox as shown below:

```csharp
//Entity.cs
public virtual void createHitbox()
{
    hitbox = new Rectangle((int)pos.X, (int)pos.Y, (int)(texture.Width * GameManager.SCALE), (int)(texture.Height * GameManager.SCALE));
}
```
The method will create a box surrounding the entity's sprite, no matter what the scale might be.



When creating the bounding box over the objects, you want to make sure that the hitbox matches the size of the sprite, so multipling the width and height with scale will match the sprite scale as well.

After we create the hitbox we need to update it every frame as the entity will also be changing position.

```csharp
public virtual void updateHitbox()
{
    hitbox.X = (int)pos.X;
    hitbox.Y = (int)pos.Y;
}
```


This will make sure that no matter what, the hitbox is the same frame as the entity at hand.

Now that we have these set up, lets get to some uncommenting of lines.

## Adding Collision box to Enemy
Now that you have created the functions to build the rectangle around the entity you will need to apply them to the Enemy and Bullet.

Navigate to the Enemy class and look for the constructor and add the following:
```csharp
//Enemy.cs
public Enemy(int width, Texture2D image, float newSpeed, int healthPoints)
{
        //From Part 4
        pos = new Vector2(width, 0);
        texture = image;
        dropSpeed = newSpeed;
        health = healthPoints;
        //Create the hitbox for the Enemy object
        createHitbox();
}

```
Then look for the update function and add the following:

```csharp
//Enemy.cs
public override void Update() 
{
    ...
    //From Part 4
    pos.Y += dropSpeed;
    //Update hitbox every frame
    updateHitbox();
}
```
Whenever you create a new Enemy object, it will come with a rectangle hitbox around it and update the position of the hitbox as it moves around.

Now you need to apply this to the Bullet, shown below.

## Adding Collision box to Bullet

Just like the Enemy class, you will need to add the functions to create the hitbox and update them. Again going to the Bullet class constructor add the following:

```csharp
//Bullet.cs
public Bullet(Vector2 initial, Texture2D image) 
{
    ...
    createHitbox();
}
```

Then add the updates to the hitbox in the Update method:
```csharp
//Bullet.cs
public override void Update()
{
    pos.Y -= BulletSpeed;

    if (pos.Y <= 0)
    {
        isActive = false;
    }
    //Update hitbox here
    updateHitbox();
}
```
Now that both entities have hitboxes, its time to add the logic when they interact with each other.
## Registering and Adding Logic to Collisons

To check if any of the bullets or enemies are interacting with each other, you can go through the specialized lists inside of EntityManager and go through them.

Then you can check if the hitboxes intersect with each other and if they do you can run some logic.

Navigate to the EntityManager's update method, and just below the code where you check if the enemies are active, add the following:

```csharp
//EntityManager.cs
public static void Update()
{
    ...
    for (int i = 0; i < enemies.Count; i++)
    {
        for (int j = 0; j < bullets.Count; j++)
        {
            //Check if any of the enemies or bullets collide with each other
            if (bullets[j].hitbox.Intersects(enemies[i].hitbox))
            {
                //Subtract enemy health by one, remove bullet
                enemies[i].health -= 1;
                bullets[j].isActive = false;
            }
        }
    }
}    
```

Here we check each enemy and bullet entity against each other and if they collide using the Intersects method for Rectangles, then we can reduce the enemy health by one and delete the bullet.

Now when you run the game, and if you have kept the enemy initial spawn from the last part you will be able to shoot down the enemy:

![](~/images/first_2d_shooter/8_CollisionsBasic.gif)


## Adding Hit Effects

Shooting down an enemy isn't exactly satisfying as shown above. Adding a effect whenever the enemy gets hit will give it more of an impact and be much more visually appealing.

To start, go to the Enemy class, and on the lines containing the variables for health and drop speed, add the following:
```csharp
//Enemy.cs
class Enemy : Entity 
{
    public int health = 2;
    protected float dropSpeed = (1 * GameManager.SCALE);
    
    //Variables to control hit effects
    protected Color tint = Color.White;
    private int hitCooldown = 0;
    private int hitFrames = 10;
    private int knockbackMultiplier = 3;
}

```
There's quite a few variables that we've added here. These will be fundamental to adding a knockback effect and a red tint.

* Color tint : Controls the current tint of the sprite. Defaults to Color.White 
* hitCooldown : Keeps track of how many frames are left when applying the hit effect
* hitFrames : How long should a hit effect last
* knockbackMultiplier : How far should the enemy be knocked back when hit

Now you need to add logic to the tint, one way to add the tint to the Enemy object is to override the draw method.

In the Entity class, the Draw method has a `virtual` keyword. This allows us to override the draw method in a different inherited class.

Add a new method just below the update function with the Draw method:

```csharp
//Enemy.cs
...
public override void Update() { ... }

public override void Draw(SpriteBatch spriteBatch)
{
    spriteBatch.Draw(
    texture,                        
    pos,                            
    null,                           
    tint,                       //Tint Color
    0f,                             
    Vector2.Zero,                   
    GameManager.SCALE,       
    SpriteEffects.None,             
    0f);
}
```
The different between the Draw method in the abstract class is that we replaced the tint value with the one that we set in the Enemy above.

Now you need to apply the effects to the Enemy. First you will need to create two functions inside of Enemy, one that calls the hit and another one that applies the visual effects.

Create a new method just below the overridden draw method inside enemy with the following:
```csharp
//Enemy.cs
...
public void OnHit() 
{
    health -= 1;
    hitCooldown = hitFrames;
    pos.Y -= dropSpeed * knockbackMultiplier;
}
```
Here the on hit method will remove one health from the enemy, set hit cooldown to hit frames which will go over in the next method, and apply a knockback effect to the Enemy.

You need to do a minor change in EntityManager Update() method and replace the health subtraction inside of the collision logic:

```csharp
//EntityManager.cs
if (bullets[j].hitbox.Intersects(enemies[i].hitbox))
{
    //Collision logic here, if enemies are hit, call OnHit and remove bullet
    //enemies[i].health -= 1;
    enemies[i].OnHit();
    bullets[j].isActive = false;
}
```

After that, go back to the Enemy class, and make one more method. This one will control how long the sprite should be tinted red:

```csharp
//Enemy.cs
...
private void OnHitEffect()
{
    if (hitCooldown >= 0)
    {
        hitCooldown -= 1;
        tint = Color.Red;
    }
    else
    {
        tint = Color.White;
    }
}
```
The logic is similar to the fire rate for the player in the Shooting article, where you have a variable that controls the delay and a check to see if the delay has elapsed or not.

Finally, go to the update method for Enemy, and just after you update the hitbox, add the OnHitEffect:
```csharp
//Enemy.cs
public override void Update() 
{
    ...
    updateHitbox();
    OnHitEffect();
}
```
Now when you run the game and hit the enemy you should see a knockback effect and a red tint applied when hitting the enemy:

![](~/images/first_2d_shooter/8_CollisionsHitEffect.gif)

## Next Steps

Once you are finished, the next part will go over infinitely spawning enemies: [Part 9: Wave Spawning](~/articles/tutorials/9_wave_manager_class.md)
