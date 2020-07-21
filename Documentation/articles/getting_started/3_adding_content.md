# Adding Content

This tutorial will go over adding content to your game.

> For help on creating a project please look at the [Creating a New Project](getting_started.md) of the Getting Started guide.

---

## The Content Pipeline tool

First, you are going to need some content for your game. For this tutorial use the following image of a ball:

![Open Content](~/images/getting_started/ball.png)

Do **right-click > Save Image As** and save it somewhere locally with the name “ball.png”.

Now open up your game project and look at the Solution explorer window. Expand the **Content** folder and open up **Content.mgcb** file by double-clicking on it.

![Open Content](~/images/getting_started/3_open_content.png)

You should now see the Pipeline Editor window open up. If it does not open up (you see a text file open), then you can right-click on **Content.mgcb** and select **Open With**, then select **MonoGame Pipeline** in the list, click **Set as Default** and then click **OK**.

> If you do not see the **MonoGame Pipeline Tool** option when you right-click and select **Open With**, then please review the [Tools documentation](~/articles/tools/tools.md) for installing the Pipeline tool for your operating system.

![Pipeline Editor](~/images/getting_started/3_pipeline_tool.png)

Your game content is managed from this external tool. You can add content to your game in one of the following ways:

- **Add Existing Item** toolbar button
- **Edit > Add > Existing Item...** menu button
- **right-click > Add > Existing Item...** context menu

In this case let us use the **Add Existing Item** toolbar button.

![Add Content](~/images/getting_started/3_add_content.png)

You should now be prompted to select a file. Select the “ball.png” image that you downloaded a moment ago, once selected you will be asked "what action you want to do when adding the file?", just leave it as the default and click **OK**.

![Copy Content](~/images/getting_started/3_copy_content.png)

Now click **Save** toolbar button and close the Pipeline tool.

![Save Content](~/images/getting_started/3_save_content.png)

---

## Adding the content in your game

Now that we have added the assets to the Content project, it is time to load it in your game. First open up the **Game1.cs** class file and declare a new **ballTexture** variable of type **Texture2D** in the **Game1** class, so we can store the ball image into memory.

```csharp
public class Game1 : Game
{
    Texture2D ballTexture;

    GraphicsDeviceManager graphics;
```

Next find the LoadContent method and use it to retrieve the "ball" sprite from the Content project into the **ballTexture** private variable using the **Content.Load()** method and specifying the type of data we are requesting, in this case a Texture2D image:

```csharp
protected override void LoadContent()
{
    // Create a new SpriteBatch, which can be used to draw textures.
    spriteBatch = new SpriteBatch(GraphicsDevice);

    // TODO: use this.Content to load your game content here
    ballTexture = Content.Load<Texture2D>("ball");
}
```

Finally, find the Draw method, and let us draw the ball onto the screen. This is done by:

- Opening a SpriteBatch (an image drawing collection function).

- Adding the images we want to draw and where we want them drawn to.

- Then finally closing the SpriteBatch to commit the textures we want drawn to the screen.

> **Note**, if you add multiple images, they will be drawn in the order you place them from back to front (each drawn on top of each other).

As shown below:

```csharp
protected override void Draw(GameTime gameTime)
{
    graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

    // TODO: Add your drawing code here
    spriteBatch.Begin();
    spriteBatch.Draw(ballTexture, new Vector2(0, 0), Color.White);
    spriteBatch.End();

    base.Draw(gameTime);
}
```

Now run the game and you should get the following:

![Game](~/images/getting_started/3_game.png)

For the next part, look at [Adding Basic Code](4_adding_basic_code.md) page.
