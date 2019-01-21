This file will go over adding content to your game. For help on creating a project please look at [Creating a New Project](getting_started/1_creating_a_new_project.md)

First of all you are gonna need some content for your game. For this tutorial use the following image of a ball:

![Open Content](images/getting_started/ball.png)

Do **right-click > Save Image As** and save it somewhere with the name "ball.png".

Now open up your game project and look at the left. You should see a solution explorer window. Expand the **Content** folder and open up **Content.mgcb** file by double clicking on it.

![Open Content](images/getting_started/3_open_content.png)

You should now see a MonoGame Pipeline Tool window open up. In case it didn't get opened, you can right-click on **Content.mgcb**, select **open with** and then select **MonoGame Pipeline**.

![MonoGame Pipeline Tool](images/getting_started/3_pipeline_tool.png)

Your game content is managed from this external tool. You can add content to your game in one of the following ways:

- **Add Existing Item** toolbar button
- **Edit > Add > Existing Item...** menu button
- **right-click > Add > Existing Item...** context menu

In our case let's use the **Add Existing Item** toolbar button.

![Add Content](images/getting_started/3_add_content.png)

You should now be prompted to select a file. Select the "ball.png" that you have downloaded a moment ago. After that you will be asked on what action you want to do for adding the file. Just leave the it to default and click **OK**.

![Copy Content](images/getting_started/3_copy_content.png)

Now simply click **Save** toolbar button and close the tool.

![Save Content](images/getting_started/3_save_content.png)

Now that we have added the content, it's time to load it. First declare a new variable so we can load the ball image into memory.

```csharp
public class Game1 : Game
{
    Texture2D textureBall;

    GraphicsDeviceManager graphics;
```

Next find the Load Content method and use it to initialize the ball private variable:

```csharp
protected override void LoadContent()
{
    // Create a new SpriteBatch, which can be used to draw textures.
    spriteBatch = new SpriteBatch(GraphicsDevice);

    // TODO: use this.Content to load your game content here
    textureBall = Content.Load<Texture2D>("ball");
}
```

And finally, find the Draw method, and let's draw the ball onto the screen:

```csharp
protected override void Draw(GameTime gameTime)
{
    graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

    // TODO: Add your drawing code here
    spriteBatch.Begin();
    spriteBatch.Draw(textureBall, new Vector2(0, 0), Color.White);
    spriteBatch.End();

    base.Draw(gameTime);
}
```

Now run the game and you should get the following:

![Game](images/getting_started/3_game.png)

For the next part, look at [Adding Basic Code](getting_started/4_adding_basic_code.md) page.
