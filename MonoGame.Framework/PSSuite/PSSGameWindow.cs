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

#region Using Statements
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

using Sce.Pss.Core;
using Sce.Pss.Core.Graphics;
#endregion Using Statements

namespace Microsoft.Xna.Framework
{
    public class PSSGameWindow : GameWindow
    {
		private Rectangle clientBounds;
		private Game _game;
		private GameTime _updateGameTime;
        private GameTime _drawGameTime;
        private DateTime _lastUpdate;
		private DateTime _now;
        private DisplayOrientation _currentOrientation;
		// TODO private GestureDetector gesture = null;
		private bool _needsToResetElapsedTime = false;
		private bool _isFirstTime = true;
		private TimeSpan _extraElapsedTime;

        public PSSGameWindow(Game game)
        {
            _game = game;
            Initialize();
        }		
						
        private void Initialize()
        {
            //FIXME: GraphicsDevice hasn't been registered here so it is null
            //clientBounds = new Rectangle(0, 0, _game.GraphicsDevice._graphics.Screen.Width, _game.GraphicsDevice._graphics.Screen.Height);
            clientBounds = new Rectangle(0, 0, 960, 544);

            // Initialize GameTime
            _updateGameTime = new GameTime();
            _drawGameTime = new GameTime();

            // Initialize _lastUpdate
            _lastUpdate = DateTime.Now;
			
			//TODO gesture = new GestureDetector(new GestureListener((AndroidGameActivity)this.Context));
        }

		public void ResetElapsedTime ()
		{
			_needsToResetElapsedTime = true;
		}

		public void Close ()
		{
		}
		
		void GameWindow_Closed(object sender,EventArgs e)
        {
			try
			{
        		_game.Exit();
			}
			catch(NullReferenceException)
			{
				// just in case the game is null
			}
		}

        ~PSSGameWindow()
		{
			//
		}
		
		#region GameWindow Overrides
		protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
		{
			// Do nothing.  PSS doesn't do orientation.
		}

		public override void BeginScreenDeviceChange(bool willBeFullScreen)
		{
		}
		
		public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
		{
		}

		protected override void SetTitle(string title)
		{
			//FIXME window.Title = title;
		}
		
		public override bool AllowUserResizing
		{
			get 
			{
				return false;
			}
			set 
			{
				// Do nothing; Ignore rather than raising and exception
			}
		}

		public override Rectangle ClientBounds 
		{
			get 
			{
				return clientBounds;
			}
		}
		
		public override DisplayOrientation CurrentOrientation
		{
			get { return DisplayOrientation.LandscapeLeft; }
		}
		
		public override IntPtr Handle { get { return IntPtr.Zero; } }
		
		public override string ScreenDeviceName { get { return ""; } } //FIXME
		#endregion
    }
}

