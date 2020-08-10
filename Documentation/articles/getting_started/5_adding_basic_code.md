# Adding Basic Code

This tutorial will go over adding basic logic to your game. This tutorial continues where [Adding Content](4_adding_content.md) left off.

---

## Positioning the content

First, we need to add few new variables in the Game1.cs class file, one for position and one for speed.

```csharp
public class Game1 : Game
{
    Texture2D ballTexture;
    Vector2 ballPosition;
    float ballSpeed;
```

Next let us initialize them. Find the **Initialize** method and add the following lines.

```csharp
// TODO: Add your initialization logic here
ballPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2,
_graphics.PreferredBackBufferHeight / 2);
ballSpeed = 100f;

base.Initialize();
```

With this, we are setting the ball starting position to the center of the screen based off the dimensions of the screen determined by the current **BackBufferWidth** and **BackBufferHeight**.

The last thing we need to do is modify the position the ball is getting drawn to.

Find the **Draw** method and update the **spriteBatch.Draw** call to:

```csharp
_spriteBatch.Draw(ballTexture, ballPosition, Color.White);
```

Now run the game.

![Draw Ball 1](~/images/getting_started/4_ball_not_center.png)

As you can see the ball is not quite centered yet. This is because MonoGame uses (0, 0) as the origin point of the image for drawing by default (Top Left hand corner). We can modify this by taking into account the height and width of the image when drawing, as follows:

```csharp
_spriteBatch.Draw(
    ballTexture,
    ballPosition,
    null,
    Color.White,
    0f,
    new Vector2(ballTexture.Width / 2, ballTexture.Height / 2),
    Vector2.One,
    SpriteEffects.None,
    0f
);
```

We have added a few extra parameters to the spriteBatch.Draw call, but do not worry about that for now, with this update we are setting the actual center (width / 2 and height / 2) of the image as it's origin (drawing point). 

Now the image will get drawn to the center of the screen.

![Draw Ball 2](~/images/getting_started/4_ball_center.png)

---

## Getting user input

Next let's setup some movement. Find the **Update** method in the Game1.cs class file and add:

```csharp
// TODO: Add your update logic here
var kstate = Keyboard.GetState();

if (kstate.IsKeyDown(Keys.Up))
    ballPosition.Y -= ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

if(kstate.IsKeyDown(Keys.Down))
    ballPosition.Y += ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

if (kstate.IsKeyDown(Keys.Left))
    ballPosition.X -= ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

if(kstate.IsKeyDown(Keys.Right))
    ballPosition.X += ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

base.Update(gameTime);
```

Let's discuss the code a bit.

With this we are getting the current keyboard state ('Keyboard.GetState()') and storing it into a variable called **kstate**.

```csharp
var kstate = Keyboard.GetState();
```

Next is just a simple check to see if the Up arrow key is pressed.

```csharp
if (kstate.IsKeyDown(Keys.Up))
```

If the Up Arrow key is pressed, we move the ball by the value we set **ballSpeed**. The reason why we then multiply the **ballSpeed** by **gameTime.ElapsedGameTime.TotalSeconds** is because Update is not a fixed time and the time between update calls is not always the same, so in order to get smooth movement on the screen we multiply speed by the time since the last update method was called.

```csharp
    ballPosition.Y -= ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
```

> Try it yourself by simply moving by the **ballSpeed** alone to see the difference and compare.

The sections repeat the above for the Down, Left and Right arrow keys.

If you now run the game and you should be able to move the ball with the arrow keys.

You will probably notice that you can get out of the window, so let's make it so that the ball can not escape the window. We will do this by setting bounds onto the ballPosition after it has already been moved to ensure it cannot go further than the width or height of the screen.

```csharp
if(kstate.IsKeyDown(Keys.Right))
    ballPosition.X += ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

if(ballPosition.X > graphics.PreferredBackBufferWidth - ballTexture.Width / 2)
    ballPosition.X = graphics.PreferredBackBufferWidth - ballTexture.Width / 2;
else if(ballPosition.X < ballTexture.Width / 2)
    ballPosition.X = ballTexture.Width / 2;

if(ballPosition.Y > graphics.PreferredBackBufferHeight - ballTexture.Height / 2)
    ballPosition.Y = graphics.PreferredBackBufferHeight - ballTexture.Height / 2;
else if(ballPosition.Y < ballTexture.Height / 2)
    ballPosition.Y = ballTexture.Height / 2;

base.Update(gameTime);
```

Now run the game and the ball will not be able to go beyond the window bounds anymore.

Happy Coding ^^

## Further Reading

Check out the [Tutorials section](~/articles/tutorials.md) for many more helpful guides and tutorials on building games with MonoGame.  We have an expansive library of helpful content all provided by other MonoGame developers in the community.

Additionally, be sure to check out the official [MonoGame Samples](~/articles/samples.md) page for fully built sample projects built with MonoGame and targeting our most common platforms.
