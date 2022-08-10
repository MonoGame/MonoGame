# Part 2: Handling Sprites
This section will go over how to handle sprites in a more organized way and easier to find, these will also include sprites you can download that will be used throughout this tutorial.

One thing to keep in mind as we go through this tutorial is the viewpoint of the sprites and how they are visualized in code. Here is an image to make you visualize it easier.

![](~/images/first_2d_shooter/2_Coords.png)

This being said, people often use the first quadrant of a graph, or goes from bottom left to top right for sizing. With sprites it actually goes from top to bottom, where the top = 0 and the bottom is = screen length.

## Assets
Before you start, download these images as a PNG as it will be part of the game you will be building in this tutorial series. You will need to download all of them.

Right click the image, then click **Save as Image**. Make sure to save it as a PNG. Alternatively, you can download the whole assets folder from the github [here](https://github.com/AlexJeter17/MonoGameStarShooter/tree/main/Docs/Sprites).

![](~/images/first_2d_shooter/Sprites/PlayerShip.png) PlayerShip 

![](~/images/first_2d_shooter/Sprites/EnemyTier1.png) EnemyTier1

![](~/images/first_2d_shooter/Sprites/EnemyTier2.png) EnemyTier2

![](~/images/first_2d_shooter/Sprites/Bullet.png) Bullet

![](~/images/first_2d_shooter/Sprites/StarBackground.jpg) StarBackground



Make sure to add these images into your Content mgcb editor of your project. If you don't know how to add content, see this article [Adding Content](~/articles/getting_started/4_adding_content.md)

## Creating a new Class to contain your Sprites
In the [Adding Content](~/articles/getting_started/4_adding_content.md) you learned how to add a sprite using the MGCB editor and render it into MonoGame. However it does come with the flaw when you start building a large game, and that multiple sprites are required and loading them all in code, one line at a time inside our LoadContent function is too tedious and messy

Instead of loading our textures inside of Game1.cs, we can load these onto a separate file, and then just load the file in Game1.cs. That way it is much more organized and easier to read.

### Setup
Create a new class file inside your Components folder, just like how you setup the GameManager class in the last part. Your components folder should look like this:

![](~/images/first_2d_shooter/2_FolderSprite.png)


Click on the new SpriteArt class we just created in our Components folder. This should open up the .cs file for you to edit.

Again like with the GameManager class you created, this will also be a static class. You won't need multiple instances of SpriteArt and that you should be able to access the values contained in them.

Edit the SpriteArt.cs file and replace everything inside with this:
```csharp
//SpriteArt.cs
//The namespace should be the same as the one in your Game1.cs
namespace StarShooterDemo 
{
    //The class name should generally be the same as the file name
    static class SpriteArt 
    {
        //Place our texture references and methods here
    }
}
```
Make sure that the namespace is the same namespace as the one located in your Game1.cs file


Now lets load in our first texture, under the SpriteArt class lets take our PlayerShip and load it:
```csharp
//SpriteArt.cs
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

static class SpriteArt 
{
    //Create our texture references here. If we want to get a certain sprite
    //you can use SpriteArt.Player to retrieve its texture
    public static Texture2D Player { get; private set; }    
}
```
Here we create a static Texture2D called Player. A Texture2D is basically an image information that you can use in your game. 

Here you created a static which can be read by other files in your project, but can only be set from the SpriteArt class.

Now you have to load the textures from the content folder. Here you can create a method inside the SpriteArt class to load all the textures into the game:

```csharp
//SpriteArt.cs
...
    public static Texture2D Player;
    //Function to load our content
    public static void Load(ContentManager content)
    {
        Player = content.Load<Texture2D>("PlayerShip");
    }
```
You can load the rest of the assets you downloaded here. Make sure that you have added them through te MGCB Editor:

```csharp
//SpriteArt.cs
...
    //Create our texture references here. If we want to get a certain sprite
    //you can use SpriteArt.Player to retrieve its texture
    public static Texture2D Player;
    public static Texture2D Enemy1;
    public static Texture2D Enemy2;
    public static Texture2D Bullet;
    public static Texture2D background;
    
    //Function to load our content
    public static void Load(ContentManager content)
    {
        Player = content.Load<Texture2D>("PlayerShip");
        Enemy1 = content.Load<Texture2D>("EnemyTier1");
        Enemy2 = content.Load<Texture2D>("EnemyTier2");
        Bullet = content.Load<Texture2D>("Bullet");
        background = content.Load<Texture2D>("StarBackground");
    }
```

To actually load these sprites into your game so that its useable, lets go back to the main class file Game1.cs and under the function LoadContent() lets add the following:
```csharp
//Game1.cs
protected override void LoadContent()
{
    _spriteBatch = new SpriteBatch(GraphicsDevice);
    //Load the SpriteArt file, which will load all the Sprites inside it
    //using the method you created in that file
    SpriteArt.Load(Content);
}
```
This will load all the sprites that is inside the SpriteArt class into the game so you can now use them.

You can now draw any of the images that was loaded into Game1.cs. Using the Player ship image you can draw using information from SpriteArt. Locate the Draw method inside the Game1.cs and use the following code:

```csharp
//Game1.cs
...
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Draw items here
            _spriteBatch.Begin();
            _spriteBatch.Draw(SpriteArt.Player, new Vector2(0, 0), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
```

Now when you run the game, you should see the Player image on the top left screen.

![](~/images/first_2d_shooter/2_PlayerSprite.png)

Once you are finished, you can go over to the next section where you create the fundamental code for all the objects in the game: [Part 3: Creating an Entity class](~/articles/tutorials/3_creating_entity_class.md)

## References and community tutorials

[MGCB Editor](~/articles/tools/mgcb_editor.md): This will show you how to get the MGCB editor downloaded and set up if you do not have it already.

[RB Whitakers 2d tutorial](http://rbwhitaker.wikidot.com/monogame-2d-tutorials): This will help you understand what is going on with sprites and give you a stronger depth of all the types of sprites.



