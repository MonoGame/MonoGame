#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;

#if ANDROID
using Android.App;
#endif

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

#endregion

namespace ChaseAndEvade
{
	/// <summary>
	/// Sample showing how to implement simple chase, evade, and wander AI behaviors.
	/// The behaviors are based on the TurnToFace function, which was explained in
	/// AI Sample 1: Aiming.
	/// </summary>
	public class ChaseAndEvadeGame : Game
	{

		/// <summary>
		/// TankAiState is used to keep track of what the tank is currently doing.
		/// </summary>
		enum TankAiState
		{
			// chasing the cat
			Chasing,
			// the tank has gotten close enough that the cat that it can stop chasing it
			Caught,
			// the tank can't "see" the cat, and is wandering around.
			Wander
		}

		/// <summary>
		/// MouseAiState is used to keep track of what the mouse is currently doing.
		/// </summary>
		enum MouseAiState
		{
			// evading the cat
			Evading,
			// the mouse can't see the "cat", and it's wandering around.
			Wander
		}



	#region Constants

		// The following values control the different characteristics of the characters
		// in this sample, including their speed, turning rates. distances are specified
		// in pixels, angles are specified in radians.

		// how fast can the cat move?
		const float MaxCatSpeed = 7.5f;

		// how fast can the tank move?
		const float MaxTankSpeed = 5.0f;

		// how fast can he turn?
		const float TankTurnSpeed = 0.10f;

		// this value controls the distance at which the tank will start to chase the
		// cat.
		const float TankChaseDistance = 250.0f;

		// TankCaughtDistance controls the distance at which the tank will stop because
		// he has "caught" the cat.
		const float TankCaughtDistance = 60.0f;

		// this constant is used to avoid hysteresis, which is common in ai programming.
		// see the doc for more details.
		const float TankHysteresis = 15.0f;

		// how fast can the mouse move?
		const float MaxMouseSpeed = 8.5f;

		// and how fast can it turn?
		const float MouseTurnSpeed = 0.20f;

		// MouseEvadeDistance controls the distance at which the mouse will flee from
		// cat. If the mouse is further than "MouseEvadeDistance" pixels away, he will
		// consider himself safe.
		const float MouseEvadeDistance = 200.0f;

		// this constant is similar to TankHysteresis. The value is larger than the
		// tank's hysteresis value because the mouse is faster than the tank: with a
		// higher velocity, small fluctuations are much more visible.
		const float MouseHysteresis = 60.0f;
	#endregion

	#region Fields

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		SpriteFont spriteFont;
		Texture2D tankTexture;
		Vector2 tankTextureCenter;
		Vector2 tankPosition;
		TankAiState tankState = TankAiState.Wander;
		float tankOrientation;
		Vector2 tankWanderDirection;
		Texture2D catTexture;
		Vector2 catTextureCenter;
		Vector2 catPosition;
		Texture2D mouseTexture;
		Vector2 mouseTextureCenter;
		Vector2 mousePosition;
		MouseAiState mouseState = MouseAiState.Wander;
		float mouseOrientation;
		Vector2 mouseWanderDirection;
		Random random = new Random ();

	#endregion

	#region Initialization

#if ANDROID 
		public ChaseAndEvadeGame (Activity activity) : base (activity)
#else 
        public ChaseAndEvadeGame ()  
#endif
        {

			graphics = new GraphicsDeviceManager (this);
			
			Content.RootDirectory = "Content";

#if WINDOWS_PHONE
			graphics.SupportedOrientations = DisplayOrientation.Portrait;
			graphics.PreferredBackBufferWidth = 480;
			graphics.PreferredBackBufferHeight = 800;
	
			TargetElapsedTime = TimeSpan.FromTicks(333333);
#elif !MONOMAC
			
			graphics.PreferredBackBufferWidth = 320;
			graphics.PreferredBackBufferHeight = 480;
#endif
			graphics.IsFullScreen = true;
		}

		/// <summary>
		/// Overridden from the base Game.Initialize. Once the GraphicsDevice is setup,
		/// we'll use the viewport to initialize some values.
		/// </summary>
		protected override void Initialize ()
		{
			base.Initialize ();

			// once base.Initialize has finished, the GraphicsDevice will have been
			// created, and we'll know how big the Viewport is. We want the tank, cat
			// and mouse to be spread out across the screen, so we'll use the viewport
			// to figure out where they should be.
			Viewport vp = graphics.GraphicsDevice.Viewport;

			tankPosition = new Vector2 (vp.Width / 4, vp.Height / 2);
			catPosition = new Vector2 (vp.Width / 2, vp.Height / 2);
			mousePosition = new Vector2 (3 * vp.Width / 4, vp.Height / 2);

		}


		/// <summary>
		/// Load your graphics content.
		/// </summary>
		protected override void LoadContent ()
		{
			// create a SpriteBatch, and load the textures and font that we'll need
			// during the game.
			spriteBatch = new SpriteBatch (graphics.GraphicsDevice);
			spriteFont = Content.Load<SpriteFont> ("Arial");
			tankTexture = Content.Load<Texture2D> ("tank");
			catTexture = Content.Load<Texture2D> ("cat");
			mouseTexture = Content.Load<Texture2D> ("mouse");

			// once all the content is loaded, we can calculate the centers of each
			// of the textures that we loaded. Just like in the previous sample in
			// this series, the aiming sample, we want spriteBatch to draw the
			// textures centered on their position vectors. SpriteBatch.Draw will
			// center the sprite on the vector that we pass in as the "origin"
			// parameter, so we'll just calculate that to be the middle of
			// the texture.
			tankTextureCenter = 
				new Vector2 (tankTexture.Width / 2, tankTexture.Height / 2);
			catTextureCenter = 
				new Vector2 (catTexture.Width / 2, catTexture.Height / 2);
			mouseTextureCenter = 
				new Vector2 (mouseTexture.Width / 2, mouseTexture.Height / 2);
		}

	#endregion

	#region Update and Draw

		/// <summary>
		/// Allows the game to run logic.
		/// </summary>
		protected override void Update (GameTime gameTime)
		{
			// handle input will read the controller input, and update the cat
			// to move according to the user's whim.
			HandleInput ();

			// UpdateTank will run the AI code that controls the tank's movement...
			UpdateTank ();

			// ... and UpdateMouse does the same thing for the mouse.
			UpdateMouse ();

			// Once we've finished that, we'll use the ClampToViewport helper function
			// to clamp everyone's position so that they stay on the screen.
			tankPosition = ClampToViewport (tankPosition);
			catPosition = ClampToViewport (catPosition);
			mousePosition = ClampToViewport (mousePosition);

			base.Update (gameTime);
		}

		/// <summary>
		/// This function takes a Vector2 as input, and returns that vector "clamped"
		/// to the current graphics viewport. We use this function to make sure that 
		/// no one can go off of the screen.
		/// </summary>
		/// <param name="vector">an input vector</param>
		/// <returns>the input vector, clamped between the minimum and maximum of the
		/// viewport.</returns>
		private Vector2 ClampToViewport (Vector2 vector)
		{
			Viewport vp = graphics.GraphicsDevice.Viewport;
			vector.X = MathHelper.Clamp (vector.X, vp.X, vp.X + vp.Width);
			vector.Y = MathHelper.Clamp (vector.Y, vp.Y, vp.Y + vp.Height);
			return vector;
		}

		/// <summary>
		/// This function contains the code that controls the mouse. It decides what the
		/// mouse should do based on the position of the cat: if the cat is too close,
		/// it will attempt to flee. Otherwise, it will idly wander around the screen.
		/// 
		/// </summary>
		private void UpdateMouse ()
		{
			// first, calculate how far away the mouse is from the cat, and use that
			// information to decide how to behave. If they are too close, the mouse
			// will switch to "active" mode - fleeing. if they are far apart, the mouse
			// will switch to "idle" mode, where it roams around the screen.
			// we use a hysteresis constant in the decision making process, as described
			// in the accompanying doc file.
			float distanceFromCat = Vector2.Distance (mousePosition, catPosition);

			// the cat is a safe distance away, so the mouse should idle:
			if (distanceFromCat > MouseEvadeDistance + MouseHysteresis) {
				mouseState = MouseAiState.Wander;
			}
			// the cat is too close; the mouse should run: 
			else if (distanceFromCat < MouseEvadeDistance - MouseHysteresis) {
				mouseState = MouseAiState.Evading;
			}
			// if neither of those if blocks hit, we are in the "hysteresis" range,
			// and the mouse will continue doing whatever it is doing now.

			// the mouse will move at a different speed depending on what state it
			// is in. when idle it won't move at full speed, but when actively evading
			// it will move as fast as it can. this variable is used to track which
			// speed the mouse should be moving.
			float currentMouseSpeed;

			// the second step of the Update is to change the mouse's orientation based
			// on its current state.
			if (mouseState == MouseAiState.Evading) {
				// If the mouse is "active," it is trying to evade the cat. The evasion
				// behavior is accomplished by using the TurnToFace function to turn
				// towards a point on a straight line facing away from the cat. In other
				// words, if the cat is point A, and the mouse is point B, the "seek
				// point" is C.
				//     C
				//   B
				// A
				Vector2 seekPosition = 2 * mousePosition - catPosition;

				// Use the TurnToFace function, which we introduced in the AI Series 1:
				// Aiming sample, to turn the mouse towards the seekPosition. Now when
				// the mouse moves forward, it'll be trying to move in a straight line
				// away from the cat.
				mouseOrientation = TurnToFace (mousePosition, seekPosition, 
					mouseOrientation, MouseTurnSpeed);

				// set currentMouseSpeed to MaxMouseSpeed - the mouse should run as fast
				// as it can.
				currentMouseSpeed = MaxMouseSpeed;
			} else {
				// if the mouse isn't trying to evade the cat, it should just meander
				// around the screen. we'll use the Wander function, which the mouse and
				// tank share, to accomplish this. mouseWanderDirection and
				// mouseOrientation are passed by ref so that the wander function can
				// modify them. for more information on ref parameters, see
				// http://msdn2.microsoft.com/en-us/library/14akc2c7(VS.80).aspx
				Wander (mousePosition, ref mouseWanderDirection, ref mouseOrientation, 
					MouseTurnSpeed);

				// if the mouse is wandering, it should only move at 25% of its maximum
				// speed. 
				currentMouseSpeed = .25f * MaxMouseSpeed;
			}

			// The final step is to move the mouse forward based on its current
			// orientation. First, we construct a "heading" vector from the orientation
			// angle. To do this, we'll use Cosine and Sine to tell us the x and y
			// components of the heading vector. See the accompanying doc for more
			// information.
			Vector2 heading = new Vector2 (
				(float)Math.Cos (mouseOrientation), (float)Math.Sin (mouseOrientation));

			// by multiplying the heading and speed, we can get a velocity vector. the
			// velocity vector is then added to the mouse's current position, moving him
			// forward.
			mousePosition += heading * currentMouseSpeed;
		}

		/// <summary>
		/// UpdateTank runs the AI code that will update the tank's orientation and
		/// position. It is very similar to UpdateMouse, but is slightly more
		/// complicated: where mouse only has two states, idle and active, the Tank has
		/// three.
		/// </summary>
		private void UpdateTank ()
		{
			// However, the tank's behavior is more complicated than the mouse's, and so
			// the decision making process is a little different. 

			// First we have to use the current state to decide what the thresholds are
			// for changing state, as described in the doc.

			float tankChaseThreshold = TankChaseDistance;
			float tankCaughtThreshold = TankCaughtDistance;
			// if the tank is idle, he prefers to stay idle. we do this by making the
			// chase distance smaller, so the tank will be less likely to begin chasing
			// the cat.
			if (tankState == TankAiState.Wander) {
				tankChaseThreshold -= TankHysteresis / 2;
			}
			// similarly, if the tank is active, he prefers to stay active. we
			// accomplish this by increasing the range of values that will cause the
			// tank to go into the active state. 
			else if (tankState == TankAiState.Chasing) {
				tankChaseThreshold += TankHysteresis / 2;
				tankCaughtThreshold -= TankHysteresis / 2;
			}
			// the same logic is applied to the finished state. 
			else if (tankState == TankAiState.Caught) {
				tankCaughtThreshold += TankHysteresis / 2;
			}

			// Second, now that we know what the thresholds are, we compare the tank's 
			// distance from the cat against the thresholds to decide what the tank's
			// current state is.
			float distanceFromCat = Vector2.Distance (tankPosition, catPosition);
			if (distanceFromCat > tankChaseThreshold) {
				// just like the mouse, if the tank is far away from the cat, it should
				// idle.
				tankState = TankAiState.Wander;
			} else if (distanceFromCat > tankCaughtThreshold) {
				tankState = TankAiState.Chasing;
			} else {
				tankState = TankAiState.Caught;
			}

			// Third, once we know what state we're in, act on that state.
			float currentTankSpeed;
			if (tankState == TankAiState.Chasing) {
				// the tank wants to chase the cat, so it will just use the TurnToFace
				// function to turn towards the cat's position. Then, when the tank
				// moves forward, he will chase the cat.
				tankOrientation = TurnToFace (tankPosition, catPosition, tankOrientation, 
					TankTurnSpeed);
				currentTankSpeed = MaxTankSpeed;
			} else if (tankState == TankAiState.Wander) {
				// wander works just like the mouse's.
				Wander (tankPosition, ref tankWanderDirection, ref tankOrientation, 
					TankTurnSpeed);
				currentTankSpeed = .25f * MaxTankSpeed;
			} else {
				// this part is different from the mouse. if the tank catches the cat, 
				// it should stop. otherwise it will run right by, then spin around and
				// try to catch it all over again. The end result is that it will kind
				// of "run laps" around the cat, which looks funny, but is not what
				// we're after.
				currentTankSpeed = 0.0f;
			}

			// this calculation is also just like the mouse's: we construct a heading
			// vector based on the tank's orientation, and then make the tank move along
			// that heading.
			Vector2 heading = new Vector2 (
				(float)Math.Cos (tankOrientation), (float)Math.Sin (tankOrientation));
			tankPosition += heading * currentTankSpeed;
		}

		/// <summary>
		/// Wander contains functionality that is shared between both the mouse and the
		/// tank, and does just what its name implies: makes them wander around the
		/// screen. The specifics of the function are described in more detail in the
		/// accompanying doc.
		/// </summary>
		/// <param name="position">the position of the character that is wandering
		/// </param>
		/// <param name="wanderDirection">the direction that the character is currently
		/// wandering. this parameter is passed by reference because it is an input and
		/// output parameter: Wander accepts it as input, and will update it as well.
		/// </param>
		/// <param name="orientation">the character's orientation. this parameter is
		/// also passed by reference and is an input/output parameter.</param>
		/// <param name="turnSpeed">the character's maximum turning speed.</param>
		private void Wander (Vector2 position, ref Vector2 wanderDirection, 
		ref float orientation, float turnSpeed)
		{
			// The wander effect is accomplished by having the character aim in a random
			// direction. Every frame, this random direction is slightly modified.
			// Finally, to keep the characters on the center of the screen, we have them
			// turn to face the screen center. The further they are from the screen
			// center, the more they will aim back towards it.

			// the first step of the wander behavior is to use the random number
			// generator to offset the current wanderDirection by some random amount.
			// .25 is a bit of a magic number, but it controls how erratic the wander
			// behavior is. Larger numbers will make the characters "wobble" more,
			// smaller numbers will make them more stable. we want just enough
			// wobbliness to be interesting without looking odd.
			wanderDirection.X += 
				MathHelper.Lerp (-.25f, .25f, (float)random.NextDouble ());
			wanderDirection.Y += 
				MathHelper.Lerp (-.25f, .25f, (float)random.NextDouble ());

			// we'll renormalize the wander direction, ...
			if (wanderDirection != Vector2.Zero) {
				wanderDirection.Normalize ();
			}
			// ... and then turn to face in the wander direction. We don't turn at the
			// maximum turning speed, but at 15% of it. Again, this is a bit of a magic
			// number: it works well for this sample, but feel free to tweak it.
			orientation = TurnToFace (position, position + wanderDirection, orientation, 
						.15f * turnSpeed);


			// next, we'll turn the characters back towards the center of the screen, to
			// prevent them from getting stuck on the edges of the screen.
			Vector2 screenCenter = Vector2.Zero;
			screenCenter.X = graphics.GraphicsDevice.Viewport.Width / 2;
			screenCenter.Y = graphics.GraphicsDevice.Viewport.Height / 2;

			// Here we are creating a curve that we can apply to the turnSpeed. This
			// curve will make it so that if we are close to the center of the screen,
			// we won't turn very much. However, the further we are from the screen
			// center, the more we turn. At most, we will turn at 30% of our maximum
			// turn speed. This too is a "magic number" which works well for the sample.
			// Feel free to play around with this one as well: smaller values will make
			// the characters explore further away from the center, but they may get
			// stuck on the walls. Larger numbers will hold the characters to center of
			// the screen. If the number is too large, the characters may end up
			// "orbiting" the center.
			float distanceFromScreenCenter = Vector2.Distance (screenCenter, position);
			float MaxDistanceFromScreenCenter = 
		Math.Min (screenCenter.Y, screenCenter.X);

			float normalizedDistance = 
		distanceFromScreenCenter / MaxDistanceFromScreenCenter;

			float turnToCenterSpeed = .3f * normalizedDistance * normalizedDistance * 
		turnSpeed;

			// once we've calculated how much we want to turn towards the center, we can
			// use the TurnToFace function to actually do the work.
			orientation = TurnToFace (position, screenCenter, orientation, 
		turnToCenterSpeed);
		}


		/// <summary>
		/// Calculates the angle that an object should face, given its position, its
		/// target's position, its current angle, and its maximum turning speed.
		/// </summary>
		private static float TurnToFace (Vector2 position, Vector2 faceThis,
		float currentAngle, float turnSpeed)
		{
			// consider this diagram:
			//         B 
			//        /|
			//      /  |
			//    /    | y
			//  / o    |
			// A--------
			//     x
			// 
			// where A is the position of the object, B is the position of the target,
			// and "o" is the angle that the object should be facing in order to 
			// point at the target. we need to know what o is. using trig, we know that
			//      tan(theta)       = opposite / adjacent
			//      tan(o)           = y / x
			// if we take the arctan of both sides of this equation...
			//      arctan( tan(o) ) = arctan( y / x )
			//      o                = arctan( y / x )
			// so, we can use x and y to find o, our "desiredAngle."
			// x and y are just the differences in position between the two objects.
			float x = faceThis.X - position.X;
			float y = faceThis.Y - position.Y;

			// we'll use the Atan2 function. Atan will calculates the arc tangent of 
			// y / x for us, and has the added benefit that it will use the signs of x
			// and y to determine what cartesian quadrant to put the result in.
			// http://msdn2.microsoft.com/en-us/library/system.math.atan2.aspx
			float desiredAngle = (float)Math.Atan2 (y, x);

			// so now we know where we WANT to be facing, and where we ARE facing...
			// if we weren't constrained by turnSpeed, this would be easy: we'd just 
			// return desiredAngle.
			// instead, we have to calculate how much we WANT to turn, and then make
			// sure that's not more than turnSpeed.

			// first, figure out how much we want to turn, using WrapAngle to get our
			// result from -Pi to Pi ( -180 degrees to 180 degrees )
			float difference = WrapAngle (desiredAngle - currentAngle);

			// clamp that between -turnSpeed and turnSpeed.
			difference = MathHelper.Clamp (difference, -turnSpeed, turnSpeed);

			// so, the closest we can get to our target is currentAngle + difference.
			// return that, using WrapAngle again.
			return WrapAngle (currentAngle + difference);
		}

		/// <summary>
		/// Returns the angle expressed in radians between -Pi and Pi.
		/// <param name="radians">the angle to wrap, in radians.</param>
		/// <returns>the input value expressed in radians from -Pi to Pi.</returns>
		/// </summary>
		private static float WrapAngle (float radians)
		{
			while (radians < -MathHelper.Pi) {
				radians += MathHelper.TwoPi;
			}
			while (radians > MathHelper.Pi) {
				radians -= MathHelper.TwoPi;
			}
			return radians;
		}

		/// <summary>
		/// This is called when the game should draw itself. Nothing too fancy in here,
		/// we'll just call Begin on the SpriteBatch, and then draw the tank, cat, and 
		/// mouse, and some overlay text. Once we're finished drawing, we'll call
		/// SpriteBatch.End.
		/// </summary>
		protected override void Draw (GameTime gameTime)
		{
			GraphicsDevice device = graphics.GraphicsDevice;

			device.Clear (Color.CornflowerBlue);

			spriteBatch.Begin ();

			// draw the tank, cat and mouse...
			spriteBatch.Draw (tankTexture, tankPosition, null, Color.White, 
		tankOrientation, tankTextureCenter, 1.0f, SpriteEffects.None, 0.0f);
			spriteBatch.Draw (catTexture, catPosition, null, Color.White, 
		0.0f, catTextureCenter, 1.0f, SpriteEffects.None, 0.0f);
			spriteBatch.Draw (mouseTexture, mousePosition, null, Color.White, 
		mouseOrientation, mouseTextureCenter, 1.0f, SpriteEffects.None, 0.0f);

			// and then draw some text showing the tank's and mouse's current state.
			// to make the text stand out more, we'll draw the text twice, once black
			// and once white, to create a drop shadow effect.
			Vector2 shadowOffset = Vector2.One;

			spriteBatch.DrawString (spriteFont, "Tank State: \n" + tankState.ToString (), 
		new Vector2 (10, 10) + shadowOffset, Color.Black);
			spriteBatch.DrawString (spriteFont, "Tank State: \n" + tankState.ToString (), 
		new Vector2 (10, 10), Color.White);

			spriteBatch.DrawString (spriteFont, "Mouse State: \n" + mouseState.ToString (), 
		new Vector2 (10, 90) + shadowOffset, Color.Black);
			spriteBatch.DrawString (spriteFont, "Mouse State: \n" + mouseState.ToString (), 
		new Vector2 (10, 90), Color.White);

			spriteBatch.End ();

			base.Draw (gameTime);
		}

	#endregion

	#region Handle Input

		/// <summary>
		/// Handles input for quitting the game.
		/// </summary>
		void HandleInput ()
		{
#if WINDOWS_PHONE
			KeyboardState currentKeyboardState = new KeyboardState();
#else
			KeyboardState currentKeyboardState = Keyboard.GetState ();
			MouseState currentMouseState = Mouse.GetState ();
#endif
			
#if IPHONE
			GamePadState currentGamePadState = GamePad.GetState (PlayerIndex.One);

			// Check for exit.
			if (currentKeyboardState.IsKeyDown (Keys.Escape) || 
				currentGamePadState.Buttons.Back == ButtonState.Pressed) {
				Exit ();
			}
#else
			// Check for exit.
			if (currentKeyboardState.IsKeyDown (Keys.Escape)) {
				Exit ();
			}
			
#endif			

			// check to see if the user wants to move the cat. we'll create a vector
			// called catMovement, which will store the sum of all the user's inputs.
			Vector2 catMovement = Vector2.Zero;

			//Move toward the touch point. We slow down the cat when it gets within a distance of MaxCatSpeed to the touch point.
			float smoothStop = 1;			
			
#if IPHONE			
			// check to see if the user wants to move the cat. we'll create a vector
			// called catMovement, which will store the sum of all the user's inputs.
			catMovement = currentGamePadState.ThumbSticks.Left;

			// flip y: on the thumbsticks, down is -1, but on the screen, down is bigger
			// numbers.
			catMovement.Y *= -1;

			if (currentKeyboardState.IsKeyDown (Keys.Left) || 
				currentGamePadState.DPad.Left == ButtonState.Pressed) {
				catMovement.X -= 1.0f;
			}
			if (currentKeyboardState.IsKeyDown (Keys.Right) || 
				currentGamePadState.DPad.Right == ButtonState.Pressed) {
				catMovement.X += 1.0f;
			}
			if (currentKeyboardState.IsKeyDown (Keys.Up) || 
				currentGamePadState.DPad.Up == ButtonState.Pressed) {
				catMovement.Y -= 1.0f;
			}
			if (currentKeyboardState.IsKeyDown (Keys.Down) || 
				currentGamePadState.DPad.Down == ButtonState.Pressed) {
				catMovement.Y += 1.0f;
			}
			
			TouchCollection currentTouchCollection = TouchPanel.GetState();
			
			if (currentTouchCollection != null )
            {
				if (currentTouchCollection.Count > 0)
	            {
					Vector2 touchPosition = currentTouchCollection[0].Position;
		            if (touchPosition != catPosition)
		            {
		                catMovement = touchPosition - catPosition;
		                float delta = MaxCatSpeed - MathHelper.Clamp(catMovement.Length(), 0, MaxCatSpeed);
		                smoothStop = 1 - delta / MaxCatSpeed;
		            }
				}
			}		
#else


			if (currentKeyboardState.IsKeyDown (Keys.Left)) {
				catMovement.X -= 1.0f;
			}
			if (currentKeyboardState.IsKeyDown (Keys.Right)) {
				catMovement.X += 1.0f;
			}
			if (currentKeyboardState.IsKeyDown (Keys.Up)) {
				catMovement.Y -= 1.0f;
			}
			if (currentKeyboardState.IsKeyDown (Keys.Down)) {
				catMovement.Y += 1.0f;
			}			

			Vector2 mousePosition = new Vector2 (currentMouseState.X, currentMouseState.Y);
			if (currentMouseState.LeftButton == ButtonState.Pressed && mousePosition != catPosition) {
				catMovement = mousePosition - catPosition;
				float delta = MaxCatSpeed - MathHelper.Clamp (catMovement.Length (), 0, MaxCatSpeed);
				smoothStop = 1 - delta / MaxCatSpeed;
			}
#endif

			// normalize the user's input, so the cat can never be going faster than
			// CatSpeed.
			if (catMovement != Vector2.Zero) {
				catMovement.Normalize ();
			}

			catPosition += catMovement * MaxCatSpeed * smoothStop;
		}

	#endregion
	}
}