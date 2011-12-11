#region License
/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License
   
using System;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Util;
using Android.Views;
using OpenTK.Graphics;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
using Android.Media;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework
{
    public partial class Game : IDisposable
    {
        internal AndroidGameWindow view;

		internal static AndroidGameActivity contextInstance;

		public static AndroidGameActivity Activity
		{
			get
			{
				return contextInstance;
			}
			set
			{
				contextInstance = value;
			}
		}

		private void PlatformConstructor()
		{
			System.Diagnostics.Debug.Assert(contextInstance != null, "Must set Game.Activity before creating the Game instance");
			contextInstance.Game = this;

            view = new AndroidGameWindow(contextInstance);
		    view.game = this;
			_content = new ContentManager(_services);
		}

        public AndroidGameWindow Window
        {
            get
            {
                return view;
            }
        }

        partial void PlatformEnterBackground()
    	{
            if (_isActive)
            {
                _isActive = false;
                view.Pause();
                Accelerometer.Pause();
				Sound.PauseAll();
            }
		}
		
		partial void PlatformEnterForeground()
    	{
            if (!_isActive)
            {
                _isActive = true;
                view.Resume();
                Accelerometer.Resume();				
				Sound.ResumeAll();
            }
		}
        
        partial void PlatformInitialize()
        {
			switch (Window.Context.Resources.Configuration.Orientation) {
				case Android.Content.Res.Orientation.Portrait :
					Window.SetOrientation(DisplayOrientation.Portrait);
					break;				
				case Android.Content.Res.Orientation.Landscape :
				    Window.SetOrientation(DisplayOrientation.LandscapeLeft);
					break;
				default:
				    Window.SetOrientation(DisplayOrientation.LandscapeLeft);
					break;
			}
		}

        partial void PlatformExit()
        {
			//TODO: Fix this
			try
			{
				Net.NetworkSession.Exit();
                view.Close();
			}
			catch
			{
			}
        }

		private bool PlatformBeforeDraw(GameTime gameTime)
		{
			return true;
		}

		private bool PlatformBeforeUpdate(GameTime gameTime)
		{
            if (!_initialized)
                Initialize();

			return true;
		}

        private bool PlatformBeforeRun()
        {
            // Get the Accelerometer going
            //TODO umcomment when the following bug is fixed
            // http://bugzilla.xamarin.com/show_bug.cgi?id=1084
            // Accelerometer currently seems to have a memory leak
            //Accelerometer.SetupAccelerometer();
            view.Run(FramesPerSecond / (FramesPerSecond * TargetElapsedTime.TotalSeconds));

            return false;
        }
    }
}

