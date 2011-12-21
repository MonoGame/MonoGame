#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2011 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

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
            Game.Activity.Paused += new EventHandler(Activity_Paused);
            Game.Activity.Resumed += new EventHandler(Activity_Resumed);

            Window = new AndroidGameWindow(Game.Activity, game);
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
            throw new NotImplementedException();
        }

        public override void StartRunLoop()
        {
            Window.Resume();
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

            base.BeforeInitialize();
        }

        public override bool BeforeRun()
        {
            // Get the Accelerometer going
            //TODO umcomment when the following bug is fixed
            // http://bugzilla.xamarin.com/show_bug.cgi?id=1084
            // Accelerometer currently seems to have a memory leak
            //Accelerometer.SetupAccelerometer();
            Window.Run(1 / Game.TargetElapsedTime.TotalSeconds);
            //Window.Pause();

            return false;
        }

        public override void EnterFullScreen()
        {
        }

        public override void ExitFullScreen()
        {
        }

        // EnterForeground
        void Activity_Resumed(object sender, EventArgs e)
        {
            if (!IsActive)
            {
                IsActive = true;
                Window.Resume();
                Accelerometer.Resume();
                Sound.ResumeAll();
            }
        }

        // EnterBackground
        void Activity_Paused(object sender, EventArgs e)
        {
            if (IsActive)
            {
                IsActive = false;
                Window.Pause();
                Accelerometer.Pause();
                Sound.PauseAll();
            }
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Asynchronous; }
        }
    }
}
