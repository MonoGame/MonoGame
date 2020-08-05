# TrueType fonts

MonoGame supports more than one method of using fonts, the following is an explanation of how to use TrueType fonts.

## Using TrueType Fonts with MonoGame

To be able to use a TrueType font, MonoGame requires the TrueType font file and a .spritefont file.
TrueType fonts may be installed on the system, or added manually in the same directory as the .spritefont file.

1. Create the .spritefont file by selecting "Edit -> Add -> New Item" from the MGCB Editor menu, then select **SpriteFont Description** from the list and click **Create**.

  ![Adding TTF fonts step 2](~/images/content/adding_ttf_fonts.PNG)

1. Open the newly created .spritefont file in your text editor of choice, find this line and change it to your selected .ttf font.
If the font is installed on the system, just type the name of the font.

```xml
<FontName>Arial</FontName>
```

## Usage Example

Make a class variable of type [`Spritefont`](xref:Microsoft.Xna.Framework.Graphics.SpriteFont)

```csharp
SpriteFont font;
```

Load the font with [`ContentManager.Load`](xref:Microsoft.Xna.Framework.Content.ContentManager)

```csharp
font = myGame.Content.Load<SpriteFont>("Fonts/myFont")
```

Draw text with [`SpriteBatch.Draw`](xref:Microsoft.Xna.Framework.Graphics.SpriteBatch)

```csharp
spriteBatch.Begin();
// Finds the center of the string in coordinates inside the text rectangle
Vector2 textMiddlePoint = font.MeasureString(text) / 2;
// Places text in center of the screen
Vector2 position = new Vector2(myGame.Window.ClientBounds.Width / 2, myGame.Window.ClientBounds.Height / 2);
spriteBatch.DrawString(font, "MonoGame Font Test", position, Color.White, 0, textMiddlePoint, 1.0f, SpriteEffects.None, 0.5f)
spriteBatch.End();
```
