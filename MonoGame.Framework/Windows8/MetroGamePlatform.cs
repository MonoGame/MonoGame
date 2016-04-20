// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

//using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Windows.ApplicationModel.Activation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
#if WINDOWS_PHONE81
using Windows.UI.Xaml;
#endif

namespace Microsoft.Xna.Framework
{
    class MetroGamePlatform : GamePlatform
    {
        internal static string LaunchParameters;

        internal static readonly TouchQueue TouchQueue = new TouchQueue();

        internal static ApplicationExecutionState PreviousExecutionState { get; set; }

        public MetroGamePlatform(Game game)
            : base(game)
        {
#if !WINDOWS_PHONE81
            // Set the starting view state so the Game class can
            // query it during construction.
            ViewState = ApplicationView.Value;
#endif
            // Setup the game window.
            Window = MetroGameWindow.Instance;
            MetroGameWindow.Instance.Game = game;

            // Setup the launch parameters.
            // - Parameters can optionally start with a forward slash.
            // - Keys can be separated from values by a colon or equals sign
            // - Double quotes can be used to enclose spaces in a key or value.
            int pos = 0;
            int paramStart = 0;
            bool inQuotes = false;
            var keySeperators = new char[] { ':', '=' };

            while (pos <= LaunchParameters.Length)
            {
                string arg = string.Empty;
                if (pos < LaunchParameters.Length)
                {
                    char c = LaunchParameters[pos];
                    if (c == '"')
                        inQuotes = !inQuotes;
                    else if ((c == ' ') && !inQuotes)
                    {
                        arg = LaunchParameters.Substring(paramStart, pos - paramStart).Replace("\"", "");
                        paramStart = pos + 1;
                    }
                }
                else
                {
                    arg = LaunchParameters.Substring(paramStart).Replace("\"", "");
                }
                ++pos;

                if (string.IsNullOrWhiteSpace(arg))
                    continue;

                string key = string.Empty;
                string value = string.Empty;
                int keyStart = 0;

                if (arg.StartsWith("/"))
                    keyStart = 1;

                if (arg.Length > keyStart)
                {
                    int keyEnd = arg.IndexOfAny(keySeperators, keyStart);

                    if (keyEnd >= 0)
                    {
                        key = arg.Substring(keyStart, keyEnd - keyStart);
                        int valueStart = keyEnd + 1;
                        if (valueStart < arg.Length)
                            value = arg.Substring(valueStart);
                    }
                    else
                    {
                        key = arg.Substring(keyStart);
                    }

                    Game.LaunchParameters.Add(key, value);
                }
            }

            Game.PreviousExecutionState = PreviousExecutionState;
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Synchronous; }
        }

        public override void RunLoop()
        {
            MetroGameWindow.Instance.RunLoop();
        }

        public override void StartRunLoop()
        {
            CompositionTarget.Rendering += (o, a) =>
            {
                MetroGameWindow.Instance.Tick();
            };
        }
        
        public override void Exit()
        {
            if (!MetroGameWindow.Instance.IsExiting)
            {
                MetroGameWindow.Instance.IsExiting = true;
#if WINDOWS_PHONE81
                Application.Current.Exit();
#endif
            }
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            TouchQueue.ProcessQueued();
            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            var device = Game.GraphicsDevice;
            if (device != null)
            {
                // For a Metro app we need to re-apply the
                // render target before every draw.  
                // 
                // I guess the OS changes it and doesn't restore it?
                device.ResetRenderTargets();
            }

            return true;
        }

        public override void EnterFullScreen()
        {
            // Metro has no concept of fullscreen vs windowed!
#if WINDOWS_PHONE81
            StatusBar.GetForCurrentView().HideAsync();
#endif
        }

        public override void ExitFullScreen()
        {
            // Metro has no concept of fullscreen vs windowed!
#if WINDOWS_PHONE81
            StatusBar.GetForCurrentView().ShowAsync();
#endif
        }
        
        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void Log(string Message)
        {
            Debug.WriteLine(Message);
        }

        public override void Present()
        {
            var device = Game.GraphicsDevice;
            if ( device != null )
                device.Present();
        }

        protected override void OnIsMouseVisibleChanged() 
        {
            MetroGameWindow.Instance.SetCursor(Game.IsMouseVisible);
        }
		
        protected override void Dispose(bool disposing)
        {
            // Make sure we dispose the graphics system.
            var graphicsDeviceManager = Game.graphicsDeviceManager;
            if (graphicsDeviceManager != null)
                graphicsDeviceManager.Dispose();

            MetroGameWindow.Instance.Dispose();
			
			base.Dispose(disposing);
        }
    }
}
