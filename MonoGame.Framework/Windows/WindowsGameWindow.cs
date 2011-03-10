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
using OpenTK;
using OpenTK.Graphics;

#endregion Using Statements

namespace Microsoft.Xna.Framework
{
    internal class WindowsGameWindow : GameWindow
    {
		private Rectangle clientBounds;
		private GameTime _updateGameTime;
        private GameTime _drawGameTime;
        private DateTime _lastUpdate;
		private DateTime _now;

        internal Game Game;
        internal OpenTK.GameWindow OpenTkGameWindow { get; private set; }

        public WindowsGameWindow() 
        {
            Initialize();
        }

        private void Initialize()
        {
            OpenTkGameWindow = new OpenTK.GameWindow();
            OpenTkGameWindow.RenderFrame += OnRenderFrame;
            OpenTkGameWindow.UpdateFrame += OnUpdateFrame;
            clientBounds = new Rectangle(0, 0, OpenTkGameWindow.Width, OpenTkGameWindow.Height);

            // Initialize GameTime
            _updateGameTime = new GameTime();
            _drawGameTime = new GameTime();

            // Initialize _lastUpdate
            _lastUpdate = DateTime.Now;
        }

        #region GameWindow Methods

        private void OnRenderFrame(object sender, FrameEventArgs e)
        {
            if (GraphicsContext.CurrentContext == null || GraphicsContext.CurrentContext.IsDisposed)
                return;

            //Should not happen at all..
            if (!GraphicsContext.CurrentContext.IsCurrent)
                OpenTkGameWindow.MakeCurrent();

            if (Game != null) {
                _drawGameTime.Update(_now - _lastUpdate);
                _lastUpdate = _now;
                Game.DoDraw(_drawGameTime);
            }

            OpenTkGameWindow.SwapBuffers();
        }

        private void OnUpdateFrame(object sender, FrameEventArgs e)
		{			
			if (Game != null )
			{
				_now = DateTime.Now;
				_updateGameTime.Update(_now - _lastUpdate);
            	Game.DoUpdate(_updateGameTime);
			}
		}
		
		#endregion

        public override IntPtr Handle
        {
            get { return IntPtr.Zero; }
        }

        public override string ScreenDeviceName 
		{
			get { return string.Empty; }
		}

		public override Rectangle ClientBounds 
		{
			get 
			{
				return clientBounds;
			}
		}

        protected override void SetTitle(string title)
        {
            OpenTkGameWindow.Title = title;
        }

        public new string Title
        {
            get { return OpenTkGameWindow.Title; }
            set { SetTitle(value); }
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

        private DisplayOrientation _currentOrientation;
		public override DisplayOrientation CurrentOrientation 
		{
            get
            {
                return _currentOrientation;
            }
            internal set
            {
                if (value != _currentOrientation)
                {
                    _currentOrientation = value;
                    OnOrientationChanged();
                }
            }
		}

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
           
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
           
        }
    }
}

