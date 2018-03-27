
This file will go over the code that is getting created when you start a blank project. For help on creating a project please look at [Creating a New Project](getting_started/1_creating_a_new_project.md)

**Using Statements**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
```

These using statements are required in order to use the code that MonoGame has to offer.

The reason why they start with Microsoft.Xna.Framework is because MonoGame is an open source implementation of Microsoft Xna framework, and in order to maintain compatibility with the XNA code, it is using the same namespaces.

**The Game1 Class**

```csharp
public class Game1 : Game
```

The main Game1 class is inheriting from the Game class, which provides all the core methods for your game (ie. Load/Unload Content, Update, Draw etc.). You usually have only one Game class per game so its name isn't that important.

**Instanced Variables**

```csharp
GraphicsDeviceManager graphics;
SpriteBatch spriteBatch;
```

The two default variables that the blank template starts with are GraphicsDeviceManager and SpriteBatch. Both of these variables are used for drawing stuff as you will see in a later tutorial.

**Constructor**

```csharp
public Game1()
{
    graphics = new GraphicsDeviceManager(this);
    Content.RootDirectory = "Content";
}
```
The main game constructor is used to initialize the starting variables. In this case we are creating a new GraphicsDeviceManager from our game, and are setting the folder which the game will search for content.

**Initialize Method**

```csharp
protected override void Initialize()
{
    // TODO: Add your initialization logic here

    base.Initialize();
}
```

This method is called after the constructor, but before the main game loop(Update/Draw). This is where you can query any required services and load any non-graphic related content.

**LoadContent Method**

```csharp
protected override void LoadContent()
{
    // Create a new SpriteBatch, which can be used to draw textures.
    spriteBatch = new SpriteBatch(GraphicsDevice);

    // TODO: use this.Content to load your game content here
}
```

This method is used to load your game content. It is called only once per game, after Initialize method, but before the main game loop methods.

**Update Method**

```csharp
protected override void Update(GameTime gameTime)
{
    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        Exit();

    // TODO: Add your update logic here

    base.Update(gameTime);
}
```

This method is called multiple times per second, and is used to update your game state (checking for collisions, gathering input, playing audio, etc.).

**Draw Method**

```csharp
protected override void Draw(GameTime gameTime)
{
    graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

    // TODO: Add your drawing code here

    base.Draw(gameTime);
}
```

Similar to the Update method, it is also called multiple times per second.

For the next part, look at [Adding Content](getting_started/3_adding_content.md) page.
