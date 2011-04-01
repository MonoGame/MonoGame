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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

#endregion

namespace Aiming
{
    /// <summary>
    /// this sample showing how to aim one object towards another. In this sample, a
    /// spotlight turns to aim towards a cat that the player controls.
    /// </summary>
    public class AimingGame : Game
    {
        #region Constants

        // how fast can the cat move?  this is in terms of pixels per frame.
        const float CatSpeed = 10.0f;

        // how fast can the spot light turn? this is in terms of radians per frame.
        const float SpotlightTurnSpeed = 0.025f;

        #endregion

        #region Fields

        GraphicsDeviceManager graphics;

        // we'll need a spriteBatch to draw the spotlight and cat.
        SpriteBatch spriteBatch;

        // these four values control the spotlight and how it draws.
        // first is the actual sprite that we'll draw to represent the spotlight.
        Texture2D spotlightTexture;
        // next is the position of the spotlight on the screen.
        Vector2 spotlightPosition = new Vector2();
        // the origin of the spotlightTexture. The spotlight will rotate around this
        // point.
        Vector2 spotlightOrigin = new Vector2();
        // the angle that the spotlight is currently facing. this is in radians. a value
        // of 0 points to the right.
        float spotlightAngle = 0.0f;


        // these next three variables control the cat. catTexture is the sprite that
        // represents the cat...
        Texture2D catTexture;
        // ...catPosition is the cat's position on the screen...
        Vector2 catPosition = new Vector2();
        // ...and catOrigin is the origin of catTexture. the sprite will be drawn
        // centered around this value.
        Vector2 catOrigin = new Vector2();

        #endregion

        #region Initialization


        public AimingGame()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
			graphics.SupportedOrientations = DisplayOrientation.Portrait;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

            TargetElapsedTime = TimeSpan.FromTicks(333333);

#else
            graphics.PreferredBackBufferWidth = 320;
            graphics.PreferredBackBufferHeight = 480;
#endif
			graphics.IsFullScreen = true;
        }


        protected override void Initialize()
        {
            base.Initialize();

            // once base.Initialize has finished, the GraphicsDevice will have been
            // created, and we'll know how big the Viewport is. We want the spotlight
            // to be centered in the middle of the screen, so we'll use the viewport
            // to calculate where that is.
            Viewport vp = graphics.GraphicsDevice.Viewport;
            spotlightPosition.X = vp.X + vp.Width / 2;
            spotlightPosition.Y = vp.Y + vp.Height / 2;

            // we'll use the viewport size again, this time to put the cat on the
            // screen. He goes 1/4 of the way across and halfway down.
            catPosition.X = vp.X + vp.Width / 4;
            catPosition.Y = vp.Y + vp.Height / 2;
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // load our textures, and create a sprite batch...
            spotlightTexture = Content.Load<Texture2D>("spotlight");
            catTexture = Content.Load<Texture2D>("cat");
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            // now that we've loaded our textures, we can use them to calculate some
            // values that we'll use when drawing them. When we draw the spotlight,
            // it needs to rotate around the "source" of the light. since
            // spriteBatch.Draw will rotate sprites around the "origin" parameter,
            // we need spotlightOrigin to be the "source" of the light. Since I drew
            // spotlight.png myself, I happen to know that the source is halfway
            // down the left hand side of the texture.
            spotlightOrigin.X = 0;
            spotlightOrigin.Y = spotlightTexture.Height / 2;

            // Next, we want spriteBatch to draw the cat texture centered on the
            // "catPosition" vector. SpriteBatch.Draw will center the sprite on the
            // "origin" parameter, so we'll just calculate that to be the middle of
            // the texture.
            catOrigin.X = catTexture.Width / 2;
            catOrigin.Y = catTexture.Height / 2;
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            // clamp the cat's position so that it stays on the screen.
            Viewport vp = graphics.GraphicsDevice.Viewport;
            catPosition.X = MathHelper.Clamp(catPosition.X, vp.X, vp.X + vp.Width);
            catPosition.Y = MathHelper.Clamp(catPosition.Y, vp.Y, vp.Y + vp.Height);

            // use the TurnToFace function to update the spotlightAngle to face
            // towards the cat.
            spotlightAngle = TurnToFace(spotlightPosition, catPosition, spotlightAngle,
                SpotlightTurnSpeed);

            base.Update(gameTime);
        }

        /// <summary>
        /// Calculates the angle that an object should face, given its position, its
        /// target's position, its current angle, and its maximum turning speed.
        /// </summary>
        private static float TurnToFace(Vector2 position, Vector2 faceThis,
            float currentAngle, float turnSpeed)
        {
            // consider this diagram:
            //         C 
            //        /|
            //      /  |
            //    /    | y
            //  / o    |
            // S--------
            //     x
            // 
            // where S is the position of the spot light, C is the position of the cat,
            // and "o" is the angle that the spot light should be facing in order to 
            // point at the cat. we need to know what o is. using trig, we know that
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
            float desiredAngle = (float)Math.Atan2(y, x);

            // so now we know where we WANT to be facing, and where we ARE facing...
            // if we weren't constrained by turnSpeed, this would be easy: we'd just 
            // return desiredAngle.
            // instead, we have to calculate how much we WANT to turn, and then make
            // sure that's not more than turnSpeed.

            // first, figure out how much we want to turn, using WrapAngle to get our
            // result from -Pi to Pi ( -180 degrees to 180 degrees )
            float difference = WrapAngle(desiredAngle - currentAngle);

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            return WrapAngle(currentAngle + difference);
        }

        /// <summary>
        /// Returns the angle expressed in radians between -Pi and Pi.
        /// </summary>
        private static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.Black);

            // draw the cat.
            spriteBatch.Begin();
            spriteBatch.Draw(catTexture, catPosition, null, Color.White,
                0.0f, catOrigin, 1.0f, SpriteEffects.None, 0.0f);
            spriteBatch.End();

            // Start sprite batch with additive blending, and draw the spotlight.
            // Additive blending works very well for effects like lights and fire.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            spriteBatch.Draw(spotlightTexture, spotlightPosition, null, Color.White,
                spotlightAngle, spotlightOrigin, 1.0f, SpriteEffects.None, 0.0f);
            spriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        void HandleInput()
        {
#if WINDOWS_PHONE
            KeyboardState currentKeyboardState = new KeyboardState();
#else
            KeyboardState currentKeyboardState = Keyboard.GetState();
#endif
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);
            MouseState currentMouseState = Mouse.GetState();
			TouchCollection currentTouchState = TouchPanel.GetState();

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }
            // check to see if the user wants to move the cat. we'll create a vector
            // called catMovement, which will store the sum of all the user's inputs.
            Vector2 catMovement = currentGamePadState.ThumbSticks.Left;

            // flip y: on the thumbsticks, down is -1, but on the screen, down is bigger
            // numbers.
            catMovement.Y *= -1;

            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                catMovement.X -= 1.0f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                catMovement.X += 1.0f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Up) ||
                currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                catMovement.Y -= 1.0f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Down) ||
                currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                catMovement.Y += 1.0f;
            }

            //Move toward the touch point. We slow down the cat when it gets within a distance of CatSpeed to the touch point.
            float smoothStop = 1;
			
			if (currentTouchState != null )
            {
				if (currentTouchState.Count > 0)
	            {
					Vector2 touchPosition = currentTouchState[0].Position;
		            if (touchPosition != catPosition)
		            {
		                catMovement = touchPosition - catPosition;
		                float delta = CatSpeed - MathHelper.Clamp(catMovement.Length(), 0, CatSpeed);
		                smoothStop = 1 - delta / CatSpeed;
		            }
				}
			}
			
            Vector2 mousePosition = new Vector2(currentMouseState.X, currentMouseState.Y);
            if (currentMouseState.LeftButton == ButtonState.Pressed && mousePosition != catPosition)
            {
                catMovement = mousePosition - catPosition;
                float delta = CatSpeed - MathHelper.Clamp(catMovement.Length(), 0, CatSpeed);
                smoothStop = 1 - delta / CatSpeed;
            }

            // normalize the user's input, so the cat can never be going faster than
            // CatSpeed.
            if (catMovement != Vector2.Zero)
            {
                catMovement.Normalize();
            }

            catPosition += catMovement * CatSpeed * smoothStop;


        }

        #endregion
    }
}
