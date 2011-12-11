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
using System.Drawing;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
	public partial class Game
	{
		private GameWindow _view;
		
		#region Ctor
		
		void PlatformConstructor ()
		{
			_view = new GameWindow ();
			_view.Game = this;
			
			// default update rate
			_targetElapsedTime = TimeSpan.FromSeconds (1.0D / FramesPerSecond);
		}
		
		#endregion
		
		#region Public Properties

		public GameWindow Window {
			get {
				return _view;
			}
		}
		
		partial void PlatformTargetElapsedTimeChanged ()
		{
			if (_view.Window != null)
				_view.Window.TargetUpdateFrequency = TargetElapsedTime.TotalSeconds;
		}
		
		#endregion		
				
		#region Public Methods
		
		partial void PlatformDispose ()
		{
			_view.Dispose();
		}
		
		partial void PlatformExit ()
		{	 
			if (!_view.Window.IsExiting)
            {
                // raise the Exiting event
                Net.NetworkSession.Exit();
				Audio.Sound.DisposeSoundServices();
                _view.Window.Exit();
            }
		}

		private bool PlatformBeforeRun ()
		{
			Initialize ();
			ResetWindowBounds(false);
			
            _view.Run(1.0f /  TargetElapsedTime.TotalSeconds);
			return false;
		}
		
		#endregion
		
		#region Private / Internal Method

		private void ResetWindowBounds (bool toggleFullScreen)
		{
			Rectangle bounds;

			bounds = _view.ClientBounds;
			
			//Changing window style forces a redraw. Some games
			//have fail-logic and toggle fullscreen in their draw function,
			//so temporarily become inactive so it won't execute.
			
			bool wasActive = IsActive;
			IsActive = false;
			
			if (graphicsDeviceManager.IsFullScreen)
			{
				bounds = new Rectangle(0, 0,
				                       OpenTK.DisplayDevice.Default.Width,
						               OpenTK.DisplayDevice.Default.Height);
			}
			else
			{
				bounds.Width = graphicsDeviceManager.PreferredBackBufferWidth;
				bounds.Height = graphicsDeviceManager.PreferredBackBufferHeight;
			}
			
			if (toggleFullScreen)
				_view.ToggleFullScreen();
			
			// we only change window bounds if we are not fullscreen 
			if (!graphicsDeviceManager.IsFullScreen)
				_view.ChangeClientBounds(bounds);
			
			IsActive = wasActive;
		}
		
		internal void ResizeWindow(bool toggleFullScreen)
		{
			ResetWindowBounds(toggleFullScreen);	
		}
		
		private bool PlatformBeforeDraw(GameTime gameTime)
		{
			return false;
		}
		
		private bool PlatformBeforeUpdate(GameTime gameTime)
		{
			return false;
		}
		
		#endregion
	}
}

