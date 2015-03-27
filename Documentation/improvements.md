The new API features which are available in the current development release and does not exist in XNA.

##Graphics and window routines

[GameWindow.AllowAltF4](http://www.monogame.net/documentation/?page=P_Microsoft_Xna_Framework_GameWindow_AllowAltF4) - boolean that allows or disallow using Alt+F4 combo for game window closing. You can change it anytime in your code. Note : this isnt working on WinRT projects because Alt+F4 is intended to work always on this platform.

[GameWindow.IsBorderless](http://www.monogame.net/documentation/?page=P_Microsoft_Xna_Framework_GameWindow_IsBorderless) - boolean that makes border of your window invisible. You cannot move or resize your game window after this boolean setted to true. Works only on desktop platforms.

[GraphicsDeviceManager.HardwareModeSwitch](http://www.monogame.net/documentation/?page=P_Microsoft_Xna_Framework_GraphicsDeviceManager_HardwareModeSwitch) - boolean that enables or disables "hard"(device switching) fullscreen switching for WindowsDX platform. By default it is true but you can change it in the constructor.

##Math

[MathHelper.Clamp(int,int,int)](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_MathHelper_Clamp_1) - this overload clamps integers instead floats which could be useful in some cases. You dont need to convert like this MathHelper.Clamp((float)integer,(float)min,(float)max) anymore.

[MathHelper.Min(int,int,int)](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_MathHelper_Min) - 
same as [MathHelper.Min](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_MathHelper_Min_1) but for integers.

[MathHelper.Max(int,int,int)](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_MathHelper_Max_1) - 
same as [MathHelper.Max](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_MathHelper_Max) but for integers.

[Point.ToVector2()](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_Point_ToVector2) - this is easy to use conversion from [Point](http://www.monogame.net/documentation/?page=T_Microsoft_Xna_Framework_Point) to [Vector2](http://www.monogame.net/documentation/?page=T_Microsoft_Xna_Framework_Vector2). Use it like this 
```
Point point = new Point(10,10);
var vector2 = point.ToVector2();
```
[Rectangle.Contains(float,float)](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_Rectangle_Contains_5) - float-based version of [Rectangle.Contains(int,int)](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_Rectangle_Contains_6)

[Rectangle.Contains(Vector2)](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_Rectangle_Contains_2) - float-based version of [Rectangle.Contains(Point)](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_Rectangle_Contains_7)

[Rectangle.Contains(ref Vector2,out bool)](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_Rectangle_Contains_1) - float-based version of [Rectangle.Contains(ref Point,out bool)](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_Rectangle_Contains_3)

[Rectangle.Offset(float,float)](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_Rectangle_Offset_1) - float-based version of [Rectangle.Offset(int,int)](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_Rectangle_Offset)

[Rectangle.Offset(Vector2)](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_Rectangle_Offset_3) - float-based version of [Rectangle.Offset(Point)](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_Rectangle_Offset_2)

[Rectangle.Size](http://www.monogame.net/documentation/?page=P_Microsoft_Xna_Framework_Rectangle_Size) - this property is a [Point](http://www.monogame.net/documentation/?page=T_Microsoft_Xna_Framework_Point) which contains width and height of this rectangle.

[Vector2.ToPoint()](http://www.monogame.net/documentation/?page=M_Microsoft_Xna_Framework_Vector2_ToPoint) - this is easy to use conversion from [Vector2](http://www.monogame.net/documentation/?page=T_Microsoft_Xna_Framework_Vector2) to [Point](http://www.monogame.net/documentation/?page=T_Microsoft_Xna_Framework_Point). Use it like this 

```
Vector2 vector2 = new Vector2(10.01f,10.99f);
var point = vector2.ToPoint(); // point will be 10,10.
```

##Input

[MouseState.Position](http://www.monogame.net/documentation/?page=P_Microsoft_Xna_Framework_Input_MouseState_Position) - property which returns a [Point](http://www.monogame.net/documentation/?page=T_Microsoft_Xna_Framework_Point) which contains X and Y parameters of MouseState. Before this change it is harder to obtain mouse position in a good way. You can now write something like this
```
spriteBatch.Draw(texture,mouseState.Position.ToVector2(),Color.White);
```
and it works fine - your sprite will be drawed under cursor.

[KeyboardInput](http://www.monogame.net/documentation/?page=T_Microsoft_Xna_Framework_Input_KeyboardInput) static class which could be used to show virtual keyboard on the current platform.

[MessageBox](http://www.monogame.net/documentation/?page=T_Microsoft_Xna_Framework_Input_MessageBox) static class which could be used to show message dialog on the current platform.
