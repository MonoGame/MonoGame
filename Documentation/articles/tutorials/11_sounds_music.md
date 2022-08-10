# Part 11: Sounds and Music!

In this section of this tutorial you will be adding sound effects to your game such as Enemy death effects, Player shooting, and a Main menu music. This can help keep your game engaging for your audience.

## Overview
Adding Sounds inside of MonoGame uses the Audio and Media packages which use various sound file extensions for its implementation.

For example in this tutorial we will be dealing with .mp3 and .wav files. 
| File type: |Class correlation: |
|---|---|
.mp3 | Song class
.wav |  SoundEffect class

>This means that Song classes will not take in a .wav file and SoundEffect class will not take in .mp3 files

<!-- ADD SOUNDS HERE -->
## Downloads
You can download the sound effects and music [here](https://github.com/AlexJeter17/MonoGameStarShooter/tree/main/Docs/Sounds), which contains sound effects for shooting, explosions, and music.

You can also download and use your own sound effects too.

Now that you have them downloaded, you can go through the content.mcgb and add the sound files the exact same way as you did the sprites. 
> Open Content.mgcb -> Right click on Content -> Add -> Existing item -> Choose Sound Files 
> Alternatively you can also choose a folder that holds all the sound files and it will import the sounds as well.


## Content Loading and Setup
Lets start by adding some necessary headers for the **SpriteArt**, **Game1**, **Player file**, and **Enemy**. Put this in addition to your current headers:
```csharp
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
```

### Load Music and Sound Effects in SpriteArt

You can utilize the same method of loading music and sound effects under SpriteArt.
First create the reference variables for each of the sounds in SpriteArt class alongside with the texture references:

```csharp
//SpriteArt.cs
...
public static Song song;
public static SoundEffect explosion;
public static SoundEffect hpDown;
public static SoundEffect shootSound;
public static SoundEffect gameOver;
```

The song variable comes from the Song class and will take in the .mp3 file to play as background home screen music.

The rest are all .wav files that are going to be put in the SoundEffect class

Next in the SpriteArt class under the Load method, you should add the content loaders for the sound effects:

```csharp
//SpriteArt.cs
public static void Load(ContentManager content) 
{
    ...
    song = Content.Load<Song>("FreeSpaceMusic");
    explosion = Content.Load<SoundEffect>("Explosion");
    shootSound = Content.Load<SoundEffect>("ShootingNoise");
    gameOver = Content.Load<SoundEffect>("GameOver");
    hpDown = Content.Load<SoundEffect>("HealthDrop");
}
```
Now that all your sounds and music are loaded, its time to move on to editing the previous code to include the sound effects.

## In class edits for sounds

### Shooting Sounds for the Player

To start off, lets add the shooting sounds for the Player. Under the update method with the shooting logic, add the following sound:
```csharp
if (Keyboard.GetState().IsKeyDown(Keys.Space) && cooldownRemaining <= 0)
{
    //From Previous Sections
    Vector2 projectileSpawn = new Vector2(pos.X + ((texture.Width * GameManager.SCALE) / 2), pos.Y);
    cooldownRemaining = fireRate;
    EntityManager.Instantiate(new Bullet(projectileSpawn, SpriteArt.Bullet));
    
    //Play the shooting sound
    SpriteArt.shootSound.Play(0.25f, 0.0f, 0.0f);
}
```

The Play method for sounds takes in 3 parameters.
* Volume (float) - Determines how loud the sound will play from 0.0f to 1.0f. The default volume is 0.5f
* Pitch (float) - Determines the pitch of the sound
* Pan (float) - Determines where the volume is played 

The next step in adding sounds to the player is the Game over sound. You will also play the music for the main menu as well:
```csharp
if (hp <= 0)
{
    //From Previous sections
    GameManager.inGame = false;
    hp = GameManager.playerHealth;
    
    //Play Music, then the game over sound effect
    MediaPlayer.Play(GameManager.song);
    SpriteArt.gameOver.Play(0.5f, 0.0f, 0.0f);
}
```
The MediaPlayer is a static class built into the Media packages. It allows us to play songs in game.

### Explosion and Hit Sound Effects for Enemy
In the next section you will be adding small lines of code for the Enemy sound effects. You will add sound effects for enemy death and when the enemy reaches the player.

Navigate to the Enemy class and under the Update method where the health check occurs add the explosion line:
```csharp
if (health <= 0)
{
    //Play the explosion sound when the enemy is dead
    SpriteArt.explosion.Play(0.5f, 0.0f, 0.0f);
    //From Previous sections
    GameManager.score += ((int)dropSpeed - 1);
    isActive = false;   
}
```

Next add the hpDown sound to the logic where the enemy reaches the player in he same Update method:

```csharp
if (pos.Y >= GameManager.screenHeight)
{
    SpriteArt.hpDown.Play(0.25f, 0.0f, 0.0f);
    isActive = false;
    EntityManager.player.hp -= 1;
}
```
You've finished the sounds for the enemy, the next section will finalize the implementation of the music in the Game1.cs class.

## Finalizing the Music in Game1

Finally, you will have to implement the music in the Game1 file. You will handle playing the music and stopping the music in this file.

Navigate to the Game1.cs file. Find the LoadContent method and add the following lines to play music:

```csharp
protected override void LoadContent()
{
    //From Previous Sections
    _spriteBatch = new SpriteBatch(GraphicsDevice);
    SpriteArt.Load(Content);
    
    //Play Media on startup
    MediaPlayer.IsRepeating = true;
    MediaPlayer.Play(SpriteArt.song);
    MediaPlayer.Volume = .5f;
}
```
First you set the MediaPlayer to repeat to true so that the music loops, then you play the song and set the volume of the song.

> It is generally better to control playing music inside the update method with additonal logic to control. In this case we will load the music inside LoadContent.

Next we want to stop the music when the player is in game. This is a simple Stop command to the MediaPlayer:

```csharp
if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !GameManager.inGame)
{ 
    //From Previous Sections
    GameManager.score = 0; 
    GameManager.inGame = true;
    WaveManager.Reset(); 
    EntityManager.ClearEntities(); 
    
    //Stop the music when the player goes in game
    MediaPlayer.Stop();
}
```

Now when you play the game, you should be able to hear the sound effects and the main menu music.

Once you are ready, you can move on to the final section of this tutorial which is packaging your game to share with people. [Part 12: Packaging and Delivering for Desktop](~/articles/tutorials/12_package_delivery.md)

## References and community tutorials

[Audio - Monogame](https://gamefromscratch.com/monogame-tutorial-audio/): This tutorial shows how to connect sounds with your keyboard amongst other cool sound ideas.

[MonoGame Microsoft.Xna.Framework.Audio](https://docs.monogame.net/api/Microsoft.Xna.Framework.Audio.html): This link shows all of monogames audio files it supports and shows links to each type so you can choose the best and most effeciant sounds for your game.
