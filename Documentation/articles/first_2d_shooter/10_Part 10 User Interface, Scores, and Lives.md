# Part 10: User Interface, Scores, and Lives

The User Interface is start of the game, so we will be interpreting some new logic to functions and adding some new variables and methods.

>Some notable things to think about.
* How are we going to start the game?
* What will we put as the background here?
* How will people know how to start the game?
 
This article will answer all these questions and hopefully lead you into more of your own creative ideas for starting a game.

**The goal of this article is to ensure you have a basic and working User Interface. That will look something like this:**
![](https://i.imgur.com/6cAgYYF.gif)


## Set up your SpriteFont!

We will be uploading a font in a similar manner to the way we create sprites.
1. **Open up content.mgcb**
2. **Right click on the content file, press add and then new item.**
![](https://i.imgur.com/TKv5NvS.png)

3. **Click Spritefont description and give the file a name then click create.**
![](https://i.imgur.com/K1culLo.png)

4. **Double click on the new file in content.mgcb and youll see this:**
![](https://i.imgur.com/wecshue.png)

5. **Now change the SIZE to 30**

Now that you have setup the font asset, you will also have to load it in game as well. This is pretty much the same steps as if you were to load in Sprites from Part 2.

> If you want to use different fonts you can add as many as you want into your local file. Then you can create multiple spritefont files in your mgcb content then load those files to use different fonts.

Navigate to the SpriteArt class, similar to how you load Sprites in, you can load in the font as well:
```csharp
//SpriteArt.cs
static class SpriteArt 
{
    public static SpriteFont font { get; private set; }
    ...
}
```

After this you will need to load the font into the code. Add this to the Load method of SpriteArt alongside the Texture2D files:
```csharp
//SpriteArt.cs
public static void Load(ContentManager content)
{
    ...
    font = Content.Load<SpriteFont>("File2");
}
```
Set it to load the spritefont file that you just created. Now the font is loaded and ready to use in your game.

## Setup, Entity Clearing, and Player Modifications

Before you get started on the user interface, there are some additions that you must make for this section of the tutorial.

The reason behind these additions is that you will also be implementing a quick main menu screen and be able to restart the game if the player loses the game (Lives reaches 0) as well as a score feature.

Restarting a game has a lot of logic behind it, but it is a essential feature for replayability in games. This segment of the tutorial will guide you through in designing the fundamentals for restarting the game.

### GameManager variable setups

In the GameManager, there are a few variables that we want to store and track for the game. They are the Player health, the player's score, and a bool that represents the current state of the game. The bool will be used to determine if the game is in the home screen or in game. 

Add the following variables to the GameManager class:

```csharp
//GameManager.cs
...
public static bool inGame = false;
public static int playerHealth = 5;
public static int score = 0;
```
### Player Modifications and Enemy Scoring

The player is a unique entity in that it is the controllable character for the user. One feature that you'll be implementing here is player lives. 

The idea is that whenever the Enemy goes beyond the screen boundaries, you punish the player for missing the enemy by removing lives. And when the health of the player is 0, the game ends.

You also want to reward the player for eliminating enemies. You will also be implementing a score system for destroying enemies and later on display the score.

To start, lets navigate to the Player class and to the Update method. This will be the health check, where the game will check to see if the Player is still alive or not:

```csharp
//Player.cs
public override void Update() 
{
    if (hp <= 0)
    {
        GameManager.inGame = false;
        hp = GameManager.playerHealth;
    }
}
```

Whenever Health is 0 or less, set the inGame boolean to false, and reset the HP of the player to prepare for the next session. You will utilize the inGame boolean later in the tutorial to control when the game starts.

Next, navigate to the Enemy class and to the Update method for the Enemy as well. Here you will modify the logic that handles the health of the enemy and reaching the edge of screen with scoring and removing player lives:

```csharp
//Enemy.cs
public override void Update() 
{
    if (health <= 0)
    {
        isActive = false;
        GameManager.score += ((int)dropSpeed - 1);
    }
//When the Enemy reaches the edge of screen, remove itself
    if (pos.Y >= GameManager.screenHeight)
    {
        isActive = false;
        EntityManager.player.hp -= 1;
    }
    ...
}
```

`GameManager.score += ((int)dropSpeed - 1)` adds score whenever the enemy health reaches 0. The way score is applied is based off of how fast the enemy is. So the faster the enemy, the more score you get for destroying it.

`EntityManager.player.hp -= 1` removes the health of the player by 1 whenever the enemy leaves the screen. EntityManager contains the reference for the player so the Enemy can easily call this value.

### Wave Manager Resets
Whenever you restart the game you want to make sure that it resets the wave counter and the number of enemies to spawn. Create a new method called Reset and add the following:

```csharp
//WaveManager.cs
...
public static void Reset()
{
    wave = 0;
    remainingEnemiesToSpawn = 0;
    maxNumEnemies = 2;
}
```
The reset method will set the wave number to 0, reduce the enemy spawns to 0, and maximum number of enemies to 2. So whenever you start a new wave, the Initialize method will set the default values every time you restart the game. 

Later on in this section you will be able to implement this method.

### Entity Manager Resets

When you want to restart the game, you also don't want to have any leftover enemies from the previous session carry over to the next one. So everytime you want to restart you have to clear all the enemies and bullets off the board.

Navigate to the EntityManager class, and create a new method called Reset() similar to the method you just created for WaveManager. Add in the following code:

```csharp
//EntityManager.cs
...
public static void Reset()
{
    for (int i = 0; i < entities.Count; i++)
    {
        if (entities[i] is Enemy || entities[i] is Bullet)
        {
            entities[i].isActive = false;
        }
    }
    enemies.Clear();
    bullets.Clear();
}
```

The for loop is to remove any entities in the entities list that is not the player. You do not want to remove the player since the player ties into some functionality for the EntityManager.

Then you clear both of the enemies and bullets list since they are not needed for the next game session.

We will be implementing these reset functions later in this section of the tutorial.

## Creating the User Interface


Start off by going to your components foler and adding a new class file called "UserInterface" and add the following code for setup:

```csharp
//UserInterface.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarShooterDemo
{
    static class UserInterface
    {
        public static bool hasStarted = false;
        private static string titleString = "MonoGame Star Shooter Tutorial!";
    }
}
```
`public static bool hasStarted = false` is the variable that controls the one time initialization for the titleString content. It is similar to the variable for initalizing the player in the EntityManager class.

`private static string titleString = "My First Game1"` is a string that will be displayed as the header when starting the game and when a game over occurs. The string will later be modified to display the score at the end of a game.

### Creating the Home Screen

This is going to add a few things that are similar. First off we will need the font variable that we implemented earlier for writing HP, score, and wave number. 

So inside the HomeScreen function, it will contain:

```csharp
//UserInterface.cs
public void HomeScreen(SpriteBatch _spriteBatch, SpriteFont font) 
{
    if (!hasStarted) titleString = "My First Game!";   
    else  titleString = "Game Over! | Score :" + GameManager.score; 
    
    _spriteBatch.DrawString(font, "Press ENTER to start a new game!", new Vector2(GameManager.screenWidth / 10, GameManager.screenHeight / 2), Color.WhiteSmoke);
    _spriteBatch.DrawString(font, titleString, new Vector2(GameManager.screenWidth / 10, (GameManager.screenHeight / 2) - 100), Color.Red);
}
```
First, the method takes in two parameters: `SpriteBatch _spriteBatch, SpriteFont font` which is the spriteBatch object from the Game1 main file, and the font that you will be using.

Next you want to check if the game hasn't been already played once, indicated by the `hasStarted` value. If it hasn't then set the title string to the title of your Game, in this case it is "My First Game!"

If the game has already had a session of playtime, then instead of displaying the title of your game, you can display the score of the last session. 

`DrawString()` function takes in 4 parameters which are (SpriteFont, String, Position, Color) - This can be seen above as (font, titleString, new Vector2(...,...), Color.whiteSmoke)


### Adding Information in game

The next step is adding information when the user is playing the game. Here you want to display the current wave the player is on, the score, and how much health the player has left.

Start by creating a new method called GameScreen with the same parameters as the home screen and implement the following code:
```csharp
public static void GameScreen(SpriteBatch _spriteBatch, SpriteFont font)
{
    _spriteBatch.DrawString(font, "Score: " + GameManager.score, new Vector2(10, 10), Color.White);
    _spriteBatch.DrawString(font, "Wave: " + WaveManager.wave, new Vector2(GameManager.screenWidth - 200, 10), Color.White);
    _spriteBatch.DrawString(font, "HP: " + EntityManager.player1.hp, new Vector2(400, 10), Color.White);
}
```

The first line draws the score on the top left of the screen, the second draws the wave number on the top right and the third line draws the remaining health right next to the wave number.

### Modifying the Draw and Update methods in Game1

To apply all the additions you just created you will need to make some modifications to both the Draw and Update methods inside your Game1 main file.

#### The Update Method

The first will be in the Update() function, here you will need to add logic on how the game will restart. 

Implement the following code under the Update method:
```csharp
protected override void Update(GameTime gameTime)
{
    //When the Player is in the main menu, check if they hit the Enter key
    if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !GameManager.inGame) 
    { 
        GameManager.inGame = true;
        WaveManager.Reset(); 
        EntityManager.ClearEntities(); 
        GameManager.score = 0; 
    }
    //When the player is in game, start the Entity updates.
    if (GameManager.inGame) 
    {
        UserInterface.hasStarted = true;
        EntityManager.Initialize();
        EntityManager.Update();
        WaveManager.Update();
    }
    base.Update(gameTime);
}
```
The first statement block checks if the Player is in the main menu. If the player hits the enter key to start/restart the game, then we want to make sure that the game ready to be played. 

Here you first set GameManager's inGame value to true to indicate that the user is playing the game. Then you want to reset the waves using them method you designed earlier. After that we want to clear all entities and reset the score.

The next statement just checks if the current status of the game is that the user is playing. The code inside is logically the same beforehand, with the added UserInterface variable that determines if the game has started at least once. After that just let the game run the updates on the Entities.


#### The Draw Method

The final step is modifying the Draw method. Here is how you would control what portions of the User Interface you want to display.

Modify the Draw method with the following:
```csharp
protected override void Draw(GameTime gameTime)
{
    _spriteBatch.Begin();
    _spriteBatch.Draw(SpriteArt.backGround, new Rectangle(0, 0, GameManager.screenWidth, GameManager.screenHeight), Color.WhiteSmoke);
     
    if (GameManager.inGame) // if inGame is true
    {
        UserInterface.gameScreen(_spriteBatch, font);
    }
    else // else inGame must be false
    {
        UserInterface.HomeScreen(_spriteBatch, font);
    }
    
    EntityManager.Draw(_spriteBatch);
    _spriteBatch.End();

    base.Draw(gameTime);
}
```

The new additions is the if statement blocks added here. These statements let us control what part of the user interface to display depending on the current game status.

The first statement checks if the player is in game, then you want to display the user interface that handles the in game information

The next statement then checks if the player is not in game, otherwise in the main menu. Then the user interface will display the home screen portion of the game.

The draw methods for EntityManager and Background are untouched otherwise, however the order in which they are drawn matters. The one that is drawn recently will be drawn on top of everything else.

Now you can run the game and see the user interface in action:

<img src="https://i.imgur.com/6cAgYYF.gif" width="230"/> <img src="https://i.imgur.com/DRbLJMA.gif" width="230"/> <img src="https://i.imgur.com/6VxyjaB.gif" width="230"/>

Congrats! You now have a working user Interface! For [Part 11: Sounds and music](https://hackmd.io/-H9VGZWaRLO1xyZM40_Taw) you will learn how play sounds and music inside your game
