// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;

namespace Microsoft.Xna.Framework
{
    public interface IAndroidGameActivity
    {
        /// <summary>
        /// One time initialization.
        /// </summary>
        /// <remarks>
        /// Used internally by the framework.
        /// </remarks>
        void InitializeGame(Game game);

        bool AutoPauseAndResumeMediaPlayer { get; }
        bool RenderOnUIThread { get; }
    }

    [CLSCompliant(false)]
    public class AndroidGameActivity : Activity, IAndroidGameActivity
    {
        void IAndroidGameActivity.InitializeGame(Game game)
        {
            _game = game;
        }

        private Game _game;

        private ScreenReceiver screenReceiver;
        private OrientationListener _orientationListener;

        public bool AutoPauseAndResumeMediaPlayer { get; set; } = true;
        public bool RenderOnUIThread { get; set; } = true; 

		/// <summary>
		/// OnCreate called when the activity is launched from cold or after the app
		/// has been killed due to a higher priority app needing the memory
		/// </summary>
		/// <param name='savedInstanceState'>
		/// Saved instance state.
		/// </param>
		protected override void OnCreate (Bundle savedInstanceState)
		{
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

			IntentFilter filter = new IntentFilter();
		    filter.AddAction(Intent.ActionScreenOff);
		    filter.AddAction(Intent.ActionScreenOn);
		    filter.AddAction(Intent.ActionUserPresent);
		    
		    screenReceiver = new ScreenReceiver();
		    RegisterReceiver(screenReceiver, filter);

            _orientationListener = new OrientationListener(this);

			Game.Activity = this;
		}

        public static event EventHandler Paused;

		public override void OnConfigurationChanged (Android.Content.Res.Configuration newConfig)
		{
			// we need to refresh the viewport here.
			base.OnConfigurationChanged (newConfig);
		}

        protected override void OnPause()
        {
            base.OnPause();
            RaiseOnPause(this, _game);

            if (_orientationListener.CanDetectOrientation())
                _orientationListener.Disable();
        }

        public static event EventHandler Resumed;
        protected override void OnResume()
        {
            base.OnResume();
            RaiseOnResume(this, _game);

            if (_orientationListener.CanDetectOrientation())
                _orientationListener.Enable();
        }

		protected override void OnDestroy ()
		{
            UnregisterReceiver(screenReceiver);
            ScreenReceiver.ScreenLocked = false;
            _orientationListener = null;
            if (_game != null)
                _game.Dispose();
            _game = null;
			base.OnDestroy ();
		}

        public static void RaiseOnPause(Object sender, Game game)
        {
            EventHelpers.Raise(sender, Paused, EventArgs.Empty);
        }

        public static void RaiseOnResume(Object sender, Game game)
        {
            EventHelpers.Raise(sender, Resumed, EventArgs.Empty);

            if (game != null)
            {
                var deviceManager = (IGraphicsDeviceManager)game.Services.GetService(typeof(IGraphicsDeviceManager));
                if (deviceManager == null)
                    return;
                ((GraphicsDeviceManager)deviceManager).ForceSetFullScreen();
                ((AndroidGameWindow)game.Window).GameView.RequestFocus();                
            }
        }
    }

    

	[CLSCompliant(false)]
	public static class ActivityExtensions
    {
        public static ActivityAttribute GetActivityAttribute<T>(this T obj)
            where T:Activity, IAndroidGameActivity
        {			
            var attr = obj.GetType().GetCustomAttributes(typeof(T), true);
			if (attr != null)
			{
            	return ((ActivityAttribute)attr[0]);
			}
			return null;
        }
    }

}
