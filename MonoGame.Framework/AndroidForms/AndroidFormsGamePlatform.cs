// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Android.Views;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Framework
{
    class AndroidFormsGamePlatform : GamePlatform
    {
        public AndroidFormsGamePlatform(Game game)
            : base(game)
        {
            System.Diagnostics.Debug.Assert(Game.Activity != null, "Must set Game.Activity before creating the Game instance");
            ((AndroidFormsGameActivity)Game.Activity).Game = Game;

            AndroidFormsGameActivity.Paused += Activity_Paused;
            AndroidFormsGameActivity.Resumed += Activity_Resumed;

            _gameWindow = new AndroidFormsGameWindow(Game.Activity, game);
            Window = _gameWindow;

            MediaLibrary.Context = Game.Activity;
            try
            {
                OpenALSoundController soundControllerInstance = OpenALSoundController.GetInstance;
            }
            catch (DllNotFoundException ex)
            {
                throw (new NoAudioHardwareException("Failed to init OpenALSoundController", ex));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AndroidFormsGameActivity.Paused -= Activity_Paused;
                AndroidFormsGameActivity.Resumed -= Activity_Resumed;
            }
            base.Dispose(disposing);
        }

        private bool _initialized;
        public static bool IsPlayingVdeo { get; set; }
        private AndroidFormsGameWindow _gameWindow;

        public override void Exit()
        {
            Game.Activity.MoveTaskToBack(true);
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
            var currentOrientation = AndroidFormsCompatibility.GetAbsoluteOrientation();

            switch (Game.Activity.Resources.Configuration.Orientation)
            {
                case Android.Content.Res.Orientation.Portrait:
                    this._gameWindow.SetOrientation(currentOrientation == DisplayOrientation.PortraitDown ? DisplayOrientation.PortraitDown : DisplayOrientation.Portrait, false);
                    break;
                default:
                    this._gameWindow.SetOrientation(currentOrientation == DisplayOrientation.LandscapeRight ? DisplayOrientation.LandscapeRight : DisplayOrientation.LandscapeLeft, false);
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

                if (_MediaPlayer_PrevState == MediaState.Playing && ((AndroidFormsGameActivity)Game.Activity).AutoPauseAndResumeMediaPlayer)
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
                if (((AndroidFormsGameActivity)Game.Activity).AutoPauseAndResumeMediaPlayer)
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
