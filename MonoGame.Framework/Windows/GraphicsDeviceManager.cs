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
using Microsoft.Xna.Framework.Graphics;

#if !WINRT
using OpenTK.Graphics.OpenGL;
#endif

namespace Microsoft.Xna.Framework
{
    public class GraphicsDeviceManager : IGraphicsDeviceService, IDisposable, IGraphicsDeviceManager
    {
        private Game _game;
        private GraphicsDevice _graphicsDevice;
        private int _preferredBackBufferHeight;
        private int _preferredBackBufferWidth;
        private bool _preferMultiSampling;
        private DepthFormat _preferredDepthStencilFormat;
        private DisplayOrientation _supportedOrientations;

        public GraphicsDeviceManager(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException("Game Cannot Be Null");
            }

            _game = game;

            _supportedOrientations = DisplayOrientation.Default;
            _preferredBackBufferHeight = PresentationParameters._defaultBackBufferHeight;
            _preferredBackBufferWidth = PresentationParameters._defaultBackBufferWidth;
            _preferredDepthStencilFormat = DepthFormat.None;

            if (game.Services.GetService(typeof(IGraphicsDeviceManager)) != null)
            {
                throw new ArgumentException("Graphics Device Manager Already Present");
            }

            game.Services.AddService(typeof(IGraphicsDeviceManager), this);
            game.Services.AddService(typeof(IGraphicsDeviceService), this);

            // TODO: This should not occur here... it occurs during Game.Initialize().
#if !WINRT
            CreateDevice();
#endif
        }

        public void CreateDevice()
        {
            _graphicsDevice = new GraphicsDevice();

            Initialize();

            OnDeviceCreated(EventArgs.Empty);
        }

        public bool BeginDraw()
        {
            throw new NotImplementedException();
        }

        public void EndDraw()
        {
            throw new NotImplementedException();
        }

        #region IGraphicsDeviceService Members

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
        public event EventHandler<PreparingDeviceSettingsEventArgs> PreparingDeviceSettings;

        internal void OnDeviceDisposing(EventArgs e)
        {
            var h = DeviceDisposing;
            if (h != null)
                h(this, e);
        }

        internal void OnDeviceResetting(EventArgs e)
        {
            var h = DeviceResetting;
            if (h != null)
                h(this, e);
        }

        internal void OnDeviceReset(EventArgs e)
        {
            var h = DeviceReset;
            if (h != null)
                h(this, e);
        }

        internal void OnDeviceCreated(EventArgs e)
        {
            var h = DeviceCreated;
            if (h != null)
                h(this, e);
        }

        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            if (_graphicsDevice != null)
            {
                _graphicsDevice.Dispose();
                _graphicsDevice = null;
            }
        }

        #endregion

        public void ApplyChanges()
        {
            _game.ResizeWindow(false);
        }

        private void Initialize()
        {
            _game.Window.SetSupportedOrientations(_supportedOrientations);

            //_graphicsDevice.PresentationParameters.BackBufferFormat = _preferredBackBufferWidth;
            _graphicsDevice.PresentationParameters.BackBufferWidth = _preferredBackBufferWidth;
            _graphicsDevice.PresentationParameters.BackBufferHeight = _preferredBackBufferHeight;
            _graphicsDevice.PresentationParameters.DepthStencilFormat = _preferredDepthStencilFormat;

            _graphicsDevice.PresentationParameters.IsFullScreen = false;
            _graphicsDevice.PresentationParameters.DeviceWindowHandle = _game.Window.Handle;
            _graphicsDevice.Initialize();

            PreferMultiSampling = _preferMultiSampling;
        }

        public void ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
        }
        
        internal void ResetClientBounds()
        {
            
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
                return _graphicsDevice.PresentationParameters.IsFullScreen;
            }
            set
            {
                bool changed = value != _graphicsDevice.PresentationParameters.IsFullScreen;
                _graphicsDevice.PresentationParameters.IsFullScreen = value;

                _game.ResizeWindow(changed);
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
#if !WINRT
                if (_preferMultiSampling)
                    _graphicsDevice.PreferedFilter = All.Linear;
                else
                    _graphicsDevice.PreferedFilter = All.Nearest;
#endif
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
                return _preferredDepthStencilFormat;
            }
            set
            {
                _preferredDepthStencilFormat = value;
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
            }
        }

    }
}