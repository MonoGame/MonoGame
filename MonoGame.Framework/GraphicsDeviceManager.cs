#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

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

#if OPENGL
using OpenTK.Graphics.ES20;
#endif

#if ANDROID
using Android.Views;
#endif

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public class GraphicsDeviceManager : IGraphicsDeviceService, IDisposable, IGraphicsDeviceManager
    {
		private Game _game;
		private GraphicsDevice _graphicsDevice;
		private int _preferredBackBufferHeight;
		private int _preferredBackBufferWidth;
		private bool _preferMultiSampling;
		private DisplayOrientation _supportedOrientations;
		private bool wantFullScreen = true;

        public GraphicsDeviceManager(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException("Game Cannot Be Null");
            }
            
			_game = game;

            // Preferred buffer width/height is used to determine default supported orientations,
            // so set the default values to match Xna behaviour of landscape only by default.
            // Note also that it's using the device window dimensions.
            _preferredBackBufferWidth = Math.Max(game.Window.ClientBounds.Height, game.Window.ClientBounds.Width);
            _preferredBackBufferHeight = Math.Min(game.Window.ClientBounds.Height, game.Window.ClientBounds.Width);
            _supportedOrientations = DisplayOrientation.Default;
			
            if (game.Services.GetService(typeof(IGraphicsDeviceManager)) != null)
            {
                throw new ArgumentException("Graphics Device Manager Already Present");
            }
			
            game.Services.AddService(typeof(IGraphicsDeviceManager), this);
            game.Services.AddService(typeof(IGraphicsDeviceService), this);
        }
		
		public void CreateDevice()
		{
			_graphicsDevice = new GraphicsDevice();

			Initialize();
			ApplyChanges();
            ResetClientBounds();

			OnDeviceCreated(EventArgs.Empty);
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

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;		
		public event EventHandler<PreparingDeviceSettingsEventArgs> PreparingDeviceSettings;
		
		internal void OnDeviceDisposing (EventArgs e)
		{
			var h = DeviceDisposing;
			if (h != null)
				h (this, e);
		}
		
		internal void OnDeviceCreated (EventArgs e)
		{
			var h = DeviceCreated;
			if (h != null)
				h (this, e);
		}
		
		internal void OnDeviceResetting (EventArgs e)
		{
			var h = DeviceResetting;
			if (h != null)
				h (this, e);
		}

		internal void OnDeviceReset (EventArgs e)
		{
			var h = DeviceReset;
			if (h != null)
				h (this, e);
		}		

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        public void ApplyChanges()
        {
            if (GraphicsDevice != null)
            {
                GraphicsDevice.PresentationParameters.DisplayOrientation = _game.Window.CurrentOrientation;

                // Set the presentation parameters' actual buffer size to match the orientation
                bool isLandscape = (0 != (_game.Window.CurrentOrientation & (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight)));
                int w = PreferredBackBufferWidth;
                int h = PreferredBackBufferHeight;

                GraphicsDevice.PresentationParameters.BackBufferWidth = isLandscape ? Math.Max(w, h) : Math.Min(w, h);
                GraphicsDevice.PresentationParameters.BackBufferHeight = isLandscape ? Math.Min(w, h) : Math.Max(w, h);

                // Trigger a change in orientation in case the supported orientations have changed
                _game.Window.SetOrientation(_game.Window.CurrentOrientation, false);
            }
        }

        private void Initialize()
        {
            // Set "full screen"  as default
            _graphicsDevice.PresentationParameters.IsFullScreen = true;

            ApplyChanges();

            _graphicsDevice.Initialize();

#if !PSS
			if (_preferMultiSampling)
			{
				_graphicsDevice.PreferedFilter = All.Linear;
			}
			else 
			{
				_graphicsDevice.PreferedFilter = All.Nearest;
			}
#endif
		}
		
        public void ToggleFullScreen()
        {
			IsFullScreen = !IsFullScreen;
        }
		
        public Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice
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
				if (_graphicsDevice != null)
					return _graphicsDevice.PresentationParameters.IsFullScreen;
				else
					return wantFullScreen;				 
            }
            set
            {
				wantFullScreen = value;
				if (_graphicsDevice != null) 
				{
					_graphicsDevice.PresentationParameters.IsFullScreen = value;
#if ANDROID
                    ForceSetFullScreen();
#endif
				}
            }
        }

#if ANDROID
        internal void ForceSetFullScreen()
        {
            if (IsFullScreen)
                Game.Activity.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            else
                Game.Activity.Window.SetFlags(WindowManagerFlags.ForceNotFullscreen, WindowManagerFlags.ForceNotFullscreen);
        }
#endif

        public bool PreferMultiSampling
        {
            get
            {
                return _preferMultiSampling;
            }
            set
            {
				_preferMultiSampling = value;
				if ( _graphicsDevice != null )
				{
#if !PSS
					if (_preferMultiSampling) 
					{
						_graphicsDevice.PreferedFilter = All.Linear;
					}
					else 
					{
						_graphicsDevice.PreferedFilter = All.Nearest;
					}
#endif
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
				_preferredBackBufferHeight = value;
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
				_preferredBackBufferWidth = value;				
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
		
		public DisplayOrientation SupportedOrientations 
		{ 
			get
			{
				return _supportedOrientations;
			}
			set
			{
				_supportedOrientations = value;
				_game.Window.SetSupportedOrientations(_supportedOrientations);
			}
		}

        internal void ResetClientBounds()
        {
#if ANDROID
            float preferredAspectRatio = (float)GraphicsDevice.PresentationParameters.BackBufferWidth / 
                                         (float)GraphicsDevice.PresentationParameters.BackBufferHeight;
            float displayAspectRatio = (float)GraphicsDevice.DisplayMode.Width / 
                                       (float)GraphicsDevice.DisplayMode.Height;

            if ((preferredAspectRatio > 1.0f && displayAspectRatio < 1.0f) ||
                (preferredAspectRatio < 1.0f && displayAspectRatio > 1.0f))
            {
                // Invert preferred aspect ratio if it's orientation differs from the display mode orientation.
                // This occurs when user sets preferredBackBufferWidth/Height and also allows multiple supported orientations
                preferredAspectRatio = 1.0f / preferredAspectRatio;
            }

            const float EPSILON = 0.00001f;
            var newClientBounds = new Rectangle();
            if (displayAspectRatio > (preferredAspectRatio + EPSILON))
            {
                newClientBounds.Height = GraphicsDevice.DisplayMode.Height;
                newClientBounds.Width = (int) (newClientBounds.Height * preferredAspectRatio);
                newClientBounds.X = (GraphicsDevice.DisplayMode.Width - newClientBounds.Width)/2;

                _game.Window.ClientBounds = newClientBounds;
            }
            else if (displayAspectRatio < (preferredAspectRatio - EPSILON))
            {
                newClientBounds.Width = GraphicsDevice.DisplayMode.Width;
                newClientBounds.Height = (int)(newClientBounds.Width / preferredAspectRatio);
                newClientBounds.Y = (GraphicsDevice.DisplayMode.Height - newClientBounds.Height) / 2;

                _game.Window.ClientBounds = newClientBounds;
            }
            else
            {
                newClientBounds.Width = GraphicsDevice.DisplayMode.Width;
                newClientBounds.Height = GraphicsDevice.DisplayMode.Height;
                _game.Window.ClientBounds = new Rectangle(0, 0, GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
            }
            GraphicsDevice.Viewport = new Viewport(newClientBounds.X, newClientBounds.Y, newClientBounds.Width, newClientBounds.Height);
            Android.Util.Log.Debug("MonoGame", "GraphicsDeviceManager.ResetClientBounds: newClientBounds=" + newClientBounds.ToString());
#endif
        }

    }
}

