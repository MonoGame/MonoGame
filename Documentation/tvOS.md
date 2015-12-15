#

## Menu Button Handling

The Menu button will map to the "Back" button on the GamePad. However on tvOS,
the Menu button requires some special processing. According to the Apple 
documentation the Menu Button

"*Pauses/resumes gameplay.
Returns to previous screen, exits to main game menu, and/or exits to Apple TV Home screen.*"

By default MonoGame will exit to the Apple TV Home screen when the Menu button is pressed, 
this is not alawys the desired behviour. When in gameplay the Menu button really should
Pause the game rather than Exiting to the Home screen.

Because MonoGame has no idea of the game state, it is down to the developer to inform
it when it can exit to the Home screen and when it should ignore the Menu event and allow
the developer to the event.

Some sample code.

```csharp

	public class Game1 : Game , IPlaformBackButton {

		bool IsOnRootMenu = true;

		public bool Handled () {
			return IsOnRootMenu ? false : true;
		}

		public Game1 ()
		{
			Services.AddService<IPlaformBackButton>(this);
		}

		public override Update(GameTime gametime)
		{
			if (GamePad.GetState (PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
			{
				// do something in game
			}
		}
	}
```

The key to this working is the `IPlatformBackButton` interface. By implementing
and registering this interface MonoGame can callback into your application to ask if it
should let you handle the Menu button or if it should pass it up to tvOS. So in this case if
the app is on the "Main menu" the developer will set *IsOnRootMenu* to true and when the Menu
button is pressed the game with Exit. However if IsOnRootMenu is false then the "Menu" button 
click will be routed to the GamePad Back button and the developer can check for the Back button
press and act accordingly.
