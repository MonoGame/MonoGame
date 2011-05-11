#region File Description
//-----------------------------------------------------------------------------
// Game1.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace TouchGestureSample
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private Texture2D cat;

        // the text we display on screen, created here to make our Draw method cleaner
        private const string helpText = 
            "Hold (in empty space) - Create sprite\n" +
            "Hold (on sprite) - Remove sprite\n" +
            "Tap - Change sprite color\n" + 
            "Drag - Move sprite\n" + 
            "Flick - Throws sprite\n" +
            "Pinch - Scale sprite";

        // a list to hold all of our sprites
        private List<Sprite> sprites = new List<Sprite>();

        // we track our selected sprite so we can drag it around
        private Sprite selectedSprite;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = true;

            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
        }

        protected override void Initialize()
        {
            // enable the gestures we care about. you must set EnabledGestures before
            // you can use any of the other gesture APIs.
            // we use both Tap and DoubleTap to workaround a bug in the XNA GS 4.0 Beta
            // where some Taps are missed if only Tap is specified.
            TouchPanel.EnabledGestures =
                GestureType.Hold |
                GestureType.Tap | 
                GestureType.DoubleTap |
                GestureType.FreeDrag |
                GestureType.Flick |
                GestureType.Pinch;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            cat = Content.Load<Texture2D>("cat");
            font = Content.Load<SpriteFont>("Font");
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // handle the touch input
            HandleTouchInput();

            // update all of the sprites
            foreach (Sprite sprite in sprites)
            {
                sprite.Update(gameTime, GraphicsDevice.Viewport.Bounds);
            }            

            base.Update(gameTime);
        }

        private void HandleTouchInput()
        {
            // we use raw touch points for selection, since they are more appropriate
            // for that use than gestures. so we need to get that raw touch data.
            TouchCollection touches = TouchPanel.GetState();

            // see if we have a new primary point down. when the first touch
            // goes down, we do hit detection to try and select one of our sprites.
            if (touches.Count > 0 && touches[0].State == TouchLocationState.Pressed)
            {
                // convert the touch position into a Point for hit testing
                Point touchPoint = new Point((int)touches[0].Position.X, (int)touches[0].Position.Y);

                // iterate our sprites to find which sprite is being touched. we iterate backwards
                // since that will cause sprites that are drawn on top to be selected before
                // sprites drawn on the bottom.
                selectedSprite = null;
                for (int i = sprites.Count - 1; i >= 0; i--)
                {
                    Sprite sprite = sprites[i];
                    if (sprite.HitBounds.Contains(touchPoint))
                    {
                        selectedSprite = sprite;
                        break;
                    }
                }

                if (selectedSprite != null)
                {
                    // make sure we stop selected sprites
                    selectedSprite.Velocity = Vector2.Zero;

                    // we also move the sprite to the end of the list so it
                    // draws on top of the other sprites
                    sprites.Remove(selectedSprite);
                    sprites.Add(selectedSprite);
                }
            }

            // next we handle all of the gestures. since we may have multiple gestures available,
            // we use a loop to read in all of the gestures. this is important to make sure the 
            // TouchPanel's queue doesn't get backed up with old data
            while (TouchPanel.IsGestureAvailable)
            {
                // read the next gesture from the queue
                GestureSample gesture = TouchPanel.ReadGesture();

                // we can use the type of gesture to determine our behavior
                switch (gesture.GestureType)
                {
                    // on taps, we change the color of the selected sprite
                    case GestureType.Tap:
                    case GestureType.DoubleTap:
                        if (selectedSprite != null)
                        {
                            selectedSprite.ChangeColor();
                        }
                        break;

                    // on holds, if no sprite is selected, we add a new sprite at the
                    // hold position and make it our selected sprite. otherwise we
                    // remove our selected sprite.
                    case GestureType.Hold:
                        if (selectedSprite == null)
                        {
                            // create the new sprite
                            selectedSprite = new Sprite(cat);
                            selectedSprite.Center = gesture.Position;

                            // add it to our list
                            sprites.Add(selectedSprite);
                        }
                        else
                        {
                            sprites.Remove(selectedSprite);
                            selectedSprite = null;
                        }
                        break;

                    // on drags, we just want to move the selected sprite with the drag
                    case GestureType.FreeDrag:
                        if (selectedSprite != null)
                        {
                            selectedSprite.Center += gesture.Delta;
                        }
                        break;

                    // on flicks, we want to update the selected sprite's velocity with
                    // the flick velocity, which is in pixels per second.
                    case GestureType.Flick:
                        if (selectedSprite != null)
                        {
                            selectedSprite.Velocity = gesture.Delta;
                        }
                        break;

                    // on pinches, we want to scale the selected sprite
                    case GestureType.Pinch:
                        if (selectedSprite != null)
                        {
                            // get the current and previous locations of the two fingers
                            Vector2 a = gesture.Position;
                            Vector2 aOld = gesture.Position - gesture.Delta;
                            Vector2 b = gesture.Position2;
                            Vector2 bOld = gesture.Position2 - gesture.Delta2;

                            // figure out the distance between the current and previous locations
                            float d = Vector2.Distance(a, b);
                            float dOld = Vector2.Distance(aOld, bOld);

                            // calculate the difference between the two and use that to alter the scale
                            float scaleChange = (d - dOld) * .01f;
                            selectedSprite.Scale += scaleChange;
                        }
                        break;
                }
            }

            // lastly, if there are no raw touch points, we make sure no sprites are selected.
            // this happens after we handle gestures because some gestures like taps and flicks
            // will come in on the same frame as our raw touch points report no touches and we
            // still want to use the selected sprite for those gestures.
            if (touches.Count == 0)
            {
                selectedSprite = null;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // draw all sprites first
            foreach (Sprite sprite in sprites)
            {
                sprite.Draw(spriteBatch);
            }

            // draw our helper text so users know what they're doing.
            spriteBatch.DrawString(font, helpText, new Vector2(10f, 32f), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
