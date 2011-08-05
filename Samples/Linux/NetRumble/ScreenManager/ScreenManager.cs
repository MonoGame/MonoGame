#region File Description
//-----------------------------------------------------------------------------
// ScreenManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace NetRumble
{
    /// <summary>
    /// The screen manager is a component which manages one or more GameScreen
    /// instances. It maintains a stack of screens, calls their Update and Draw
    /// methods at the appropriate times, and automatically routes input to the
    /// topmost active screen.
    /// </summary>
    /// <remarks>
    /// This public class is similar to one in the GameStateManagement sample.
    /// </remarks>
    public class ScreenManager : DrawableGameComponent
    {
        #region Fields

        List<GameScreen> screens = new List<GameScreen>();
        List<GameScreen> screensToUpdate = new List<GameScreen>();
        List<GameScreen> screensToDraw = new List<GameScreen>();

        InputState input = new InputState();

        IGraphicsDeviceService graphicsDeviceService;
        public SignedInGamer invited;

        ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D blankTexture;
        Rectangle titleSafeArea;

        bool traceEnabled;

        #endregion


        #region Properties

        /// <summary>
        /// Expose access to our Game instance (this is protected in the
        /// default GameComponent, but we want to make it public).
        /// </summary>
        new public Game Game
        {
            get { return base.Game; }
        }


        /// <summary>
        /// Expose access to our graphics device (this is protected in the
        /// default DrawableGameComponent, but we want to make it public).
        /// </summary>
        new public GraphicsDevice GraphicsDevice
        {
            get { return base.GraphicsDevice; }
        }

        
        /// <summary>
        /// A content manager used to load data that is shared between multiple
        /// screens. This is never unloaded, so if a screen requires a large amount
        /// of temporary data, it should create a local content manager instead.
        /// </summary>
        public ContentManager Content
        {
            get { return content; }
        }


        /// <summary>
        /// A default SpriteBatch shared by all the screens. This saves
        /// each screen having to bother creating their own local instance.
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }


        /// <summary>
        /// A default font shared by all the screens. This saves
        /// each screen having to bother loading their own local copy.
        /// </summary>
        public SpriteFont Font
        {
            get { return font; }
        }


        /// <summary>
        /// If true, the manager prints out a list of all the screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        public bool TraceEnabled
        {
            get { return traceEnabled; }
            set { traceEnabled = value; }
        }


        /// <summary>
        /// The title-safe area for the menus.
        /// </summary>
        public Rectangle TitleSafeArea
        {
            get { return titleSafeArea; }
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public ScreenManager(Game game)
            : base(game)
        {
            content = new ContentManager(game.Services, "Content");

            graphicsDeviceService = (IGraphicsDeviceService)game.Services.GetService(
                                                        typeof(IGraphicsDeviceService));

            if (graphicsDeviceService == null)
                throw new InvalidOperationException("No graphics device service.");

            invited = null;
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load content belonging to the screen manager.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = content.Load<SpriteFont>("Fonts/MenuFont");
            blankTexture = content.Load<Texture2D>("Textures/blank");

            // Tell each of the screens to load their content.
            foreach (GameScreen screen in screens)
            {
                screen.LoadContent();
            }

            // update the title-safe area
            titleSafeArea = new Rectangle(
                (int)Math.Floor(GraphicsDevice.Viewport.X + 
                   GraphicsDevice.Viewport.Width * 0.05f),
                (int)Math.Floor(GraphicsDevice.Viewport.Y + 
                   GraphicsDevice.Viewport.Height * 0.05f), 
                (int)Math.Floor(GraphicsDevice.Viewport.Width * 0.9f), 
                (int)Math.Floor(GraphicsDevice.Viewport.Height * 0.9f));
        }


        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Unload content belonging to the screen manager.
            content.Unload();

            // Tell each of the screens to unload their content.
            foreach (GameScreen screen in screens)
            {
                screen.UnloadContent();
            }
        }


        #endregion


        #region Update and Draw


        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Read the keyboard and gamepad.
            input.Update();

            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others
            // (or it happens on another thread)
            screensToUpdate.Clear();

            foreach (GameScreen screen in screens)
                screensToUpdate.Add(screen);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are screens waiting to be updated.
            while (screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                GameScreen screen = screensToUpdate[screensToUpdate.Count - 1];

                screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

                // Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn ||
                    screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input and update presence.
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput(input);

                        screen.UpdatePresence(); // presence support

                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }

            // Print debug trace?
            if (traceEnabled)
                TraceScreens();
        }


        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        void TraceScreens()
        {
            List<string> screenNames = new List<string>();

            foreach (GameScreen screen in screens)
                screenNames.Add(screen.GetType().Name);

            Debug.WriteLine(string.Join(", ", screenNames.ToArray()));
        }


        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Make a copy of the master screen list, to avoid confusion if
            // the process of drawing one screen adds or removes others
            // (or it happens on another thread
            screensToDraw.Clear();

            foreach (GameScreen screen in screens)
                screensToDraw.Add(screen);

            foreach (GameScreen screen in screensToDraw)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;
             
                screen.Draw(gameTime);
            }
        }

        /// <summary>
        /// Draw an empty rectangle of the given size and color.
        /// </summary>
        /// <param name="rectangle">The destination rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        public void DrawRectangle(Rectangle rectangle, Color color)
        {
            //SpriteBatch.Begin();
			// We changed this to be Opaque
			spriteBatch.Begin(0,BlendState.Opaque, null, null, null);
            SpriteBatch.Draw(blankTexture, rectangle, color);
            SpriteBatch.End();
        }


        #endregion


        #region Public Methods


        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(GameScreen screen)
        {
            screen.ScreenManager = this;

            // If we have a graphics device, tell the screen to load content.
            if ((graphicsDeviceService != null) &&
                (graphicsDeviceService.GraphicsDevice != null))
            {
                screen.LoadContent();
            }
            
            screens.Add(screen);
        }


        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen screen)
        {
            // If we have a graphics device, tell the screen to unload content.
            if ((graphicsDeviceService != null) &&
                (graphicsDeviceService.GraphicsDevice != null))
            {
                screen.UnloadContent();
            }
            
            screens.Remove(screen);
            screensToUpdate.Remove(screen);
        }


        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
        public GameScreen[] GetScreens()
        {
            return screens.ToArray();
        }


        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToBlack(int alpha)
        {
            Viewport viewport = GraphicsDevice.Viewport;

            spriteBatch.Begin();

            spriteBatch.Draw(blankTexture,
                             new Rectangle(0, 0, viewport.Width, viewport.Height),
                             new Color(0, 0, 0, (byte)alpha));
            
            spriteBatch.End();
        }


        #endregion
    }
}
