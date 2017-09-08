// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

#if WINDOWS_STOREAPP || WINDOWS_UAP
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

            // XNA would read this from the manifest, but it would always default
            // to Reach unless changed.  So lets mimic that without the manifest bit.
            GraphicsProfile = GraphicsProfile.Reach;

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
            EventHelpers.Raise(this, DeviceDisposing, e);
        }

        // FIXME: Why does the GraphicsDeviceManager not know enough about the
        //        GraphicsDevice to raise these events without help?
        internal void OnDeviceResetting(EventArgs e)
        {
            EventHelpers.Raise(this, DeviceResetting, e);
        }

        // FIXME: Why does the GraphicsDeviceManager not know enough about the
        //        GraphicsDevice to raise these events without help?
        internal void OnDeviceReset(EventArgs e)
        {
            EventHelpers.Raise(this, DeviceReset, e);
        }

        // FIXME: Why does the GraphicsDeviceManager not know enough about the
        //        GraphicsDevice to raise these events without help?
        internal void OnDeviceCreated(EventArgs e)
        {
            EventHelpers.Raise(this, DeviceCreated, e);
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

        /// <summary>
        /// Finds the best device configuration that is compatible with the current device preferences.
        /// </summary>
        /// <param name="anySuitableDevice">If true, the best device configuration can be selected from any
        /// available adaptor. If false, only the current adaptor is used.</param>
        /// <returns>The best device configuration that could be found.</returns>
        protected virtual GraphicsDeviceInformation FindBestDevice(bool anySuitableDevice)
        {
            // Create a list of available devices
            var devices = new List<GraphicsDeviceInformation>();
            if (anySuitableDevice)
            {
                foreach (var adapter in GraphicsAdapter.Adapters)
                {
                    if (adapter.IsProfileSupported(GraphicsProfile))
                        AddModes(adapter, devices);
                }
            }
            else
            {
                var adapter = GraphicsAdapter.DefaultAdapter;
                if (adapter.IsProfileSupported(GraphicsProfile))
                    AddModes(adapter, devices);
            }

            // Rank them to get the most preferred device first
            RankDevices(devices);

            // No devices left in the list?
            if (devices.Count == 0)
            {
                throw new NoSuitableGraphicsDeviceException(FrameworkResources.CouldNotFindCompatibleGraphicsDevice);
            }

            // The first device in the list is the most suitable
            return devices[0];
        }

        // Add all modes supported by an adapter to the list
        void AddModes(GraphicsAdapter adapter, List<GraphicsDeviceInformation> devices)
        {
            int multiSampleCount = 0;
            if (_preferMultiSampling)
            {
                // Always initialize MultiSampleCount to the maximum. If users want to overwrite
                // this they have to respond to the PreparingDeviceSettingsEvent and modify
                // args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount
                multiSampleCount = GraphicsDevice != null ? GraphicsDevice.GraphicsCapabilities.MaxMultiSampleCount : 32;
            }

            if (IsFullScreen)
            {
                // Fullscreen mode adds every supported display mode by each adaptor
                foreach (var mode in adapter.SupportedDisplayModes)
                {
                    var gdi = new GraphicsDeviceInformation()
                    {
                        Adapter = adapter,
                        GraphicsProfile = GraphicsProfile,
                        PresentationParameters = new PresentationParameters()
                        {
                            BackBufferFormat = mode.Format,
                            BackBufferHeight = mode.Height,
                            BackBufferWidth = mode.Width,
                            DepthStencilFormat = PreferredDepthStencilFormat,
                            IsFullScreen = IsFullScreen,
                            HardwareModeSwitch = _hardwareModeSwitch,
                            PresentationInterval = _synchronizedWithVerticalRetrace ? PresentInterval.One : PresentInterval.Immediate,
                            DisplayOrientation = _game.Window.CurrentOrientation,
                            DeviceWindowHandle = _game.Window.Handle,
                            MultiSampleCount = multiSampleCount
                        }
                    };
                    devices.Add(gdi);
                }
            }
            else
            {
                // Windowed mode only adds an entry for the requested window size
                var gdi = new GraphicsDeviceInformation()
                {
                    Adapter = adapter,
                    GraphicsProfile = GraphicsProfile,
                    PresentationParameters = new PresentationParameters()
                    {
                        BackBufferFormat = PreferredBackBufferFormat,
                        BackBufferHeight = PreferredBackBufferHeight,
                        BackBufferWidth = PreferredBackBufferWidth,
                        DepthStencilFormat = PreferredDepthStencilFormat,
                        IsFullScreen = IsFullScreen,
                        HardwareModeSwitch = _hardwareModeSwitch,
                        PresentationInterval = _synchronizedWithVerticalRetrace ? PresentInterval.One : PresentInterval.Immediate,
                        DisplayOrientation = _game.Window.CurrentOrientation,
                        DeviceWindowHandle = _game.Window.Handle,
                        MultiSampleCount = multiSampleCount
                    }
                };
                devices.Add(gdi);
            }
        }

        int CompareGraphicsDeviceInformation(GraphicsDeviceInformation left, GraphicsDeviceInformation right)
        {
            var leftPP = left.PresentationParameters;
            var rightPP = right.PresentationParameters;
            var leftAdapter = left.Adapter;
            var rightAdapter = right.Adapter;

            // Prefer a higher graphics profile
            if (left.GraphicsProfile != right.GraphicsProfile)
                return left.GraphicsProfile > right.GraphicsProfile ? -1 : 1;

            // Prefer windowed/fullscreen based on IsFullScreen
            if (leftPP.IsFullScreen != rightPP.IsFullScreen)
                return IsFullScreen == leftPP.IsFullScreen ? -1 : 1;

            // BackBufferFormat
            if (leftPP.BackBufferFormat != rightPP.BackBufferFormat)
            {
                var preferredSize = PreferredBackBufferFormat.GetSize();
                var leftRank = leftPP.BackBufferFormat.GetSize() == preferredSize ? 0 : 1;
                var rightRank = rightPP.BackBufferFormat.GetSize() == preferredSize ? 0 : 1;
                if (leftRank != rightRank)
                    return leftRank < rightRank ? -1 : 1;
            }

            // MultiSampleCount
            if (leftPP.MultiSampleCount != rightPP.MultiSampleCount)
                return leftPP.MultiSampleCount > rightPP.MultiSampleCount ? -1 : 1;

            // Resolution
            int leftWidthDiff = Math.Abs(leftPP.BackBufferWidth - PreferredBackBufferWidth);
            int leftHeightDiff = Math.Abs(leftPP.BackBufferHeight - PreferredBackBufferHeight);
            int rightWidthDiff = Math.Abs(rightPP.BackBufferWidth - PreferredBackBufferWidth);
            int rightHeightDiff = Math.Abs(rightPP.BackBufferHeight - PreferredBackBufferHeight);
            if (leftHeightDiff != rightHeightDiff)
                return leftHeightDiff < rightHeightDiff ? -1 : 1;
            if (leftWidthDiff != rightWidthDiff)
                return leftWidthDiff < rightWidthDiff ? -1 : 1;

            // Aspect ratio
            var targetAspectRatio = (float)PreferredBackBufferWidth / (float)PreferredBackBufferHeight;
            var leftAspectRatio = (float)leftPP.BackBufferWidth / (float)leftPP.BackBufferHeight;
            var rightAspectRatio = (float)rightPP.BackBufferWidth / (float)rightPP.BackBufferHeight;
            if (Math.Abs(leftAspectRatio - rightAspectRatio) > 0.1f)
                return Math.Abs(leftAspectRatio - targetAspectRatio) < Math.Abs(rightAspectRatio - targetAspectRatio) ? -1 : 1;

            // Default adapter first
            if (leftAdapter.IsDefaultAdapter != rightAdapter.IsDefaultAdapter)
                return leftAdapter.IsDefaultAdapter ? -1 : 1;

            return 0;
        }

        /// <summary>
        /// Orders the supplied devices based on the current preferences.
        /// </summary>
        /// <param name="foundDevices">The list of devices to rank.</param>
        /// <remarks>
        /// The list of devices is sorted so that devices earlier in the list are preferred over devices
        /// later in the list. Devices may be removed from the list if they do not satisfy the criteria.
        /// </remarks>
        protected virtual void RankDevices(List<GraphicsDeviceInformation> foundDevices)
        {
            // Filter out any unsuitable graphics profiles. Hopefully there shouldn't be many to remove
            for (int i = foundDevices.Count - 1; i >= 0; --i)
            {
                if (foundDevices[i].GraphicsProfile > GraphicsProfile)
                    foundDevices.RemoveAt(i);
            }

            foundDevices.Sort(CompareGraphicsDeviceInformation);
        }

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
            var displayIndex = Sdl.Window.GetDisplayIndex (SdlGameWindow.Instance.Handle);
            var displayName = Sdl.Display.GetDisplayName (displayIndex);

            _graphicsDevice.PresentationParameters.BackBufferFormat = _preferredBackBufferFormat;
            _graphicsDevice.PresentationParameters.BackBufferWidth = _preferredBackBufferWidth;
            _graphicsDevice.PresentationParameters.BackBufferHeight = _preferredBackBufferHeight;
            _graphicsDevice.PresentationParameters.DepthStencilFormat = _preferredDepthStencilFormat;
            _graphicsDevice.PresentationParameters.PresentationInterval = _synchronizedWithVerticalRetrace ? PresentInterval.Default : PresentInterval.Immediate;
            _graphicsDevice.PresentationParameters.IsFullScreen = _wantFullScreen;

            //Set the swap interval based on if vsync is desired or not.
            //See GetSwapInterval for more details
            int swapInterval;
            if (_synchronizedWithVerticalRetrace)
                swapInterval = _graphicsDevice.PresentationParameters.PresentationInterval.GetSwapInterval();
            else
                swapInterval = 0;
            _graphicsDevice.Context.SwapInterval = swapInterval;

            _graphicsDevice.ApplyRenderTargets (null);

            _game.Platform.BeginScreenDeviceChange (GraphicsDevice.PresentationParameters.IsFullScreen);
            _game.Platform.EndScreenDeviceChange (displayName, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);

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
#elif WINDOWS_STOREAPP
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
            var preparingDeviceSettingsHandler = PreparingDeviceSettings;

            if (preparingDeviceSettingsHandler != null)
            {
                var gdi = FindBestDevice(true);
                PreparingDeviceSettingsEventArgs pe = new PreparingDeviceSettingsEventArgs(gdi);
                preparingDeviceSettingsHandler(this, pe);
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
        /// The default value is <c>true</c>.
        /// </summary>
        public bool HardwareModeSwitch
        {
            get { return _hardwareModeSwitch; }
            set
            {
                _hardwareModeSwitch = value;
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
