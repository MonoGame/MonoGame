using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework
{
    class AndroidGamePlatform : GamePlatform
    {
        public AndroidGamePlatform(Game game)
            : base(game)
        {
            System.Diagnostics.Debug.Assert(Game.Activity != null, "Must set Game.Activity before creating the Game instance");
            Game.Activity.Game = game;

            Window = new AndroidGameWindow(Game.Activity, game);
            IsActive = true;
        }

        private bool _initialized;
        public static bool IsPlayingVdeo { get; set; }

        public override void Exit()
        {
            //TODO: Fix this
            try
            {
                Net.NetworkSession.Exit();
                Window.Close();
            }
            catch
            {
            }
        }

        public override void RunLoop()
        {
            StartRunLoop();
            //throw new NotImplementedException();
        }

        public override void StartRunLoop()
        {
            // Get the Accelerometer going
            //TODO umcomment when the following bug is fixed
            // http://bugzilla.xamarin.com/show_bug.cgi?id=1084
            // Accelerometer currently seems to have a memory leak
            //Accelerometer.SetupAccelerometer();
            Window.Run(1 / Game.TargetElapsedTime.TotalSeconds);
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
            return !IsPlayingVdeo;
        }

        public override void BeforeInitialize()
        {
            switch (Window.Context.Resources.Configuration.Orientation)
            {
                case Android.Content.Res.Orientation.Portrait:
                    Window.SetOrientation(DisplayOrientation.Portrait);
                    break;
                case Android.Content.Res.Orientation.Landscape:
                    Window.SetOrientation(DisplayOrientation.LandscapeLeft);
                    break;
                default:
                    Window.SetOrientation(DisplayOrientation.LandscapeLeft);
                    break;
            }
        }

        public override bool BeforeRun()
        {
            // Get the Accelerometer going
            //TODO umcomment when the following bug is fixed
            // http://bugzilla.xamarin.com/show_bug.cgi?id=1084
            // Accelerometer currently seems to have a memory leak
            //Accelerometer.SetupAccelerometer();
            Window.Run(1 / Game.TargetElapsedTime.TotalSeconds);

            return false;
        }

        public override void EnterFullScreen()
        {
        }

        public override void ExitFullScreen()
        {
        }

        public override void EnterBackground()
        {
            if (IsActive)
            {
                IsActive = false;
                Window.Pause();
                Accelerometer.Pause();
                Sound.PauseAll();
            }
        }

        public override void EnterForeground()
        {
            if (!IsActive)
            {
                IsActive = true;
                Window.Resume();
                Accelerometer.Resume();
                Sound.ResumeAll();
            }
        }
    }
}
