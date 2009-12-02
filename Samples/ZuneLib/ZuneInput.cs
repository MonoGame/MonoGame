using System.Collections.Generic;
using System.Collections.ObjectModel;
#if IPHONE
using XnaTouch.Framework;
using XnaTouch.Framework.Input;
#else
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endif

namespace ZuneLib
{
	/// <summary>
	/// A static helper class for handling input between Windows and Zune builds.
	/// </summary>
	public static class ZuneInput
	{
#if WINDOWS
		// used to give our ZuneTouch an ID. we increment this with each new mouse press
		private static int id;

		// used so we can have a proper ZuneTouchState from our mouse
		private static MouseState lastMouseState;
#endif

		// our list of current touches
		private static readonly List<ZuneTouch> touches = new List<ZuneTouch>();

		/// <summary>
		/// Gets a ReadOnlyCollection of the current ZuneTouchs.
		/// </summary>
		public static ReadOnlyCollection<ZuneTouch> Touches { get; private set; }

		/// <summary>
		/// Gets the current reading of the accelerometer's acceleration.
		/// </summary>
		public static Vector3 Acceleration { get; private set; }

		static ZuneInput()
		{
			Touches = new ReadOnlyCollection<ZuneTouch>(touches);
		}

		// internal because we want our game types to call this.
		internal static void Update()
		{
			// call our other update method, casting int.MaxValue to an orientation.
			// since the orientation is not Right or Left, it will default to portrait
			// as desired by this Update call.
			Update((LandscapeOrientation)int.MaxValue);
		}

		// internal because we want our game types to call this.
		internal static void Update(LandscapeOrientation landscapeOrientation)
		{
#if !WINDOWS
			// On Zune we can just use the TouchPanel class and fill
			// our ZuneTouch structures with the same data.

			// clear out the old touches
			touches.Clear();

			// for each touch on the touch panel
			foreach (var t in TouchPanel.GetState())
			{
				Vector2 position = t.Position;

				// adjust the touch's position based on device orientation
				switch (landscapeOrientation)
				{
					case LandscapeOrientation.Right:
						position = new Vector2(position.Y, 272 - position.X);
						break;
					case LandscapeOrientation.Left:
						position = new Vector2(480 - position.Y, position.X);
						break;
					default:
						break;
				}

				// add a new ZuneTouch instance that duplicates the data
				touches.Add(new ZuneTouch 
            	{
            		ID = t.Id,
            		Position = position,
            		State = (ZuneTouchState)(int)t.State, 
            		Pressure = t.Pressure 
            	});
			}

			// Then we just update our accelerometer property from the Accelerometer class
			AccelerometerState accState = Accelerometer.GetState();
			Acceleration = accState.Acceleration;			
#else
			// On PC we use the current and last mouse state's to position our 
			// fake touch as well as update its state.
			MouseState mouseState = Mouse.GetState();

			// we want to make sure we're in bounds of the screen, so figure out if we're
			// actually in landscape mode or not.
			int screenWidth = 272, screenHeight = 480;

			if (landscapeOrientation == LandscapeOrientation.Left || landscapeOrientation == LandscapeOrientation.Right)
			{
				screenWidth = 480;
				screenHeight = 272;
			}

			// first we check to see if we have a touch in the collection with a state of released,
			// and if so, we clear the list out
			if (touches.Count > 0 && touches[0].State == ZuneTouchState.Released)
				touches.Clear();

			// if this is a new press of the left mouse button and the mouse is inside the window...
			if (mouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released && 
				mouseState.X >= 0 && mouseState.X <= screenWidth && mouseState.Y >= 0 && mouseState.Y <= screenHeight)
			{
				// add a new ZuneTouch instance with our data
				touches.Add(new ZuneTouch
				{
					ID = id++,
					Position = new Vector2(mouseState.X, mouseState.Y),
					State = ZuneTouchState.Pressed,
					Pressure = 1
				});
			}

			// otherwise if the left button isn't newly pressed and we are tracking a touch already...
			else if (touches.Count > 0)
			{
				// get the touch
				var touch = touches[0];

				// update the position
				touch.Position = new Vector2(mouseState.X, mouseState.Y);

				// we assume the touch has moved
				touch.State = ZuneTouchState.Moved;

				// if the mouse was released inside the window OR moved outside of the window...
				if ((mouseState.LeftButton == ButtonState.Released && lastMouseState.LeftButton == ButtonState.Pressed) ||
					mouseState.X < 0 || mouseState.X > screenWidth || mouseState.Y < 0 || mouseState.Y > screenHeight)
				{
					// set the State to Released
					touch.State = ZuneTouchState.Released;
				}

				// set the touch back onto the list
				touches[0] = touch;
			}

			lastMouseState = mouseState;

            // to fake accelerometer data, we use a GamePad's thumbsticks and set the acceleration
			// using this system:
			// X Axis of Left Thumbstick = X axis
			// Y Axis of Left Thumbstick = Y axis
			// X Axis of Right Thumbstick = Z axis
			GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
			Acceleration = new Vector3(
				gamePadState.ThumbSticks.Left.X, 
				gamePadState.ThumbSticks.Left.Y, 
				gamePadState.ThumbSticks.Right.X);
#endif
		}
	}
}