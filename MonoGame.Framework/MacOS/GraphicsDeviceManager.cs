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

using MonoMac.OpenGL;

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
		private bool wantFullScreen = false;
		private bool synchronizedWithVerticalRefresh = true;

		public GraphicsDeviceManager (Game game)
		{
			if (game == null) {
				throw new ArgumentNullException ("Game Cannot Be Null");
			}

			_game = game;

			_supportedOrientations = DisplayOrientation.Default;
			_preferredBackBufferHeight = PresentationParameters._defaultBackBufferHeight;
			_preferredBackBufferWidth = PresentationParameters._defaultBackBufferWidth;			

			if (game.Services.GetService (typeof(IGraphicsDeviceManager)) != null) {
				throw new ArgumentException ("Graphics Device Manager Already Present");
			}

			game.Services.AddService (typeof(IGraphicsDeviceManager), this);
			game.Services.AddService (typeof(IGraphicsDeviceService), this);	

		}

		public void CreateDevice ()
		{
			_graphicsDevice = new GraphicsDevice ();

			Initialize();
			
			OnDeviceCreated(EventArgs.Empty);
		}

		public bool BeginDraw ()
		{
			throw new NotImplementedException ();
		}

		public void EndDraw ()
		{
			throw new NotImplementedException ();
		}

		#region IGraphicsDeviceService Members

		public event EventHandler<EventArgs> DeviceCreated;
		public event EventHandler<EventArgs> DeviceDisposing;
		public event EventHandler<EventArgs> DeviceReset;
		public event EventHandler<EventArgs> DeviceResetting;
		public event EventHandler<PreparingDeviceSettingsEventArgs> PreparingDeviceSettings;

        // FIXME: Why does the GraphicsDeviceManager not know enough about the
        //        GraphicsDevice to raise these events without help?
		internal void OnDeviceDisposing (EventArgs e)
		{
            Raise(DeviceDisposing, e);
		}

        // FIXME: Why does the GraphicsDeviceManager not know enough about the
        //        GraphicsDevice to raise these events without help?
		internal void OnDeviceResetting (EventArgs e)
		{
            Raise(DeviceResetting, e);
		}

        // FIXME: Why does the GraphicsDeviceManager not know enough about the
        //        GraphicsDevice to raise these events without help?
		internal void OnDeviceReset (EventArgs e)
		{
            Raise(DeviceReset, e);
		}

        // FIXME: Why does the GraphicsDeviceManager not know enough about the
        //        GraphicsDevice to raise these events without help?
		internal void OnDeviceCreated (EventArgs e)
		{
            Raise(DeviceCreated, e);
		}


        private void Raise<TEventArgs>(EventHandler<TEventArgs> handler, TEventArgs e)
            where TEventArgs : EventArgs
        {
            if (handler != null)
                handler(this, e);
        }
		
		#endregion
		

		#region IDisposable Members

		public void Dispose ()
		{
		}

		#endregion

		public void ApplyChanges ()
		{
            _graphicsDevice.PresentationParameters.IsFullScreen = wantFullScreen;

			if (_preferMultiSampling) {
				_graphicsDevice.PreferedFilter = All.Linear;
			} else {
				_graphicsDevice.PreferedFilter = All.Nearest;
			}

			_game.applyChanges(this);
		}

		private void Initialize ()
		{
            _graphicsDevice.PresentationParameters.IsFullScreen = wantFullScreen;

			if (_preferMultiSampling) {
				_graphicsDevice.PreferedFilter = All.Linear;
			} else {
				_graphicsDevice.PreferedFilter = All.Nearest;
			}

            _graphicsDevice.Initialize();
		}

		public void ToggleFullScreen ()
		{
			IsFullScreen = !IsFullScreen;
		}

		public Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice {
			get {
				return _graphicsDevice;
			}
		}

		public bool IsFullScreen {
			get {
				if (_graphicsDevice != null)
					return _graphicsDevice.PresentationParameters.IsFullScreen;
				else
					return wantFullScreen;
			}
			set {
				wantFullScreen = value;
                if (_graphicsDevice != null)
                    ApplyChanges();
			}
		}

		public bool PreferMultiSampling {
			get {
				return _preferMultiSampling;
			}
			set {
				_preferMultiSampling = value;
				if (_graphicsDevice != null) {				
					if (_preferMultiSampling) {
						_graphicsDevice.PreferedFilter = All.Linear;
					} else {
						_graphicsDevice.PreferedFilter = All.Nearest;
					}
				}
			}
		}

		public SurfaceFormat PreferredBackBufferFormat {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		public int PreferredBackBufferHeight {
			get {
				return _preferredBackBufferHeight;
			}
			set {
				_preferredBackBufferHeight = value;
			}
		}

		public int PreferredBackBufferWidth {
			get {
				return _preferredBackBufferWidth;
			}
			set {
				_preferredBackBufferWidth = value;				
			}
		}

		public DepthFormat PreferredDepthStencilFormat {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		public bool SynchronizeWithVerticalRetrace {
			get {
				return synchronizedWithVerticalRefresh;
			}
			set {
				synchronizedWithVerticalRefresh = value;
			}
		}

		public DisplayOrientation SupportedOrientations { 
			get {
				return _supportedOrientations;
			}
			set {
				_supportedOrientations = value;
			}
		}
		
		
        internal void ResetClientBounds()
        {
            // do nothing for now
        }

	}
}
