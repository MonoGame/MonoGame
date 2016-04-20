MonoGame supports multiple methods of using different fonts.  The following is an explanation of how to use TrueType fonts.

#### Using TrueType Fonts with MonoGame
To use a TrueType font, MonoGame requires 2 files: the TrueType font file, and a .spritefont file.
TrueType fonts may be installed on the system in use, or added to the project manually in the same directory as the .spritefont file.

1. Create the .spritefont file.

<p align="center">
<img src="images/adding_ttf_fonts_step_1.PNG"/>
</p>

<p align="center">
<img src="images/adding_ttf_fonts_step_2.PNG"/>
</p>

2- Open the newly created .spritefont file in your text editor of choice.

3- Find the line below: 

```
<FontName>Arial</FontName>
```

4- Replace the text between the tags with your selected .ttf font.
If the font is installed on the system, just type the name of the font, without the file extension.


#### Usage Example
1- Make a class variable of type SpriteFont.

```
SpriteFont font;
```

2- Load the font in the LoadContent function of your game.

```
font = myGame.Content.Load<SpriteFont>("Fonts/myFont")
```

3- Draw any text in the Draw function of your game. The example below uses a Vector2 to find the center of the text to be displayed, and places it in the center of the window with an other Vector2.

```
spriteBatch.Begin();
// Finds the center of the string in coordinates inside the text rectangle
Vector2 textMiddlePoint = font.MeasureString(text) / 2;
// Places text in center of the window
Vector2 position = new Vector2(myGame.Window.ClientBounds.Width / 2, myGame.Window.ClientBounds.Height / 2);
spriteBatch.DrawString(font, "MonoGame Font Test", position, Color.White, 0, textMiddlePoint, 1.0f, SpriteEffects.None, 0.5f)
spriteBatch.End();
```

If you want to read more on fonts, please refer to the [API Documentation]()
