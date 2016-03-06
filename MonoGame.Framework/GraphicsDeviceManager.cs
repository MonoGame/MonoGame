// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
#else
using OpenGL;
#endif
#elif GLES
using OpenTK.Graphics.ES20;
#elif OPENGL
using OpenTK.Graphics.OpenGL;
#elif WINDOWS_STOREAPP || WINDOWS_UAP
using Windows.UI.Xaml.Controls;
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
        private bool _drawBegun;
        bool disposed;
        private bool _hardwareModeSwitch = true;

#if (WINDOWS || WINDOWS_UAP) && DIRECTX
        private bool _firstLaunch = true;
#endif

#if !WINRT || WINDOWS_UAP
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

#if WINDOWS || MONOMAC || DESKTOPGL
            _preferredBackBufferHeight = DefaultBackBufferHeight;
            _preferredBackBufferWidth = DefaultBackBufferWidth;
#else
            // Preferred buffer width/height is used to determine default supported orientations,
            // so set the default values to match Xna behaviour of landscape only by default.
            // Note also that it's using the device window dimensions.
            _preferredBackBufferWidth = Math.Max(_game.Window.ClientBounds.Height, _game.Window.ClientBounds.Width);
            _preferredBackBufferHeight = Math.Min(_game.Window.ClientBounds.Height, _game.Window.ClientBounds.Width);
#endif

            _preferredBackBufferFormat = SurfaceFormat.Color;
            _preferredDepthStencilFormat = DepthFormat.Depth24;
            _synchronizedWithVerticalRetrace = true;

            GraphicsProfile = GraphicsDevice.GetHighestSupportedGraphicsProfile(null);

            if (_game.Services.GetService(typeof(IGraphicsDeviceManager)) != null)
                throw new ArgumentException("Graphics Device Manager Already Present");

            _game.Services.AddService(typeof(IGraphicsDeviceManager), this);
            _game.Services.AddService(typeof(IGraphicsDeviceService), this);
        }

        ~GraphicsDeviceManager()
        {
            Dispose(false);
        }

        public void CreateDevice()
        {
            Initialize();

            OnDeviceCreated(EventArgs.Empty);
        }

        public bool BeginDraw()
        {
            if (_graphicsDevice == null)
                return false;

            _drawBegun = true;
            return true;
        }

        public void EndDraw()
        {
            if (_graphicsDevice != null && _drawBegun)
            {
                _drawBegun = false;
                _graphicsDevice.Present();
            }
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_graphicsDevice != null)
                    {
                        _graphicsDevice.Dispose();
                        _graphicsDevice = null;
                    }
                }
                disposed = true;
            }
        }

        #endregion

        public void ApplyChanges()
        {
            // Calling ApplyChanges() before CreateDevice() should have no effect
            if (_graphicsDevice == null)
                return;

#if WINDOWS_PHONE
            _graphicsDevice.GraphicsProfile = GraphicsProfile;
            // Display orientation is always portrait on WP8
            _graphicsDevice.PresentationParameters.DisplayOrientation = DisplayOrientation.Portrait;
#elif WINDOWS_STOREAPP || WINDOWS_UAP

            // TODO:  Does this need to occur here?
            _game.Window.SetSupportedOrientations(_supportedOrientations);

            _graphicsDevice.PresentationParameters.BackBufferFormat = _preferredBackBufferFormat;
            _graphicsDevice.PresentationParameters.BackBufferWidth = _preferredBackBufferWidth;
            _graphicsDevice.PresentationParameters.BackBufferHeight = _preferredBackBufferHeight;
            _graphicsDevice.PresentationParameters.DepthStencilFormat = _preferredDepthStencilFormat;
            
            // TODO: We probably should be resetting the whole device
            // if this changes as we are targeting a different 
            // hardware feature level.
            _graphicsDevice.GraphicsProfile = GraphicsProfile;

#if WINDOWS_UAP
			_graphicsDevice.PresentationParameters.DeviceWindowHandle = IntPtr.Zero;
			_graphicsDevice.PresentationParameters.SwapChainPanel = this.SwapChainPanel;
            _graphicsDevice.PresentationParameters.IsFullScreen = _wantFullScreen;
#else
            _graphicsDevice.PresentationParameters.IsFullScreen = false;

			// The graphics device can use a XAML panel or a window
			// to created the default swapchain target.
            if (this.SwapChainBackgroundPanel != null)
            {
                _graphicsDevice.PresentationParameters.DeviceWindowHandle = IntPtr.Zero;
                _graphicsDevice.PresentationParameters.SwapChainBackgroundPanel = this.SwapChainBackgroundPanel;
            }
            else
            {
                _graphicsDevice.PresentationParameters.DeviceWindowHandle = _game.Window.Handle;
                _graphicsDevice.PresentationParameters.SwapChainBackgroundPanel = null;
            }
#endif
			// Update the back buffer.
			_graphicsDevice.CreateSizeDependentResources();
            _graphicsDevice.ApplyRenderTargets(null);

#if WINDOWS_UAP
            ((UAPGameWindow)_game.Window).SetClientSize(_preferredBackBufferWidth, _preferredBackBufferHeight);
#endif

#elif WINDOWS && DIRECTX

            _graphicsDevice.PresentationParameters.BackBufferFormat = _preferredBackBufferFormat;
            _graphicsDevice.PresentationParameters.BackBufferWidth = _preferredBackBufferWidth;
            _graphicsDevice.PresentationParameters.BackBufferHeight = _preferredBackBufferHeight;
            _graphicsDevice.PresentationParameters.DepthStencilFormat = _preferredDepthStencilFormat;
            _graphicsDevice.PresentationParameters.PresentationInterval = _synchronizedWithVerticalRetrace ? PresentInterval.Default : PresentInterval.Immediate;
            _graphicsDevice.PresentationParameters.IsFullScreen = _wantFullScreen;

            // TODO: We probably should be resetting the whole 
            // device if this changes as we are targeting a different
            // hardware feature level.
            _graphicsDevice.GraphicsProfile = GraphicsProfile;

            _graphicsDevice.PresentationParameters.DeviceWindowHandle = _game.Window.Handle;

            // Update the back buffer.
            _graphicsDevice.CreateSizeDependentResources();
            _graphicsDevice.ApplyRenderTargets(null);

            ((MonoGame.Framework.WinFormsGamePlatform)_game.Platform).ResetWindowBounds();

#elif DESKTOPGL
            ((OpenTKGamePlatform)_game.Platform).ResetWindowBounds();

            //Set the swap interval based on if vsync is desired or not.
            //See GetSwapInterval for more details
            int swapInterval;
            if (_synchronizedWithVerticalRetrace)
                swapInterval = _graphicsDevice.PresentationParameters.PresentationInterval.GetSwapInterval();
            else
                swapInterval = 0;
            _graphicsDevice.Context.SwapInterval = swapInterval;
#elif MONOMAC
            _graphicsDevice.PresentationParameters.IsFullScreen = _wantFullScreen;

            // TODO: Implement multisampling (aka anti-alising) for all platforms!

			_game.applyChanges(this);
#else

#if ANDROID
            // Trigger a change in orientation in case the supported orientations have changed
            ((AndroidGameWindow)_game.Window).SetOrientation(_game.Window.CurrentOrientation, false);
#endif
            // Ensure the presentation parameter orientation and buffer size matches the window
            _graphicsDevice.PresentationParameters.DisplayOrientation = _game.Window.CurrentOrientation;

            // Set the presentation parameters' actual buffer size to match the orientation
            bool isLandscape = (0 != (_game.Window.CurrentOrientation & (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight)));
            int w = PreferredBackBufferWidth;
            int h = PreferredBackBufferHeight;

            _graphicsDevice.PresentationParameters.BackBufferWidth = isLandscape ? Math.Max(w, h) : Math.Min(w, h);
            _graphicsDevice.PresentationParameters.BackBufferHeight = isLandscape ? Math.Min(w, h) : Math.Max(w, h);

            ResetClientBounds();
#endif

            // Set the new display size on the touch panel.
            //
            // TODO: In XNA this seems to be done as part of the 
            // GraphicsDevice.DeviceReset event... we need to get 
            // those working.
            //
            TouchPanel.DisplayWidth = _graphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = _graphicsDevice.PresentationParameters.BackBufferHeight;

#if (WINDOWS || WINDOWS_UAP) && DIRECTX

            if (!_firstLaunch)
            {
                if (IsFullScreen)
                {
                    _game.Platform.EnterFullScreen();
                }
                else
                {
                   _game.Platform.ExitFullScreen();
                }
            }
            _firstLaunch = false;
#endif
        }

        private void Initialize()
        {
            var presentationParameters = new PresentationParameters();
            presentationParameters.DepthStencilFormat = DepthFormat.Depth24;

#if (WINDOWS || WINRT) && !DESKTOPGL
            _game.Window.SetSupportedOrientations(_supportedOrientations);

            presentationParameters.BackBufferFormat = _preferredBackBufferFormat;
            presentationParameters.BackBufferWidth = _preferredBackBufferWidth;
            presentationParameters.BackBufferHeight = _preferredBackBufferHeight;
            presentationParameters.DepthStencilFormat = _preferredDepthStencilFormat;
            presentationParameters.IsFullScreen = false;

#if WINDOWS_PHONE
			// Nothing to do!
#elif WINDOWS_UAP
			presentationParameters.DeviceWindowHandle = IntPtr.Zero;
			presentationParameters.SwapChainPanel = this.SwapChainPanel;
#elif WINDOWS_STORE
			// The graphics device can use a XAML panel or a window
			// to created the default swapchain target.
            if (this.SwapChainBackgroundPanel != null)
            {
                presentationParameters.DeviceWindowHandle = IntPtr.Zero;
                presentationParameters.SwapChainBackgroundPanel = this.SwapChainBackgroundPanel;
            }
            else
            {
                presentationParameters.DeviceWindowHandle = _game.Window.Handle;
                presentationParameters.SwapChainBackgroundPanel = null;
            }
#else
            presentationParameters.DeviceWindowHandle = _game.Window.Handle;
#endif

#else

#if MONOMAC || DESKTOPGL
            presentationParameters.IsFullScreen = _wantFullScreen;
#elif WEB
            presentationParameters.IsFullScreen = false;
#else
            // Set "full screen"  as default
            presentationParameters.IsFullScreen = true;
#endif // MONOMAC

#endif // WINDOWS || WINRT

            // TODO: Implement multisampling (aka anti-alising) for all platforms!
            if (PreparingDeviceSettings != null)
            {
                GraphicsDeviceInformation gdi = new GraphicsDeviceInformation();
                gdi.GraphicsProfile = GraphicsProfile; // Microsoft defaults this to Reach.
                gdi.Adapter = GraphicsAdapter.DefaultAdapter;
                gdi.PresentationParameters = presentationParameters;
                PreparingDeviceSettingsEventArgs pe = new PreparingDeviceSettingsEventArgs(gdi);
                PreparingDeviceSettings(this, pe);
                presentationParameters = pe.GraphicsDeviceInformation.PresentationParameters;
                GraphicsProfile = pe.GraphicsDeviceInformation.GraphicsProfile;
            }

            // Needs to be before ApplyChanges()
            _graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile, presentationParameters);

#if !MONOMAC
            ApplyChanges();
#endif

            // Set the new display size on the touch panel.
            //
            // TODO: In XNA this seems to be done as part of the 
            // GraphicsDevice.DeviceReset event... we need to get 
            // those working.
            //
            TouchPanel.DisplayWidth = _graphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = _graphicsDevice.PresentationParameters.BackBufferHeight;
            TouchPanel.DisplayOrientation = _graphicsDevice.PresentationParameters.DisplayOrientation;
        }

        public void ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;

#if ((WINDOWS || WINDOWS_UAP) && DIRECTX) || DESKTOPGL
            ApplyChanges();
#endif
        }

#if WINDOWS_STOREAPP
        [CLSCompliant(false)]
        public SwapChainBackgroundPanel SwapChainBackgroundPanel { get; set; }
#endif

#if WINDOWS_UAP
        [CLSCompliant(false)]
        public SwapChainPanel SwapChainPanel { get; set; }
#endif

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
#if WINDOWS_UAP
                return _wantFullScreen;
#elif WINRT
                return true;
#else
                if (_graphicsDevice != null)
                    return _graphicsDevice.PresentationParameters.IsFullScreen;
                return _wantFullScreen;
#endif
            }
            set
            {
#if WINDOWS_UAP
                _wantFullScreen = value;
#elif WINRT
                // Just ignore this as it is not relevant on Windows 8
#elif WINDOWS && DIRECTX
                _wantFullScreen = value;
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
			{
				Game.Activity.Window.ClearFlags(Android.Views.WindowManagerFlags.ForceNotFullscreen);
                Game.Activity.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
			}
            else
                Game.Activity.Window.SetFlags(WindowManagerFlags.ForceNotFullscreen, WindowManagerFlags.ForceNotFullscreen);
        }
#endif

        /// <summary>
        /// Gets or sets the boolean which defines how window switches from windowed to fullscreen state.
        /// "Hard" mode(true) is slow to switch, but more effecient for performance, while "soft" mode(false) is vice versa.
        /// The default value is <c>true</c>. Can only be changed before graphics device is created (in game's constructor).
        /// </summary>
        public bool HardwareModeSwitch
        {
            get { return _hardwareModeSwitch;}
            set
            {
                if (_graphicsDevice == null) _hardwareModeSwitch = value;
                else throw new InvalidOperationException("This property can only be changed before graphics device is created(in game constructor).");
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
                return _synchronizedWithVerticalRetrace;
            }
            set
            {
                _synchronizedWithVerticalRetrace = value;
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
                if (_game.Window != null)
                    _game.Window.SetSupportedOrientations(_supportedOrientations);
            }
        }

        /// <summary>
        /// This method is used by MonoGame Android to adjust the game's drawn to area to fill
        /// as much of the screen as possible whilst retaining the aspect ratio inferred from
        /// aspectRatio = (PreferredBackBufferWidth / PreferredBackBufferHeight)
        ///
        /// NOTE: this is a hack that should be removed if proper back buffer to screen scaling
        /// is implemented. To disable it's effect, in the game's constructor use:
        ///
        ///     graphics.IsFullScreen = true;
        ///     graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        ///     graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        ///
        /// </summary>
        internal void ResetClientBounds()
        {
#if ANDROID
            float preferredAspectRatio = (float)PreferredBackBufferWidth /
                                         (float)PreferredBackBufferHeight;
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
                // Fill the entire height and reduce the width to keep aspect ratio
                newClientBounds.Height = _graphicsDevice.DisplayMode.Height;
                newClientBounds.Width = (int)(newClientBounds.Height * adjustedAspectRatio);
                newClientBounds.X = (_graphicsDevice.DisplayMode.Width - newClientBounds.Width) / 2;
            }
            else if (displayAspectRatio < (adjustedAspectRatio - EPSILON))
            {
                // Fill the entire width and reduce the height to keep aspect ratio
                newClientBounds.Width = _graphicsDevice.DisplayMode.Width;
                newClientBounds.Height = (int)(newClientBounds.Width / adjustedAspectRatio);
                newClientBounds.Y = (_graphicsDevice.DisplayMode.Height - newClientBounds.Height) / 2;
            }
            else
            {
                // Set the ClientBounds to match the DisplayMode
                newClientBounds.Width = GraphicsDevice.DisplayMode.Width;
                newClientBounds.Height = GraphicsDevice.DisplayMode.Height;
            }

            // Ensure buffer size is reported correctly
            _graphicsDevice.PresentationParameters.BackBufferWidth = newClientBounds.Width;
            _graphicsDevice.PresentationParameters.BackBufferHeight = newClientBounds.Height;

            // Set the veiwport so the (potentially) resized client bounds are drawn in the middle of the screen
            _graphicsDevice.Viewport = new Viewport(newClientBounds.X, -newClientBounds.Y, newClientBounds.Width, newClientBounds.Height);

            ((AndroidGameWindow)_game.Window).ChangeClientBounds(newClientBounds);

            // Touch panel needs latest buffer size for scaling
            TouchPanel.DisplayWidth = newClientBounds.Width;
            TouchPanel.DisplayHeight = newClientBounds.Height;

            Android.Util.Log.Debug("MonoGame", "GraphicsDeviceManager.ResetClientBounds: newClientBounds=" + newClientBounds.ToString());
#endif
        }

    }
}
