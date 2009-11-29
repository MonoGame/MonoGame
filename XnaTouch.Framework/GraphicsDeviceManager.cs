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

using XnaTouch.Framework.Graphics;
using System;
using MonoTouch.UIKit;
using OpenTK.Graphics.ES11;

namespace XnaTouch.Framework
{
    public class GraphicsDeviceManager : IGraphicsDeviceService, IDisposable, IGraphicsDeviceManager
    {
		private Game _game;
		private GraphicsDevice _graphicsDevice;
		private int _preferredBackBufferHeight;
		private int _preferredBackBufferWidth;
		private bool _preferMultiSampling;

        public GraphicsDeviceManager(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException("Game Cannot Be Null");
            }
			
			// Set "full screen"  as default
			UIApplication.SharedApplication.StatusBarHidden = true;
            
			_game = game;
			_preferredBackBufferHeight = game.Window.ClientBounds.Height;
			_preferredBackBufferWidth = game.Window.ClientBounds.Width;
			
            if (game.Services.GetService(typeof(IGraphicsDeviceManager)) != null)
            {
                throw new ArgumentException("Graphics Device Manager Already Present");
            }
			
            game.Services.AddService(typeof(IGraphicsDeviceManager), this);
            game.Services.AddService(typeof(IGraphicsDeviceService), this);	
			
			Initialize();
        }
		
		public void CreateDevice ()
		{
			throw new System.NotImplementedException ();
		}

		public bool BeginDraw ()
		{
			throw new NotImplementedException();
		}

		public void EndDraw ()
		{
			throw new NotImplementedException();
		}
		
		 #region IGraphicsDeviceService Members

        public event EventHandler DeviceCreated;

        public event EventHandler DeviceDisposing;

        public event EventHandler DeviceReset;

        public event EventHandler DeviceResetting;

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        public void ApplyChanges()
        {
        }

		private void Initialize()
		{
			_graphicsDevice = new GraphicsDevice();

			if (_preferMultiSampling) 
			{
				_graphicsDevice.PreferedFilter = All.Linear;
			}
			else 
			{
				_graphicsDevice.PreferedFilter = All.Nearest;
			}
		}
		
        public void ToggleFullScreen()
        {
			IsFullScreen = !IsFullScreen;
        }
		
        public XnaTouch.Framework.Graphics.GraphicsDevice GraphicsDevice
        {
            get
            {
                return _graphicsDevice;
            }
        }

        public bool IsFullScreen
        {
            get
            {
				 return UIApplication.SharedApplication.StatusBarHidden;
            }
            set
            {
				UIApplication.SharedApplication.StatusBarHidden = value;				
            }
        }

        public bool PreferMultiSampling
        {
            get
            {
                return _preferMultiSampling;
            }
            set
            {
				_preferMultiSampling = value;
				if (_preferMultiSampling) 
				{
					_graphicsDevice.PreferedFilter = All.Linear;
				}
				else 
				{
					_graphicsDevice.PreferedFilter = All.Nearest;
				}
            }
        }

        public SurfaceFormat PreferredBackBufferFormat
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
            }
        }

        public int PreferredBackBufferHeight
        {
            get
            {
                return _preferredBackBufferHeight;
            }
            set
            {
				// throw new NotSupportedException(); 
				// Don't throw exception, just don't set it in this case.
            }
        }

        public int PreferredBackBufferWidth
        {
            get
            {
                return _preferredBackBufferWidth;
            }
            set
            {
				// throw new NotSupportedException(); 
				// Don't throw exception, just don't set it in this case as it
            }
        }

        public DepthFormat PreferredDepthStencilFormat
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
            }
        }

        public bool SynchronizeWithVerticalRetrace
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
            }
        }
    }
}

