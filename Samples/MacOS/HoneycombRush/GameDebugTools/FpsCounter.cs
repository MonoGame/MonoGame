#region File Description
//-----------------------------------------------------------------------------
// FpsCounter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace HoneycombRush.GameDebugTools
{
    /// <summary>
    /// Component for FPS measure and draw.
    /// </summary>
    public class FpsCounter : DrawableGameComponent
    {
        #region Properties

        /// <summary>
        /// Gets current FPS
        /// </summary>
        public float Fps { get; private set; }

        /// <summary>
        /// Gets/Sets FPS sample duration.
        /// </summary>
        public TimeSpan SampleSpan { get; set; }

        #endregion

        #region Fields

        // Reference for debug manager.
        private DebugManager debugManager;

        // Stopwatch for fps measuring.
        private Stopwatch stopwatch;

        private int sampleFrames;

        // stringBuilder for FPS counter draw.
        private StringBuilder stringBuilder = new StringBuilder(16);

        #endregion

        #region Initialize

        public FpsCounter(Game game)
            : base(game)
        {
            SampleSpan = TimeSpan.FromSeconds(1);
        }

        public override void Initialize()
        {
            DrawOrder = int.MaxValue - 1;

            // Get debug manager from game service.
            debugManager =
                Game.Services.GetService(typeof(DebugManager)) as DebugManager;

            if (debugManager == null)
                throw new InvalidOperationException("DebugManaer is not registered.");

            // Register 'fps' command if debug command is registered as a service.
            IDebugCommandHost host =
                                Game.Services.GetService(typeof(IDebugCommandHost))
                                                                as IDebugCommandHost;

            if (host != null)
            {
                host.RegisterCommand("fps", "FPS Counter", this.CommandExecute);
                Visible = false;
            }

            // Initialize parameters.
            Fps = 0;
            sampleFrames = 0;
            stopwatch = Stopwatch.StartNew();
            stringBuilder.Length = 0;

            base.Initialize();
        }

        #endregion

        /// <summary>
        /// FPS command implementation.
        /// </summary>
        private void CommandExecute(IDebugCommandHost host,
                                    string command, IList<string> arguments)
        {
            if (arguments.Count == 0)
                Visible = !Visible;

            foreach (string arg in arguments)
            {
                switch (arg.ToLower())
                {
                    case "on":
                        Visible = true;
                        break;
                    case "off":
                        Visible = false;
                        break;
                }
            }
        }

        #region Update and Draw

        public override void Update(GameTime gameTime)
        {
            if (stopwatch.Elapsed > SampleSpan)
            {
                // Update FPS value and start next sampling period.
                Fps = (float)sampleFrames / (float)stopwatch.Elapsed.TotalSeconds;

                stopwatch.Reset();
                stopwatch.Start();
                sampleFrames = 0;

                // Update draw string.
                stringBuilder.Length = 0;
                stringBuilder.Append("FPS: ");
                stringBuilder.AppendNumber(Fps);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            sampleFrames++;

            SpriteBatch spriteBatch = debugManager.SpriteBatch;
            SpriteFont font = debugManager.DebugFont;

            // Compute size of border area.
            Vector2 size = font.MeasureString("X");
            Rectangle rc =
                new Rectangle(0, 0, (int)(size.X * 14f), (int)(size.Y * 1.3f));

            Layout layout = new Layout(spriteBatch.GraphicsDevice.Viewport);
            rc = layout.Place(rc, 0.01f, 0.01f, Alignment.TopLeft);

            // Place FPS string in border area.
            size = font.MeasureString(stringBuilder);
            layout.ClientArea = rc;
            Vector2 pos = layout.Place(size, 0, 0.1f, Alignment.Center);

            // Draw
            spriteBatch.Begin();
            spriteBatch.Draw(debugManager.WhiteTexture, rc, new Color(0, 0, 0, 128));
            spriteBatch.DrawString(font, stringBuilder, pos, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

    }
}
