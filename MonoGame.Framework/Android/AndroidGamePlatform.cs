// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Framework
{
    class AndroidGamePlatform : GamePlatform
    {
        public AndroidGamePlatform(Game game)
            : base(game)
        {
            System.Diagnostics.Debug.Assert(Game.Activity != null, "Must set Game.Activity before creating the Game instance");
            Game.Activity.Game = Game;
            AndroidGameActivity.Paused += Activity_Paused;
            AndroidGameActivity.Resumed += Activity_Resumed;

            _gameWindow = new AndroidGameWindow(Game.Activity, game);
            Window = _gameWindow;
        }

        private bool _initialized;
        public static bool IsPlayingVdeo { get; set; }
        private bool _exiting = false;
        private AndroidGameWindow _gameWindow;

        public override void Exit()
        {
            //TODO: Fix this
            try
            {
				if (!_exiting)
				{
					_exiting = true;
					AndroidGameActivity.Paused -= Activity_Paused;
					AndroidGameActivity.Resumed -= Activity_Resumed;
					Game.DoExiting();
                    Net.NetworkSession.Exit();
               	    Game.Activity.Finish();
				    _gameWindow.GameView.Close();
				}
            }
            catch
            {
            }
        }

        public override void RunLoop()
        {
            throw new NotSupportedException("The Android platform does not support synchronous run loops");
        }

        public override void StartRunLoop()
        {
			_gameWindow.GameView.Resume();
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            if (!_initialized)
            {
                Game.DoInitialize();
                _initialized = true;
            }

            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            PrimaryThreadLoader.DoLoads();
            return !IsPlayingVdeo;
        }

        public override void BeforeInitialize()
        {
            // TODO: Determine whether device natural orientation is Portrait or Landscape for OrientationListener
            //SurfaceOrientation currentOrient = Game.Activity.WindowManager.DefaultDisplay.Rotation;

			switch (Game.Activity.Resources.Configuration.Orientation)
            {
                case Android.Content.Res.Orientation.Portrait:
					_gameWindow.SetOrientation(DisplayOrientation.Portrait, false);
                    break;
                case Android.Content.Res.Orientation.Landscape:
					_gameWindow.SetOrientation(DisplayOrientation.LandscapeLeft, false);
                    break;
                default:
					_gameWindow.SetOrientation(DisplayOrientation.LandscapeLeft, false);
                    break;
            }
            base.BeforeInitialize();
            _gameWindow.GameView.TouchEnabled = true;
        }

        public override bool BeforeRun()
        {

            // Run it as fast as we can to allow for more response on threaded GPU resource creation
			_gameWindow.GameView.Run();

            return false;
        }

        public override void EnterFullScreen()
        {
        }

        public override void ExitFullScreen()
        {
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            // Force the Viewport to be correctly set
            Game.graphicsDeviceManager.ResetClientBounds();
        }

        // EnterForeground
        void Activity_Resumed(object sender, EventArgs e)
        {
            if (!IsActive)
            {
                IsActive = true;
				_gameWindow.GameView.Resume();
				SoundEffectInstance.SoundPool.AutoResume();
				if(_MediaPlayer_PrevState == MediaState.Playing && Game.Activity.AutoPauseAndResumeMediaPlayer)
                	MediaPlayer.Resume();
				if (!_gameWindow.GameView.IsFocused)
					_gameWindow.GameView.RequestFocus();
            }
        }

		MediaState _MediaPlayer_PrevState = MediaState.Stopped;
	    // EnterBackground
        void Activity_Paused(object sender, EventArgs e)
        {
            if (IsActive)
            {
                IsActive = false;
				_MediaPlayer_PrevState = MediaPlayer.State;
				_gameWindow.GameView.Pause();
				_gameWindow.GameView.ClearFocus();
				SoundEffectInstance.SoundPool.AutoPause();
				if(Game.Activity.AutoPauseAndResumeMediaPlayer)
                	MediaPlayer.Pause();
            }
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Asynchronous; }
        }
		
		public override void Log(string Message) 
		{
#if LOGGING
			Android.Util.Log.Debug("MonoGameDebug", Message);
#endif
		}
		
        public override void Present()
        {
			if (_exiting)
                return;
            try
            {
                var device = Game.GraphicsDevice;
                if (device != null)
                    device.Present();

				_gameWindow.GameView.SwapBuffers();
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("Error in swap buffers", ex.ToString());
            }
        }
    }
}
