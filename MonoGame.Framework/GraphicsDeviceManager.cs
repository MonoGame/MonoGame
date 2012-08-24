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
using Microsoft.Xna.Framework.Input.Touch;

#if MONOMAC
using MonoMac.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
#elif OPENGL
using OpenTK.Graphics.OpenGL;
#endif

#if ANDROID
using Android.Views;
#endif

namespace Microsoft.Xna.Framework
{
    public class GraphicsDeviceManager : IGraphicsDeviceService, IDisposable, IGraphicsDeviceManager
    {
        private Game _game;
        private GraphicsDevice _graphicsDevice;
        private int _preferredBackBufferHeight;
        private int _preferredBackBufferWidth;
        private SurfaceFormat _preferredBackBufferFormat;
        private DepthFormat _preferredDepthStencilFormat;
        private bool _preferMultiSampling;
        private DisplayOrientation _supportedOrientations;
        private bool _synchronizedWithVerticalRetrace = true;

#if !(WINDOWS || LINUX || WINRT)
        private bool _wantFullScreen = false;
#endif
        public static readonly int DefaultBackBufferHeight = 480;
        public static readonly int DefaultBackBufferWidth = 800;

        public GraphicsDeviceManager(Game game)
        {
            if (game == null)
                throw new ArgumentNullException("The game cannot be null!");

            _game = game;

            _supportedOrientations = DisplayOrientation.Default;

#if WINDOWS || MONOMAC || LINUX
            _preferredBackBufferHeight = DefaultBackBufferHeight;
            _preferredBackBufferWidth = DefaultBackBufferWidth;
#else
            // Preferred buffer width/height is used to determine default supported orientations,
            // so set the default values to match Xna behaviour of landscape only by default.
            // Note also that it's using the device window dimensions.
            _preferredBackBufferWidth = Math.Max(game.Window.ClientBounds.Height, game.Window.ClientBounds.Width);
            _preferredBackBufferHeight = Math.Min(game.Window.ClientBounds.Height, game.Window.ClientBounds.Width);
#endif

            _preferredBackBufferFormat = SurfaceFormat.Color;
            _preferredDepthStencilFormat = DepthFormat.Depth24;
            _synchronizedWithVerticalRetrace = true;

            if (game.Services.GetService(typeof(IGraphicsDeviceManager)) != null)
                throw new ArgumentException("Graphics Device Manager Already Present");

            game.Services.AddService(typeof(IGraphicsDeviceManager), this);
            game.Services.AddService(typeof(IGraphicsDeviceService), this);

#if (WINDOWS && !WINRT) || LINUX
            // TODO: This should not occur here... it occurs during Game.Initialize().
            CreateDevice();
#endif
        }

        public void CreateDevice()
        {
            _graphicsDevice = new GraphicsDevice();

            Initialize();

            // Is this really correct?
#if !WINDOWS && !WINRT && !MONOMAC && !LINUX
            ApplyChanges();
            ResetClientBounds();
#endif

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

        // FIXME: Why does the GraphicsDeviceManager not know enough about the
        //        GraphicsDevice to raise these events without help?
        internal void OnDeviceDisposing(EventArgs e)
        {
            Raise(DeviceDisposing, e);
        }

        // FIXME: Why does the GraphicsDeviceManager not know enough about the
        //        GraphicsDevice to raise these events without help?
        internal void OnDeviceResetting(EventArgs e)
        {
            Raise(DeviceResetting, e);
        }

        // FIXME: Why does the GraphicsDeviceManager not know enough about the
        //        GraphicsDevice to raise these events without help?
        internal void OnDeviceReset(EventArgs e)
        {
            Raise(DeviceReset, e);
        }

        // FIXME: Why does the GraphicsDeviceManager not know enough about the
        //        GraphicsDevice to raise these events without help?
        internal void OnDeviceCreated(EventArgs e)
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
            // Calling ApplyChanges() before CreateDevice() should have no effect
            if (_graphicsDevice == null)
                return;
#if WINRT
            // TODO:  Does this need to occur here?
            _game.Window.SetSupportedOrientations(_supportedOrientations);

            _graphicsDevice.PresentationParameters.BackBufferFormat = _preferredBackBufferFormat;
            _graphicsDevice.PresentationParameters.BackBufferWidth = _preferredBackBufferWidth;
            _graphicsDevice.PresentationParameters.BackBufferHeight = _preferredBackBufferHeight;
            _graphicsDevice.PresentationParameters.DepthStencilFormat = _preferredDepthStencilFormat;
            _graphicsDevice.PresentationParameters.IsFullScreen = false;
            
            // TODO: We probably should be resetting the whole device
            // if this changes as we are targeting a different 
            // hardware feature level.
            _graphicsDevice.GraphicsProfile = GraphicsProfile;

            // Update the 
            _graphicsDevice.CreateSizeDependentResources();

#elif WINDOWS || LINUX
            _game.ResizeWindow(false);
#elif MONOMAC
            _graphicsDevice.PresentationParameters.IsFullScreen = _wantFullScreen;

			if (_preferMultiSampling) {
				_graphicsDevice.PreferedFilter = All.Linear;
			} else {
				_graphicsDevice.PreferedFilter = All.Nearest;
			}

			_game.applyChanges(this);
#else
            _graphicsDevice.PresentationParameters.DisplayOrientation = _game.Window.CurrentOrientation;

            // Set the presentation parameters' actual buffer size to match the orientation
            bool isLandscape = (0 != (_game.Window.CurrentOrientation & (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight)));
            int w = PreferredBackBufferWidth;
            int h = PreferredBackBufferHeight;

            _graphicsDevice.PresentationParameters.BackBufferWidth = isLandscape ? Math.Max(w, h) : Math.Min(w, h);
            _graphicsDevice.PresentationParameters.BackBufferHeight = isLandscape ? Math.Min(w, h) : Math.Max(w, h);

#if !PSS && !IPHONE
            // Trigger a change in orientation in case the supported orientations have changed
            _game.Window.SetOrientation(_game.Window.CurrentOrientation, false);
#endif
#endif

            // Set the new display size on the touch panel.
            //
            // TODO: In XNA this seems to be done as part of the 
            // GraphicsDevice.DeviceReset event... we need to get 
            // those working.
            //
            TouchPanel.DisplayWidth = _graphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = _graphicsDevice.PresentationParameters.BackBufferHeight;
        }

        private void Initialize()
        {
#if WINDOWS || WINRT
            _game.Window.SetSupportedOrientations(_supportedOrientations);

            _graphicsDevice.PresentationParameters.BackBufferFormat = _preferredBackBufferFormat;
            _graphicsDevice.PresentationParameters.BackBufferWidth = _preferredBackBufferWidth;
            _graphicsDevice.PresentationParameters.BackBufferHeight = _preferredBackBufferHeight;
            _graphicsDevice.PresentationParameters.DepthStencilFormat = _preferredDepthStencilFormat;

            _graphicsDevice.PresentationParameters.IsFullScreen = false;
            _graphicsDevice.PresentationParameters.DeviceWindowHandle = _game.Window.Handle;
            _graphicsDevice.GraphicsProfile = GraphicsProfile;
            _graphicsDevice.Initialize();
#else

#if MONOMAC
            _graphicsDevice.PresentationParameters.IsFullScreen = _wantFullScreen;
#elif LINUX
            _graphicsDevice.PresentationParameters.IsFullScreen = false;
#else
            // Set "full screen"  as default
            _graphicsDevice.PresentationParameters.IsFullScreen = true;
#endif // MONOMAC

#if !PSS
            if (_preferMultiSampling)
                _graphicsDevice.PreferedFilter = All.Linear;
            else
                _graphicsDevice.PreferedFilter = All.Nearest;
#endif

            _graphicsDevice.Initialize();

#if !MONOMAC
            ApplyChanges();
#endif

#endif // WINDOWS || WINRT

            // Set the new display size on the touch panel.
            //
            // TODO: In XNA this seems to be done as part of the 
            // GraphicsDevice.DeviceReset event... we need to get 
            // those working.
            //
            TouchPanel.DisplayWidth = _graphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = _graphicsDevice.PresentationParameters.BackBufferHeight;
        }

        public void ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
        }

        public GraphicsProfile GraphicsProfile { get; set; }

        public GraphicsDevice GraphicsDevice
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
#if WINDOWS || LINUX || WINRT
                return _graphicsDevice.PresentationParameters.IsFullScreen;
#else
                if (_graphicsDevice != null)
                    return _graphicsDevice.PresentationParameters.IsFullScreen;
                else
                    return _wantFullScreen;
#endif
            }
            set
            {
#if WINDOWS || LINUX || WINRT
                _graphicsDevice.PresentationParameters.IsFullScreen = value;
#else
                _wantFullScreen = value;
                if (_graphicsDevice != null)
                {
                    _graphicsDevice.PresentationParameters.IsFullScreen = value;
#if ANDROID
                    ForceSetFullScreen();
#endif
                }
#endif
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

                // TODO: I'm pretty sure this shouldn't occur until ApplyChanges().
#if !PSS && !WINRT
                if (_graphicsDevice != null)
                {
                    if (_preferMultiSampling)
                    {
                        _graphicsDevice.PreferedFilter = All.Linear;
                    }
                    else
                    {
                        _graphicsDevice.PreferedFilter = All.Nearest;
                    }
                }
#endif
            }
        }

        public SurfaceFormat PreferredBackBufferFormat
        {
            get
            {
                return _preferredBackBufferFormat;
            }
            set
            {
                _preferredBackBufferFormat = value;
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
#if LINUX
                return _game.Platform.VSyncEnabled;
#else
                return _synchronizedWithVerticalRetrace;
#endif
            }
            set
            {
#if LINUX
                // TODO: I'm pretty sure this shouldn't occur until ApplyChanges().
                _game.Platform.VSyncEnabled = value;
#else
                _synchronizedWithVerticalRetrace = value;
#endif
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

#if !MONOMAC && !LINUX
                _game.Window.SetSupportedOrientations(_supportedOrientations);
#endif
            }
        }

        internal void ResetClientBounds()
        {
#if ANDROID
            float preferredAspectRatio = (float)GraphicsDevice.PresentationParameters.BackBufferWidth / 
                                         (float)GraphicsDevice.PresentationParameters.BackBufferHeight;
            float displayAspectRatio = (float)GraphicsDevice.DisplayMode.Width / 
                                       (float)GraphicsDevice.DisplayMode.Height;

            float adjustedAspectRatio = preferredAspectRatio;

            if ((preferredAspectRatio > 1.0f && displayAspectRatio < 1.0f) ||
                (preferredAspectRatio < 1.0f && displayAspectRatio > 1.0f))
            {
                // Invert preferred aspect ratio if it's orientation differs from the display mode orientation.
                // This occurs when user sets preferredBackBufferWidth/Height and also allows multiple supported orientations
                adjustedAspectRatio = 1.0f / preferredAspectRatio;
            }

            const float EPSILON = 0.00001f;
            var newClientBounds = new Rectangle();
            if (displayAspectRatio > (adjustedAspectRatio + EPSILON))
            {
                newClientBounds.Height = GraphicsDevice.DisplayMode.Height;
                newClientBounds.Width = (int)(newClientBounds.Height * adjustedAspectRatio);
                newClientBounds.X = (GraphicsDevice.DisplayMode.Width - newClientBounds.Width)/2;

                _game.Window.ClientBounds = newClientBounds;
            }
            else if (displayAspectRatio < (adjustedAspectRatio - EPSILON))
            {
                newClientBounds.Width = GraphicsDevice.DisplayMode.Width;
                newClientBounds.Height = (int)(newClientBounds.Width / adjustedAspectRatio);
                newClientBounds.Y = (GraphicsDevice.DisplayMode.Height - newClientBounds.Height) / 2;

                _game.Window.ClientBounds = newClientBounds;
            }
            else
            {
                // Set the ClientBounds to match the DisplayMode but swapped if necessary to match current orientation
                bool isLandscape = preferredAspectRatio > 1.0f;
                int w = GraphicsDevice.DisplayMode.Width;
                int h = GraphicsDevice.DisplayMode.Height;

                newClientBounds.Width = isLandscape ? Math.Max(w, h) : Math.Min(w, h);
                newClientBounds.Height = isLandscape ? Math.Min(w, h) : Math.Max(w, h);
                _game.Window.ClientBounds = new Rectangle(0, 0, newClientBounds.Width, newClientBounds.Height);
            }
            GraphicsDevice.Viewport = new Viewport(newClientBounds.X, newClientBounds.Y, newClientBounds.Width, newClientBounds.Height);
            Android.Util.Log.Debug("MonoGame", "GraphicsDeviceManager.ResetClientBounds: newClientBounds=" + newClientBounds.ToString());
#endif
        }

    }
}
