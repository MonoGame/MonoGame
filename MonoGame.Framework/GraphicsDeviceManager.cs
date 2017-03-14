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
        private bool _initialized = false;

        private int _preferredBackBufferWidth = DefaultBackBufferWidth;
        private int _preferredBackBufferHeight = DefaultBackBufferHeight;
        private SurfaceFormat _preferredBackBufferFormat;
        private DepthFormat _preferredDepthStencilFormat;
        private bool _preferMultiSampling;
        private DisplayOrientation _supportedOrientations;
        private bool _synchronizedWithVerticalRetrace = true;
        private bool _drawBegun;
        private bool _disposed;
        private bool _hardwareModeSwitch = true;
        private bool _wantFullScreen;
        private GraphicsProfile _graphicsProfile;
        // dirty flag for ApplyChanges
        private bool _shouldApplyChanges;

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

            // TODO We should get rid of this, the window size should be intialized to the back buffer size
            //      not the other way around. At least for desktop platforms.
#if !DESKTOPGL
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
#endif

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

        private void CreateDevice()
        {
            if (_graphicsDevice != null)
                return;

            try
            {
                var gdi = DoPreparingDeviceSettings();

                if (!_initialized)
                    Initialize(gdi.PresentationParameters);

                CreateDevice(gdi);
            }
            catch (NoSuitableGraphicsDeviceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NoSuitableGraphicsDeviceException("Failed to create graphics device!", ex);
            }
        }

        private void CreateDevice(GraphicsDeviceInformation gdi)
        {
            if (_graphicsDevice != null)
                return;

            _graphicsDevice = new GraphicsDevice(gdi);
            _shouldApplyChanges = false;

            // hook up reset events
            GraphicsDevice.DeviceReset     += (sender, args) => OnDeviceReset(args);
            GraphicsDevice.DeviceResetting += (sender, args) => OnDeviceResetting(args);

            // update the touchpanel display size when the graphicsdevice is reset
            _graphicsDevice.DeviceReset += UpdateTouchPanel;
            _graphicsDevice.PresentationChanged += OnPresentationChanged;

            OnDeviceCreated(EventArgs.Empty);
        }

        void IGraphicsDeviceManager.CreateDevice()
        {
            CreateDevice();
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
        public event EventHandler<EventArgs> Disposed;

        protected void OnDeviceDisposing(EventArgs e)
        {
            Raise(DeviceDisposing, e);
        }

        protected void OnDeviceResetting(EventArgs e)
        {
            Raise(DeviceResetting, e);
        }

        internal void OnDeviceReset(EventArgs e)
        {
            Raise(DeviceReset, e);
        }

        internal void OnDeviceCreated(EventArgs e)
        {
            Raise(DeviceCreated, e);
        }

        /// <summary>
        /// This populates a GraphicsDeviceInformation instance and invokes PreparingDeviceSettings to
        /// allow users to change the settings. Then returns that GraphicsDeviceInformation.
        /// Throws NullReferenceException if users set GraphicsDeviceInformation.PresentationParameters to null.
        /// </summary>
        private GraphicsDeviceInformation DoPreparingDeviceSettings()
        {
            var gdi = new GraphicsDeviceInformation();
            PrepareGraphicsDeviceInformation(gdi);

            if (PreparingDeviceSettings != null)
            {
                // this allows users to overwrite settings through the argument
                var args = new PreparingDeviceSettingsEventArgs(gdi);
                PreparingDeviceSettings(this, args);

                if (gdi.PresentationParameters == null || gdi.Adapter == null)
                    throw new NullReferenceException("Members should not be set to null in PreparingDeviceSettingsEventArgs");

                if (gdi.PresentationParameters.MultiSampleCount > 0)
                {
                    // Round down MultiSampleCount to the nearest power of two
                    // hack from http://stackoverflow.com/a/2681094
                    // Note: this will return an incorrect, but large value
                    // for very large numbers. That doesn't matter because
                    // the number will get clamped below anyway in this case.
                    var msc = gdi.PresentationParameters.MultiSampleCount;
                    msc = msc | (msc >> 1);
                    msc = msc | (msc >> 2);
                    msc = msc | (msc >> 4);
                    msc -= (msc >> 1);

                    if (GraphicsDevice != null)
                    {
                        // and clamp it to what the device can handle
                        if (msc > GraphicsDevice.GraphicsCapabilities.MaxMultiSampleCount)
                            msc = GraphicsDevice.GraphicsCapabilities.MaxMultiSampleCount;
                    }
                    gdi.PresentationParameters.MultiSampleCount = msc;
                }
            }

            return gdi;
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
                if (Disposed != null)
                    Disposed(this, EventArgs.Empty);
            }
        }

        #endregion

        partial void PlatformApplyChanges();

        partial void PlatformPreparePresentationParameters(PresentationParameters presentationParameters);

        private void PreparePresentationParameters(PresentationParameters presentationParameters)
        {
            presentationParameters.BackBufferFormat = _preferredBackBufferFormat;
            presentationParameters.BackBufferWidth = _preferredBackBufferWidth;
            presentationParameters.BackBufferHeight = _preferredBackBufferHeight;
            presentationParameters.DepthStencilFormat = _preferredDepthStencilFormat;
            presentationParameters.IsFullScreen = _wantFullScreen;
            presentationParameters.HardwareModeSwitch = _hardwareModeSwitch;
            presentationParameters.PresentationInterval = _synchronizedWithVerticalRetrace ? PresentInterval.One : PresentInterval.Immediate;
            presentationParameters.DisplayOrientation = _game.Window.CurrentOrientation;
            presentationParameters.DeviceWindowHandle = _game.Window.Handle;

            if (_preferMultiSampling)
            {
                // always initialize MultiSampleCount to the maximum, if users want to overwrite
                // this they have to respond to the PreparingDeviceSettingsEvent and modify
                // args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount
                presentationParameters.MultiSampleCount = GraphicsDevice != null
                    ? GraphicsDevice.GraphicsCapabilities.MaxMultiSampleCount
                    : 32;
            }
            else
            {
                presentationParameters.MultiSampleCount = 0;
            }

            PlatformPreparePresentationParameters(presentationParameters);
        }

        private void PrepareGraphicsDeviceInformation(GraphicsDeviceInformation gdi)
        {
            gdi.Adapter = GraphicsAdapter.DefaultAdapter;
            gdi.GraphicsProfile = GraphicsProfile;
            var pp = new PresentationParameters();
            PreparePresentationParameters(pp);
            gdi.PresentationParameters = pp;
        }

        /// <summary>
        /// Applies any pending property changes to the graphics device.
        /// </summary>
        public void ApplyChanges()
        {
            // If the device hasn't been created then create it now.
            if (_graphicsDevice == null)
                CreateDevice();

            if (!_shouldApplyChanges)
                return;

            _game.Window.SetSupportedOrientations(_supportedOrientations);

            // Allow for optional platform specific behavior.
            PlatformApplyChanges();

            // populates a gdi with settings in this gdm and allows users to override them with
            // PrepareDeviceSettings event this information should be applied to the GraphicsDevice
            var gdi = DoPreparingDeviceSettings();

            if (gdi.GraphicsProfile != GraphicsDevice.GraphicsProfile)
            {
                // if the GraphicsProfile changed we need to create a new GraphicsDevice
                DisposeGraphicsDevice();
                CreateDevice(gdi);
                return;
            }

            GraphicsDevice.Reset(gdi.PresentationParameters);

            _shouldApplyChanges = false;
        }

        private void DisposeGraphicsDevice()
        {
            _graphicsDevice.Dispose();

            if (DeviceDisposing != null)
                DeviceDisposing(this, EventArgs.Empty);

            _graphicsDevice = null;
        }

        partial void PlatformInitialize(PresentationParameters presentationParameters);

        private void Initialize(PresentationParameters pp)
        {
            _game.Window.SetSupportedOrientations(_supportedOrientations);

            // Allow for any per-platform changes to the presentation.
            PlatformInitialize(pp);

            _initialized = true;
        }

        private void UpdateTouchPanel(object sender, EventArgs eventArgs)
        {
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

        private void OnPresentationChanged(object sender, EventArgs args)
        {
            _game.Platform.OnPresentationChanged();
        }

        /// <summary>
        /// The profile which determines the graphics feature level.
        /// </summary>
        public GraphicsProfile GraphicsProfile
        {
            get
            {
                return _graphicsProfile;
            }
            set
            {
                _shouldApplyChanges = true;
                _graphicsProfile = value;
            }
        }

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
                _shouldApplyChanges = true;
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
                _shouldApplyChanges = true;
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
                _shouldApplyChanges = true;
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
                _shouldApplyChanges = true;
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
                _shouldApplyChanges = true;
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
                _shouldApplyChanges = true;
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
                _shouldApplyChanges = true;
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
                _shouldApplyChanges = true;
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
                _shouldApplyChanges = true;
                _supportedOrientations = value;
            }
        }
    }
}
