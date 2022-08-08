# Part 4: Creating the Entity Manager
In this section, you will be creating a class that handles the properties and updates of all your entities in the game. 

This article will control all the updates and draw methods of every entity, and later on in the series this will also manage collisions.

## The need for an Entity Manager
In the last section [Part 3: Creating the Entity class](https://github.com/AlexJeter17/MonoGameStarShooter/blob/main/Docs/Articles/3_Part%203%20Creating%20an%20Entity%20class.md) you designed an Entity class that you can use to branch off and make the player and enemies. However you need a way to track all the objects on screen, and update them. Putting this logic inside the main file will quickly be difficult to read.

The solution is to make a new class that handles all the functionalities of the Entities on screen. 

## Setup and List Adding
To start, create another class file inside the Componenets folder. Name this class EntityManager and include the following:

```csharp
//EntityManager.cs
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarShooterDemo
{
    static class EntityManager
    {
        static List<Entity> entities = new List<Entity>();
    }
}
```


Next we need to add these entities to the list. To do this we construct a publically available method that adds our Entities into the list.

```csharp
//EntityManager.cs
//Create the entity on screen and add them to an appropriate list
public static void Instantiate(Entity entity)
{
    entities.Add(entity); // adds all entities to total Entity list
}
```

Later on in the series, you will add on top of this method to include adding entities to specific list types, such as lists for just enemies and bullets. These specialized lists will allow you to update only specific entities.

## Drawing and Updating
In the previous section, you created a draw method for entities. Here we will put this method to use in which this method will draw all the entities in the list as shown here:

```csharp
//EntityManager.cs
public static void Draw(SpriteBatch spriteBatch)
{
    for (int i = 0; i < entities.Count; i++)
    {
        entities[i].Draw(spriteBatch);
    }
}
```

Finally we will begin the Update method which is the most important of this class. This method will allow you to update all entities inside the list. 

This will loop through all the entities in the list and call the update method for each entity.
```csharp
//EntityManager.cs
public static void Update()
{
    //Update all entities on screen
    for (int i = 0; i < entities.Count; i++)
    {
        entities[i].Update();
    }
}
```

## Applying to the main file
Now that you have methods to update and draw entities, you will need to add the functionality in the main file (Game1.cs).

Find the Update and Draw functions inside of Game1.cs and add the EntityManager functionality:

```csharp
//Game1.cs
...
    protected override void Update(GameTime gameTime)
    {
        EntityManager.Update();
        base.Update(gameTime);
    }
    protected override void Draw(GameTime gameTime) 
    {
        ...
        _spriteBatch.Begin();
        EntityManager.Draw(_spriteBatch);
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }
```
When you run your game, nothing should happen. We haven't added any entities to the list so nothing appears. In the next section you will create the Player class and utilize the EntityManager.

## Moving On

Now lets go to the next article which is [Part 5: Creating the Player Class](https://github.com/AlexJeter17/MonoGameStarShooter/blob/main/Docs/Articles/5_Part%205%20Creating%20the%20Player.md)
