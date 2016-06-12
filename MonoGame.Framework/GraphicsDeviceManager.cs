// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Used to initialize and control the presentation of the graphics device.
    /// </summary>
    public partial class GraphicsDeviceManager : IGraphicsDeviceService, IDisposable, IGraphicsDeviceManager
    {
        private readonly Game _game;
        private GraphicsDevice _graphicsDevice;
        private int _preferredBackBufferHeight;
        private int _preferredBackBufferWidth;
        private SurfaceFormat _preferredBackBufferFormat;
        private DepthFormat _preferredDepthStencilFormat;
        private bool _preferMultiSampling;
        private DisplayOrientation _supportedOrientations;
        private bool _synchronizedWithVerticalRetrace = true;
        private bool _drawBegun;
        private bool _disposed;
        private bool _hardwareModeSwitch = true;
        private bool _wantFullScreen;

        /// <summary>
        /// The default back buffer width.
        /// </summary>
        public static readonly int DefaultBackBufferWidth = 800;

        /// <summary>
        /// The default back buffer height.
        /// </summary>
        public static readonly int DefaultBackBufferHeight = 480;

        /// <summary>
        /// Optional override for platform specific defaults.
        /// </summary>
        partial void PlatformConstruct();

        /// <summary>
        /// Associates this graphics device manager to a game instances.
        /// </summary>
        /// <param name="game">The game instance to attach.</param>
        public GraphicsDeviceManager(Game game)
        {
            if (game == null)
                throw new ArgumentNullException("game", "Game cannot be null.");

            _game = game;

            _supportedOrientations = DisplayOrientation.Default;
            _preferredBackBufferFormat = SurfaceFormat.Color;
            _preferredDepthStencilFormat = DepthFormat.Depth24;
            _synchronizedWithVerticalRetrace = true;

            // Assume the window client size as the default back 
            // buffer resolution in the landscape orientation.
            var clientBounds = _game.Window.ClientBounds;
            if (clientBounds.Width >= clientBounds.Height)
            {
                _preferredBackBufferWidth = clientBounds.Width;
                _preferredBackBufferHeight = clientBounds.Height;
            }
            else
            {
                _preferredBackBufferWidth = clientBounds.Height;
                _preferredBackBufferHeight = clientBounds.Width;
            }

            // Default to windowed mode... this is ignored on platforms that don't support it.
            _wantFullScreen = false;

            // XNA would read this from the manifest, but it would always default
            // to reach unless changed.  So lets mimic that without the manifest bit.
            GraphicsProfile = GraphicsProfile.Reach;

            // Let the plaform optionally overload construction defaults.
            PlatformConstruct();

            if (_game.Services.GetService(typeof(IGraphicsDeviceManager)) != null)
                throw new ArgumentException("A graphics device manager is already registered.  The graphics device manager cannot be changed once it is set.");

            _game.Services.AddService(typeof(IGraphicsDeviceManager), this);
            _game.Services.AddService(typeof(IGraphicsDeviceService), this);
        }

        ~GraphicsDeviceManager()
        {
            Dispose(false);
        }

        public void CreateDevice()
        {
            if (_graphicsDevice != null)
                return;

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
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_graphicsDevice != null)
                    {
                        _graphicsDevice.Dispose();
                        _graphicsDevice = null;
                    }
                }
                _disposed = true;
            }
        }

        #endregion

        partial void PlatformApplyChanges();

        private void PreparePresentationParameters(PresentationParameters presentationParameters)
        {
            presentationParameters.BackBufferFormat = _preferredBackBufferFormat;
            presentationParameters.BackBufferWidth = _preferredBackBufferWidth;
            presentationParameters.BackBufferHeight = _preferredBackBufferHeight;
            presentationParameters.DepthStencilFormat = _preferredDepthStencilFormat;
            presentationParameters.IsFullScreen = _wantFullScreen;
            presentationParameters.PresentationInterval = _synchronizedWithVerticalRetrace ? PresentInterval.One : PresentInterval.Immediate;
            presentationParameters.DisplayOrientation = _game.Window.CurrentOrientation;
            presentationParameters.DeviceWindowHandle = _game.Window.Handle;

            // TODO: This isn't correct... we need to query the hardware
            // to see what the max quality level supported is for the current
            // device and back buffer format.
            presentationParameters.MultiSampleCount = _preferMultiSampling ? 4 : 0;
        }

        /// <summary>
        /// Applies any pending property changes to the graphics device.
        /// </summary>
        public void ApplyChanges()
        {
            // If the device hasn't been created then create it now.
            if (_graphicsDevice == null)
                CreateDevice();

            _game.Window.SetSupportedOrientations(_supportedOrientations);

            PreparePresentationParameters(_graphicsDevice.PresentationParameters);

            // TODO: Should this trigger some sort of device reset?
            _graphicsDevice.GraphicsProfile = GraphicsProfile;

            // Allow for optional platform specific behavior.
            PlatformApplyChanges();

            // Update the graphics device and then the platform window.
            _graphicsDevice.OnPresentationChanged();
            _game.Platform.OnPresentationChanged();

            // Set the new display size on the touch panel.
            //
            // TODO: In XNA this seems to be done as part of the 
            // GraphicsDevice.DeviceReset event... we need to get 
            // those working.
            //
            TouchPanel.DisplayWidth = _graphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = _graphicsDevice.PresentationParameters.BackBufferHeight;
        }

        partial void PlatformInitialize(PresentationParameters presentationParameters);

        private void Initialize()
        {
            _game.Window.SetSupportedOrientations(_supportedOrientations);

            var presentationParameters = new PresentationParameters();
            PreparePresentationParameters(presentationParameters);

            // Allow for any per-platform changes to the presentation.
            PlatformInitialize(presentationParameters);

            // TODO: Implement multisampling (aka anti-alising) for all platforms!
            if (PreparingDeviceSettings != null)
            {
                var gdi = new GraphicsDeviceInformation();
                gdi.GraphicsProfile = GraphicsProfile; // Microsoft defaults this to Reach.
                gdi.Adapter = GraphicsAdapter.DefaultAdapter;
                gdi.PresentationParameters = presentationParameters;
                var pe = new PreparingDeviceSettingsEventArgs(gdi);
                PreparingDeviceSettings(this, pe);
                presentationParameters = pe.GraphicsDeviceInformation.PresentationParameters;
                GraphicsProfile = pe.GraphicsDeviceInformation.GraphicsProfile;
            }

            // Create and initialize the graphics device.
            _graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile, presentationParameters);

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

        /// <summary>
        /// Toggles between windowed and fullscreen modes.
        /// </summary>
        /// <remarks>
        /// Note that on platforms that do not support windowed modes this has no affect.
        /// </remarks>
        public void ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
            ApplyChanges();
        }

        /// <summary>
        /// The profile which determines the graphics feature level.
        /// </summary>
        public GraphicsProfile GraphicsProfile { get; set; }

        /// <summary>
        /// Returns the graphics device for this manager.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return _graphicsDevice;
            }
        }

        /// <summary>
        /// Indicates the desire to switch into fullscreen mode.
        /// </summary>
        /// <remarks>
        /// When called at startup this will automatically set fullscreen mode during initialization.  If
        /// set after startup you must call ApplyChanges() for the fullscreen mode to be changed.
        /// Note that for some platforms that do not support windowed modes this property has no affect.
        /// </remarks>
        public bool IsFullScreen
        {
            get { return _wantFullScreen; }
            set
            {
                _wantFullScreen = value;
            }
        }

        /// <summary>
        /// Gets or sets the boolean which defines how window switches from windowed to fullscreen state.
        /// "Hard" mode(true) is slow to switch, but more effecient for performance, while "soft" mode(false) is vice versa.
        /// The default value is <c>true</c>.
        /// </summary>
        public bool HardwareModeSwitch
        {
            get { return _hardwareModeSwitch;}
            set
            {
                _hardwareModeSwitch = value;
            }
        }

        /// <summary>
        /// Indicates the desire for a multisampled back buffer.
        /// </summary>
        /// <remarks>
        /// When called at startup this will automatically set the MSAA mode during initialization.  If
        /// set after startup you must call ApplyChanges() for the MSAA mode to be changed.
        /// </remarks>
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

        /// <summary>
        /// Indicates the desired back buffer color format.
        /// </summary>
        /// <remarks>
        /// When called at startup this will automatically set the format during initialization.  If
        /// set after startup you must call ApplyChanges() for the format to be changed.
        /// </remarks>
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

        /// <summary>
        /// Indicates the desired back buffer height in pixels.
        /// </summary>
        /// <remarks>
        /// When called at startup this will automatically set the height during initialization.  If
        /// set after startup you must call ApplyChanges() for the height to be changed.
        /// </remarks>
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

        /// <summary>
        /// Indicates the desired back buffer width in pixels.
        /// </summary>
        /// <remarks>
        /// When called at startup this will automatically set the width during initialization.  If
        /// set after startup you must call ApplyChanges() for the width to be changed.
        /// </remarks>
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

        /// <summary>
        /// Indicates the desired depth-stencil buffer format.
        /// </summary>
        /// <remarks>
        /// The depth-stencil buffer format defines the scene depth precision and stencil bits available for effects during rendering.
        /// When called at startup this will automatically set the format during initialization.  If
        /// set after startup you must call ApplyChanges() for the format to be changed.
        /// </remarks>
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

        /// <summary>
        /// Indicates the desire for vsync when presenting the back buffer.
        /// </summary>
        /// <remarks>
        /// Vsync limits the frame rate of the game to the monitor referesh rate to prevent screen tearing.
        /// When called at startup this will automatically set the vsync mode during initialization.  If
        /// set after startup you must call ApplyChanges() for the vsync mode to be changed.
        /// </remarks>
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

        /// <summary>
        /// Indicates the desired allowable display orientations when the device is rotated.
        /// </summary>
        /// <remarks>
        /// This property only applies to mobile platforms with automatic display rotation.
        /// When called at startup this will automatically apply the supported orientations during initialization.  If
        /// set after startup you must call ApplyChanges() for the supported orientations to be changed.
        /// </remarks>
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
