// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Concurrent;
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
        private Rectangle _viewBounds;

        private object _eventLocker = new object();

        private InputEvents _inputEvents;
        private readonly ConcurrentQueue<char> _textQueue = new ConcurrentQueue<char>();
        private bool _isSizeChanged = false;
        private Rectangle _newViewBounds;
        private bool _isOrientationChanged = false;
        private DisplayOrientation _newOrientation;
        private bool _isFocusChanged = false;
        private CoreWindowActivationState _newActivationState;
        private bool _backPressed = false;

        #region Internal Properties

        internal Game Game { get; set; }

        public ApplicationView AppView { get { return _appView; } }

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
            _inputEvents = new InputEvents(_coreWindow, inputElement, touchQueue);

            _dinfo = DisplayInformation.GetForCurrentView();
            _appView = ApplicationView.GetForCurrentView();

            // Set a min size that is reasonable knowing someone might try
            // to use some old school resolution like 640x480.
            var minSize = new Windows.Foundation.Size(640 / _dinfo.RawPixelsPerViewPixel, 480 / _dinfo.RawPixelsPerViewPixel);
            _appView.SetPreferredMinSize(minSize);

            _orientation = ToOrientation(_dinfo.CurrentOrientation);
            _dinfo.OrientationChanged += DisplayProperties_OrientationChanged;

            _coreWindow.SizeChanged += Window_SizeChanged;

            _coreWindow.Closed += Window_Closed;
            _coreWindow.Activated += Window_FocusChanged;
            _coreWindow.CharacterReceived += Window_CharacterReceived;

            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;

            SetViewBounds(_appView.VisibleBounds.Width, _appView.VisibleBounds.Height);

            SetCursor(false);

        }

        internal void RegisterCoreWindowService()
        {
            // Register the CoreWindow with the services registry
            Game.Services.AddService(typeof(CoreWindow), _coreWindow);
        }

        private void Window_FocusChanged(CoreWindow sender, WindowActivatedEventArgs args)
        {
            lock (_eventLocker)
            {
                _isFocusChanged = true;
                _newActivationState = args.WindowActivationState;
            }
        }

        private void UpdateFocus()
        {
            lock (_eventLocker)
            {
                _isFocusChanged = false;

                if (_newActivationState == CoreWindowActivationState.Deactivated)
                    Platform.IsActive = false;
                else
                    Platform.IsActive = true;
            }
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

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            lock (_eventLocker)
            {
                _isSizeChanged = true;
                var pixelWidth  = Math.Max(1, (int)Math.Round(args.Size.Width * _dinfo.RawPixelsPerViewPixel));
                var pixelHeight = Math.Max(1, (int)Math.Round(args.Size.Height * _dinfo.RawPixelsPerViewPixel));
                _newViewBounds = new Rectangle(0, 0, pixelWidth, pixelHeight);
            }
        }

        private void UpdateSize()
        {
            lock (_eventLocker)
            {
                _isSizeChanged = false;

                var manager = Game.graphicsDeviceManager;

                // Set the new client bounds.
                _viewBounds = _newViewBounds;

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
            _textQueue.Enqueue((char)args.KeyCode);
        }

        private void UpdateTextInput()
        {
            char ch;
            while (_textQueue.TryDequeue(out ch))
                OnTextInput(_coreWindow, new TextInputEventArgs(ch));
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
                _isOrientationChanged = true;
                _newOrientation = ToOrientation(dinfo.CurrentOrientation);
            }
        }

        private void UpdateOrientation()
        {
            lock (_eventLocker)
            {
                _isOrientationChanged = false;

                // Set the new orientation.
                _orientation = _newOrientation;

                // Call the user callback.
                OnOrientationChanged();

                // If we have a valid client bounds then update the graphics device.
                if (_viewBounds.Width > 0 && _viewBounds.Height > 0)
                    Game.graphicsDeviceManager.ApplyChanges();
            }
        }

        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            // We need to manually hide the keyboard input UI when the back button is pressed
            if (KeyboardInput.IsVisible)
                KeyboardInput.Cancel(null);
            else
                _backPressed = true;

            e.Handled = true;
        }

        private void UpdateBackButton()
        {
            GamePad.Back = _backPressed;
            _backPressed = false;
        }

        protected override void SetTitle(string title)
        {
            Debug.WriteLine("WARNING: GameWindow.Title has no effect under UWP.");
        }

        internal void SetCursor(bool visible)
        {
            if ( _coreWindow == null )
                return;

            var asyncResult = _coreWindow.Dispatcher.RunIdleAsync( (e) =>
           {
               if (visible)
                   _coreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
               else
                   _coreWindow.PointerCursor = null;
           });
        }

        internal void RunLoop()
        {
            SetCursor(Game.IsMouseVisible);
            _coreWindow.Activate();

            while (true)
            {
                // Process events incoming to the window.
                _coreWindow.Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessAllIfPresent);

                Tick();

                if (IsExiting)
                    break;
            }
        }

        void ProcessWindowEvents()
        {
            // Update input
            _inputEvents.UpdateState();

            // Update TextInput
            if(!_textQueue.IsEmpty)
                UpdateTextInput();

            // Update size
            if (_isSizeChanged)
                UpdateSize();

            // Update orientation
            if (_isOrientationChanged)
                UpdateOrientation();

            // Update focus
            if (_isFocusChanged)
                UpdateFocus();

            // Update back button
            UpdateBackButton();
        }

        internal void Tick()
        {
            // Update state based on window events.
            ProcessWindowEvents();

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

