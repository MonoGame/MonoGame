#region File Description
//-----------------------------------------------------------------------------
// ParticleSampleGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;

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

namespace ParticleSample
{
    /// <summary>
    /// This is the main type for the ParticleSample, and inherits from the Framework's
    /// Game class. It creates three different kinds of ParticleSystems, and then adds
    /// them to its components collection. It also has keeps a random number generator,
    /// a SpriteBatch, and a ContentManager that the different classes in this sample
    /// can share.
    /// </summary>
    public class ParticleSampleGame : Game
    {
        #region Fields and Properties

        GraphicsDeviceManager graphics;

        // The particle systems will all need a SpriteBatch to draw their particles,
        // so let's make one they can share. We'll use this to draw our SpriteFont
        // too.
        SpriteBatch spriteBatch;
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        // Used to draw the instructions on the screen.
        SpriteFont arialFont;
        
        // a random number generator that the whole sample can share.
        private static Random random = new Random();
        public static Random Random
        {
            get { return random; }
        }

        // Here's the really fun part of the sample, the particle systems! These are
        // drawable game components, so we can just add them to the components
        // collection. Read more about each particle system in their respective source
        // files.
        ExplosionParticleSystem explosion;
        ExplosionSmokeParticleSystem smoke;
        SmokePlumeParticleSystem smokePlume;

        // State is an enum that represents which effect we're currently demoing.
        enum State
        {
            Explosions,
            SmokePlume
        };
        // the number of values in the "State" enum.
        const int NumStates = 2;
        State currentState = State.Explosions;

        // a timer that will tell us when it's time to trigger another explosion.
        const float TimeBetweenExplosions = 2.0f;
        float timeTillExplosion = 0.0f;

        // keep a timer that will tell us when it's time to add more particles to the
        // smoke plume.
        const float TimeBetweenSmokePlumePuffs = .5f;
        float timeTillPuff = 0.0f;

        // keep track of the last frame's keyboard and gamepad state, so that we know
        // if the user has pressed a button.
        KeyboardState lastKeyboardState;
        GamePadState lastGamepadState;

        #endregion

        #region Initialization

        public ParticleSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 320;
            graphics.PreferredBackBufferHeight = 480;

            Content.RootDirectory = "Content";

            // create the particle systems and add them to the components list.
            // we should never see more than one explosion at once
            explosion = new ExplosionParticleSystem(this, 1);
            Components.Add(explosion);

            // but the smoke from the explosion lingers a while.
            smoke = new ExplosionSmokeParticleSystem(this, 2);
            Components.Add(smoke);

            // we'll see lots of these effects at once; this is ok
            // because they have a fairly small number of particles per effect.
            smokePlume = new SmokePlumeParticleSystem(this, 9);
            Components.Add(smokePlume);
        }

        /// <summary>
        /// Load your graphics content. 
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            arialFont = Content.Load<SpriteFont>("Arial");
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // check the input devices to see if someone has decided they want to see
            // the other effect, if they want to quit.
            HandleInput();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            switch (currentState)
            {
                // if we should be demoing the explosions effect, check to see if it's
                // time for a new explosion.
                case State.Explosions:
                    UpdateExplosions(dt);
                    break;
                // if we're showing off the smoke plume, check to see if it's time for a
                // new puff of smoke.
                case State.SmokePlume:
                    UpdateSmokePlume(dt);
                    break;
            }

            // the base update will handle updating the particle systems themselves,
            // because we added them to the components collection.
            base.Update(gameTime);
        }
        
        // this function is called when we want to demo the smoke plume effect. it
        // updates the timeTillPuff timer, and adds more particles to the plume when
        // necessary.
        private void UpdateSmokePlume(float dt)
        {
            timeTillPuff -= dt;
            if (timeTillPuff < 0)
            {
                Vector2 where = Vector2.Zero;
                // add more particles at the bottom of the screen, halfway across.
                where.X = graphics.GraphicsDevice.Viewport.Width / 2;
                where.Y = graphics.GraphicsDevice.Viewport.Height;
                smokePlume.AddParticles(where);

                // and then reset the timer.
                timeTillPuff = TimeBetweenSmokePlumePuffs;
            }
        }

        // this function is called when we want to demo the explosion effect. it
        // updates the timeTillExplosion timer, and starts another explosion effect
        // when the timer reaches zero.
        private void UpdateExplosions(float dt)
        {
            timeTillExplosion -= dt;
            if (timeTillExplosion < 0)
            {
                Vector2 where = Vector2.Zero;
                // create the explosion at some random point on the screen.
                where.X = RandomBetween(0, graphics.GraphicsDevice.Viewport.Width);
                where.Y = RandomBetween(0, graphics.GraphicsDevice.Viewport.Height);

                // the overall explosion effect is actually comprised of two particle
                // systems: the fiery bit, and the smoke behind it. add particles to
                // both of those systems.
                explosion.AddParticles(where);
                smoke.AddParticles(where);

                // reset the timer.
                timeTillExplosion = TimeBetweenExplosions;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // draw some instructions on the screen
            string message = string.Format("Touch the Screen\nto switch effects.\n\n" +
			    "Current effect:\n {0}!\n\n" +                 
                "Free particles:\n" +
                "Explosion:     {1}\n" +
                "ExplosionSmoke:{2}\n" +
                "SmokePlume:    {3}",
                currentState, explosion.FreeParticleCount,
                smoke.FreeParticleCount, smokePlume.FreeParticleCount );
                spriteBatch.DrawString(arialFont, message, new Vector2(10, 10), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        // This function will check to see if the user has just pushed the A button or
        // the space bar. If so, we should go to the next effect.
        private void HandleInput()
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);
			TouchCollection currentTouchCollection = TouchPanel.GetState();

            // Allows the game to exit
            if (currentGamePadState.Buttons.Back == ButtonState.Pressed || 
                currentKeyboardState.IsKeyDown(Keys.Escape))
                this.Exit();


            // check to see if someone has just released the space bar.            
            bool keyboardSpace =
                currentKeyboardState.IsKeyUp(Keys.Space) &&
                lastKeyboardState.IsKeyDown(Keys.Space);


            // check the gamepad to see if someone has just released the A button.
            bool gamepadA =
                currentGamePadState.Buttons.A == ButtonState.Pressed &&
                lastGamepadState.Buttons.A == ButtonState.Released;
			
			bool touched = false;
				
			// tap the screen to select				
			foreach (TouchLocation location in currentTouchCollection)
			{
			    switch (location.State)
			    {
			        case TouchLocationState.Pressed:	
						touched = true;	
			            break;
			        case TouchLocationState.Moved:
			            break;
			        case TouchLocationState.Released:
			            break;
			    }	
			}
            

            // if either the A button or the space bar was just released, move to the
            // next state. Doing modulus by the number of states lets us wrap back
            // around to the first state.
            if (keyboardSpace || gamepadA || touched)
            {
                currentState = (State)((int)(currentState + 1) % NumStates);
            }

            lastKeyboardState = currentKeyboardState;
            lastGamepadState = currentGamePadState;
        }

        #endregion

        #region Helper Functions

        //  a handy little function that gives a random float between two
        // values. This will be used in several places in the sample, in particilar in
        // ParticleSystem.InitializeParticle.
        public static float RandomBetween(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        #endregion
    }
}
