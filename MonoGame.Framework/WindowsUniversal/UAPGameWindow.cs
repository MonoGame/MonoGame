// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.Graphics.Display;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Xna.Framework
{
    partial class UAPGameWindow : GameWindow
    {
        private DisplayOrientation _supportedOrientations;
        private DisplayOrientation _orientation;
        private CoreWindow _coreWindow;
        private DisplayInformation _dinfo;
        private ApplicationView _appView;
        private SwapChainPanel _swapChainPanel;
        private Rectangle _viewBounds;

        private object _eventLocker = new object();

        private InputEvents _windowEvents;

        #region Internal Properties

        internal Game Game { get; set; }

        internal bool IsExiting { get; set; }

        #endregion

        #region Public Properties

        public override IntPtr Handle { get { return Marshal.GetIUnknownForObject(_coreWindow); } }

        public override string ScreenDeviceName { get { return String.Empty; } } // window.Title

        public override Rectangle ClientBounds { get { return _viewBounds; } }

        public override bool AllowUserResizing
        {
            get { return false; }
            set 
            {
                // You cannot resize a Metro window!
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return _orientation; }
        }

        private UAPGamePlatform Platform { get { return Game.Instance.Platform as UAPGamePlatform; } }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // We don't want to trigger orientation changes 
            // when no preference is being changed.
            if (_supportedOrientations == orientations)
                return;
            
            _supportedOrientations = orientations;
            
            DisplayOrientations supported;
            if (orientations == DisplayOrientation.Default)
            {
                // Make the decision based on the preferred backbuffer dimensions.
                var manager = Game.graphicsDeviceManager;
                if (manager.PreferredBackBufferWidth > manager.PreferredBackBufferHeight)
                    supported = FromOrientation(DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight);
                else
                    supported = FromOrientation(DisplayOrientation.Portrait | DisplayOrientation.PortraitDown);
            }
            else
                supported = FromOrientation(orientations);

            DisplayInformation.AutoRotationPreferences = supported;
        }

        #endregion

        static public UAPGameWindow Instance { get; private set; }

        static UAPGameWindow()
        {
            Instance = new UAPGameWindow();
        }

        public void Initialize(CoreWindow coreWindow, UIElement inputElement, TouchQueue touchQueue)
        {
            _coreWindow = coreWindow;
            _windowEvents = new InputEvents(_coreWindow, inputElement, touchQueue);

			_dinfo = DisplayInformation.GetForCurrentView();
            _appView = ApplicationView.GetForCurrentView();

            // Set a min size that is reasonable knowing someone might try
            // to use some old school resolution like 640x480.
            var minSize = new Windows.Foundation.Size(640 / _dinfo.RawPixelsPerViewPixel, 480 / _dinfo.RawPixelsPerViewPixel);
            _appView.SetPreferredMinSize(minSize);

            _orientation = ToOrientation(_dinfo.CurrentOrientation);
            _dinfo.OrientationChanged += DisplayProperties_OrientationChanged;
            _swapChainPanel = inputElement as SwapChainPanel;

            _swapChainPanel.SizeChanged += SwapChain_SizeChanged;

            _coreWindow.Closed += Window_Closed;
            _coreWindow.Activated += Window_FocusChanged;
			_coreWindow.CharacterReceived += Window_CharacterReceived;

            SetViewBounds(_appView.VisibleBounds.Width, _appView.VisibleBounds.Height);

            SetCursor(false);
        }

        private void Window_FocusChanged(CoreWindow sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == CoreWindowActivationState.Deactivated)
                Platform.IsActive = false;
            else
                Platform.IsActive = true;
        }

        private void Window_Closed(CoreWindow sender, CoreWindowEventArgs args)
        {
            Game.SuppressDraw();
            Game.Platform.Exit();
        }

        private void SetViewBounds(double width, double height)
        {
            var pixelWidth = Math.Max(1, (int)Math.Round(width * _dinfo.RawPixelsPerViewPixel));
            var pixelHeight = Math.Max(1, (int)Math.Round(height * _dinfo.RawPixelsPerViewPixel));
            _viewBounds = new Rectangle(0, 0, pixelWidth, pixelHeight);
        }

        private void SwapChain_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            lock (_eventLocker)
            {
                var manager = Game.graphicsDeviceManager;

                // Set the new client bounds.
                SetViewBounds(args.NewSize.Width, args.NewSize.Height);

                // Set the default new back buffer size and viewport, but this
                // can be overloaded by the two events below.

                manager.IsFullScreen = _appView.IsFullScreenMode;
                manager.PreferredBackBufferWidth = _viewBounds.Width;
                manager.PreferredBackBufferHeight = _viewBounds.Height;
                manager.ApplyChanges();

                // Set the new view state which will trigger the 
                // Game.ApplicationViewChanged event and signal
                // the client size changed event.
                OnClientSizeChanged();
            }
        }

		private void Window_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
		{
			OnTextInput(sender, new TextInputEventArgs((char)args.KeyCode));
		}

        private static DisplayOrientation ToOrientation(DisplayOrientations orientations)
        {
            var result = DisplayOrientation.Default;
            if ((orientations & DisplayOrientations.Landscape) != 0)
                result |= DisplayOrientation.LandscapeLeft;
            if ((orientations & DisplayOrientations.LandscapeFlipped) != 0)
                result |= DisplayOrientation.LandscapeRight;
            if ((orientations & DisplayOrientations.Portrait) != 0)
                result |= DisplayOrientation.Portrait;
            if ((orientations & DisplayOrientations.PortraitFlipped) != 0)
                result |= DisplayOrientation.PortraitDown;

            return result;
        }

        private static DisplayOrientations FromOrientation(DisplayOrientation orientation)
        {
            var result = DisplayOrientations.None;
            if ((orientation & DisplayOrientation.LandscapeLeft) != 0)
                result |= DisplayOrientations.Landscape;
            if ((orientation & DisplayOrientation.LandscapeRight) != 0)
                result |= DisplayOrientations.LandscapeFlipped;
            if ((orientation & DisplayOrientation.Portrait) != 0)
                result |= DisplayOrientations.Portrait;
            if ((orientation & DisplayOrientation.PortraitDown) != 0)
                result |= DisplayOrientations.PortraitFlipped;

            return result;
        }

        internal void SetClientSize(int width, int height)
        {
            if (_appView.IsFullScreenMode)
                return;

            if (_viewBounds.Width == width &&
                _viewBounds.Height == height)
                return;

            var viewSize = new Windows.Foundation.Size(width / _dinfo.RawPixelsPerViewPixel, height / _dinfo.RawPixelsPerViewPixel);

            //_appView.SetPreferredMinSize(viewSize);
            if (!_appView.TryResizeView(viewSize))
            {
                // TODO: What now?
            }
        }

        private void DisplayProperties_OrientationChanged(DisplayInformation dinfo, object sender)
        {
            lock(_eventLocker)
            {
                // Set the new orientation.
                _orientation = ToOrientation(dinfo.CurrentOrientation);

                // Call the user callback.
                OnOrientationChanged();

                // If we have a valid client bounds then update the graphics device.
                if (_viewBounds.Width > 0 && _viewBounds.Height > 0)
                    Game.graphicsDeviceManager.ApplyChanges();
            }
        }

        protected override void SetTitle(string title)
        {
            Debug.WriteLine("WARNING: GameWindow.Title has no effect under UWP.");
        }

        internal void SetCursor(bool visible)
        {
            if ( _coreWindow == null )
                return;

            if (visible)
                _coreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
            else
                _coreWindow.PointerCursor = null;
        }

        internal void RunLoop()
        {
            SetCursor(Game.IsMouseVisible);
            _coreWindow.Activate();

            while (true)
            {
                if (Platform.IsActive)
                {
                    // Process events incoming to the window.
                    _coreWindow.Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessAllIfPresent);

                    Tick();
                }
                else
                {
                    _coreWindow.Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessOneAndAllPending);
                }
                if (IsExiting)
                    break;
            }
        }

        internal void Tick()
        {
            // Update state based on window events.
            _windowEvents.UpdateState();

            // Update and render the game.
            if (Game != null)
                Game.Tick();
        }

        #region Public Methods

        public void Dispose()
        {
            //window.Dispose();
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {

        }

        #endregion
    }
}

