# Part 9: Wave Manager Class

In this section, we will be taking a look at spawning enemies into the game automatically.

The WaveManager class will help you spawn enemies without having to manually add them. It will handle all the logic in wave spawning including ever increasing enemy count, spawn rates, and waves.
## GameManager Settings
Before you begin, lets add some new values to the GameManager. These values will control how much the enemy health has, and how fast it can move. Add the following to GameManager:
```csharp
//GameManager.cs
...
public static int level1Health = 2;
public static float level1Speed = 3f;

public static int level2Health = 3;
public static float level2Speed = 5f;
```

## WaveManager class setup
Once you setup these variables in GameManager you can now create a new class file called WaveManager. You will add the following variables here:
```csharp
//WaveManager.cs
using System;
namespace StarShooterDemo
{
    static class WaveManager
    {
        public static int wave = 0;

        static int spawnRate = 50;
        static int spawnDelay = 0;
        static int maxNumEnemies = 0;
        static int remainingEnemiesToSpawn = 0;
        
        static Random random = new Random();    
    }
}
```
The WaveManager contains a few variables to control the waves and spawning of enemies.

* Wave represents the current wave number. This is equivalent to levels and later on will be display in the user interface.
* Spawn rate, which is the delay between spawning enemies. The lower it is, the faster the enemy will spawn.
* Spawn Delay, similar to the fire rate counter keeps track of the delay between spawns.
* Max Number of Enemies, which is the number of enemies that can spawn in a wave.
* Remaining Enemies, the number of enemies left to spawn in a wave.
* Random object, which is used to randomize the spawn points of the enemy.

## Initalizing Waves

First, we need a method to start the waves. This method will first check if there are no enemies left on screen and that all enemies have spawned from the current wave. Then it can send in the next wave.

Create a new method called InitializeWave:
```csharp
//WaveManager.cs
public static void InitializeWave()
{
    if (EntityManager.enemies.Count == 0 && remainingEnemiesToSpawn == 0) 
    {
        wave += 1;
        remainingEnemiesToSpawn += maxNumEnemies;
        maxNumEnemies += 2;
    }
}
```

> Ensure that the enemies list in Entity Collections is a public variable, otherwise the protection level will prevent WaveManager from accessing it

When the WaveManager starts a wave, it will reset the remaining Enemies to spawn to the maximum number of enemies for that wave, and add 2 more enemies to the next wave. You can adjust the number of enemies that can spawn per wave by adjusting the maximum number.

## Creating Enemies and Spawn Conditions

The WaveManager needs to also have an Update method similar to the EntityManager. Just below the InitalizeWave method, add a new method called Update. Here we will process logic for starting waves and spawning enemies.

```csharp
//WaveManager.cs
public static void Update()
{
    //The method that you created earlier
    InitializeWave();
    //Spawning logic
    //First check if the spawn delay is 0 and 
    //that there are no enemies left to spawn form current wave
    if (spawnDelay <= 0 && remainingEnemiesToSpawn != 0)
    {
        //Every even enemy is a level 1 enemy, every odd enemy is a level 2 enemy
        if (remainingEnemiesToSpawn % 2 == 0)
        {
            //Subtract 1 from remaining enemies, then spawn the enemy object using EntityManager
            remainingEnemiesToSpawn -= 1;
            
            //Spawn enemy using the Instantiate method from EntityManager
            EntityManager.Instantiate(new Enemy( 
                random.Next(128,(GameManager.screenWidth - 128)),    //Position to Spawn Enemy
                SpriteArt.Enemy1,                              //Texture for the Enemy
                GameManager.level1Speed,                             //Speed of the Enemy
                GameManager.level1Health                             //Health of the Enemy
            ));
        }
        else
        {
            remainingEnemiesToSpawn -= 1;
            EntityManager.Instantiate(new Enemy(
                random.Next(128, (GameManager.screenWidth - 128)),
                SpriteArt.Enemy2,
                GameManager.level2Speed,                                                  
                GameManager.level2Health
            ));
        }
    //Reset the spawn delay to the spawn rate.
    spawnDelay = spawnRate;

    }
    //If the spawn delay is not 0, subtract 1 from it.
    if (spawnDelay > 0) spawnDelay--;
}
```

Whenever you spawn an enemy, you subtract the remaining enemies for that current wave and reset the spawn delay to spawn the next enemy.

While there is a current spawn delay, subtract one off from the delay. Once it reaches zero the cycle begins again with spawning another enemy.

When spawning an enemy, the constructor requires the position to spawn, the sprite, the speed and level of the object. Here we offset the values by 128 so that the enemy does not spawn on the edges of the screen. Then you can put the different enemy sprites in and use the GameManager variables to set the speed and health.

## Updating in Game1 main file

Navigate to the Game1.cs file, and you just have to add one line inside the Update method right below the EntityManager to update the WaveManager
```csharp
//Game1.cs
...
protected override void Update(GameTime gameTime)
{
    EntityManager.Initialize();
    EntityManager.Update();
    WaveManager.Update();
    base.Update(gameTime);
}
```

Now when you run the game you should be able to shoot down all the enemies and will continously spawn:

![](https://i.imgur.com/rioGP7W.gif)


Now that your game has spawning mechanics for your enemies, you can move on to the next section which is user interface: [Part 10: User Interface!](https://hackmd.io/LTnckC_cSl29XFo7DUog-Q)

