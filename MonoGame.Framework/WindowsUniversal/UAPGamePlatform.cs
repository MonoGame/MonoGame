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
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    class UAPGamePlatform : GamePlatform
    {
		//private OpenALSoundController soundControllerInstance = null;
        internal static string LaunchParameters;

        internal static readonly TouchQueue TouchQueue = new TouchQueue();

        internal static ApplicationExecutionState PreviousExecutionState { get; set; }

        public UAPGamePlatform(Game game)
            : base(game)
        {
            // Setup the game window.
            Window = UAPGameWindow.Instance;
			UAPGameWindow.Instance.Game = game;

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

            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;

            CoreApplication.Suspending += this.CoreApplication_Suspending;

            Game.PreviousExecutionState = PreviousExecutionState;
        }

        private void CoreApplication_Suspending(object sender, SuspendingEventArgs e)
        {
            if (this.Game.GraphicsDevice != null)
                this.Game.GraphicsDevice.Trim();
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Synchronous; }
        }

        private static void BackRequested(object sender, BackRequestedEventArgs e)
        {
            // We need to manually hide the keyboard input UI when the back button is pressed
            if (KeyboardInput.IsVisible)
                KeyboardInput.Cancel(null);
            else
                GamePad.Back = true;

            e.Handled = true;
        }

        public override void RunLoop()
        {
            UAPGameWindow.Instance.RunLoop();
        }

        public override void StartRunLoop()
        {
            CompositionTarget.Rendering += (o, a) =>
            {
				UAPGameWindow.Instance.Tick();
                GamePad.Back = false;
            };
        }
        
        public override void Exit()
        {
            if (!UAPGameWindow.Instance.IsExiting)
            {
				UAPGameWindow.Instance.IsExiting = true;
                Application.Current.Exit();
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
				// For a UAP app we need to re-apply the
				// render target before every draw.  
				// 
				// I guess the OS changes it and doesn't restore it?
				device.ResetRenderTargets();
            }

            return true;
        }

        public override void EnterFullScreen()
        {
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
		}

		public override void ExitFullScreen()
        {
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
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
			UAPGameWindow.Instance.SetCursor(Game.IsMouseVisible);
        }
		
        protected override void Dispose(bool disposing)
        {
            // Make sure we dispose the graphics system.
            var graphicsDeviceManager = Game.graphicsDeviceManager;
            if (graphicsDeviceManager != null)
                graphicsDeviceManager.Dispose();

			UAPGameWindow.Instance.Dispose();
			
			base.Dispose(disposing);
        }
    }
}
