// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;
using Windows.System.Threading;

using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
    public partial class UAPGameWindow : GameWindow
    {
        private struct KeyEvent
        {
            public CoreWindow sender;
            public CharacterReceivedEventArgs args;
        }

        private DisplayOrientation _supportedOrientations;
        private DisplayOrientation _orientation;
        private CoreWindow _coreWindow;
        private DisplayInformation _dinfo;
        private ApplicationView _appView;
        private SwapChainPanel _swapChainPanel;
        private Rectangle _viewBounds;

        private Queue<KeyEvent> _windowKeyEventsToPlayback = new Queue<KeyEvent>(); // we record keys on UI thread and play them back on game thread.
        private object _gameAndUiThreadLock = new object(); // Prevents the UI thread and game thread from executing at the same time
        private bool _disableGameTicking = false; // Needed alongside locks to make app responsive

        private InputEvents _windowEvents;

        #region Internal Properties

        internal Game Game { get; set; }

        internal bool IsExiting { get; set; }

        #endregion

        #region Public Properties

        public object GetGameAndUIThreadLock() { return _gameAndUiThreadLock; }

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
            _disableGameTicking = true;
            lock (_gameAndUiThreadLock)
            {
                // We don't want to trigger orientation changes 
                // when no preference is being changed.
                if (_supportedOrientations == orientations)
                {
                    _disableGameTicking = false;
                    return;
                }

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
            _disableGameTicking = false;
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
            _disableGameTicking = true;
            lock (_gameAndUiThreadLock)
            {
                if (args.WindowActivationState == CoreWindowActivationState.Deactivated)
                    Platform.IsActive = false;
                else
                    Platform.IsActive = true;
            }
            _disableGameTicking = false;
        }

        private void Window_Closed(CoreWindow sender, CoreWindowEventArgs args)
        {
            _disableGameTicking = true;
            lock (_gameAndUiThreadLock)
            {
                Game.SuppressDraw();
                Game.Platform.Exit();
            }
            _disableGameTicking = false;
        }

        private void SetViewBounds(double width, double height)
        {
            _disableGameTicking = true;
            lock (_gameAndUiThreadLock)
            {
                var pixelWidth = Math.Max(1, (int)Math.Round(width * _dinfo.RawPixelsPerViewPixel));
                var pixelHeight = Math.Max(1, (int)Math.Round(height * _dinfo.RawPixelsPerViewPixel));
                _viewBounds = new Rectangle(0, 0, pixelWidth, pixelHeight);
            }
            _disableGameTicking = false;
        }

        private void SwapChain_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            _disableGameTicking = true;
            lock (_gameAndUiThreadLock)
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

            _disableGameTicking = false;

        }

        private void Window_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            // Cannot use 'lock(_lock)' because it blocks keyboard input because of waiting for main loops lock.       
            _disableGameTicking = true;
            lock (_windowKeyEventsToPlayback)
            {
                KeyEvent e = new KeyEvent();
                e.args = args;
                e.sender = sender;
                _windowKeyEventsToPlayback.Enqueue(e);
            }
            _disableGameTicking = false;
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
            _disableGameTicking = true;
            lock (_gameAndUiThreadLock)
            {
                if (_appView.IsFullScreenMode)
                {
                    _disableGameTicking = false;
                    return;
                }

                if (_viewBounds.Width == width &&
                    _viewBounds.Height == height)
                {
                    _disableGameTicking = false;
                    return;
                }

                var viewSize = new Windows.Foundation.Size(width / _dinfo.RawPixelsPerViewPixel, height / _dinfo.RawPixelsPerViewPixel);

                //_appView.SetPreferredMinSize(viewSize);
                if (!_appView.TryResizeView(viewSize))
                {
                    // TODO: What now?
                }
            }
            _disableGameTicking = false;
        }

        private void DisplayProperties_OrientationChanged(DisplayInformation dinfo, object sender)
        {
            _disableGameTicking = true;
            lock (_gameAndUiThreadLock)
            {
                // Set the new orientation.
                _orientation = ToOrientation(dinfo.CurrentOrientation);

                // Call the user callback.
                OnOrientationChanged();

                // If we have a valid client bounds then update the graphics device.
                if (_viewBounds.Width > 0 && _viewBounds.Height > 0)
                    Game.graphicsDeviceManager.ApplyChanges();
            }

            _disableGameTicking = false;

        }

        protected override void SetTitle(string title)
        {
            _disableGameTicking = true;
            lock (_gameAndUiThreadLock)
            {
                Debug.WriteLine("WARNING: GameWindow.Title has no effect under UWP.");
            }
            _disableGameTicking = false;
        }

        internal void SetCursor(bool visible)
        {
            _disableGameTicking = true;
            lock (_gameAndUiThreadLock)
            {
                if (_coreWindow == null)
                {
                    _disableGameTicking = false;
                    return;
                }

                if (visible)
                    _coreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
                else
                    _coreWindow.PointerCursor = null;
            }
            _disableGameTicking = false;
        }

        internal void RunLoop()
        {
            lock (_gameAndUiThreadLock)
            {
                SetCursor(Game.IsMouseVisible);
                _coreWindow.Activate();
            }

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
            // Must return before lock happens
            if (_disableGameTicking == true)
            {
                return;
            }

            // Should be outside lock
            _windowEvents.CopyKeyStateToGameThread();

            lock (_gameAndUiThreadLock)
            {
                // Playback keys that the window recieved on the game thread
                lock (_windowKeyEventsToPlayback)
                {
                    while (_windowKeyEventsToPlayback.Count > 0)
                    {
                        KeyEvent e = _windowKeyEventsToPlayback.Dequeue();
                        OnTextInput(e.sender, new TextInputEventArgs((char)e.args.KeyCode));
                    }
                }

                // Update state based on window events.  
                _windowEvents.UpdateKeyboardState();

                // Update and render the game.
                if (Game != null)
                    Game.Tick();

            }
        }

        #region Public Methods

        public void Dispose()
        {
            _disableGameTicking = true;
            lock (_gameAndUiThreadLock)
            {
                //window.Dispose();
            }
            _disableGameTicking = false;
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            _disableGameTicking = true;
            lock (_gameAndUiThreadLock)
            {

            }
            _disableGameTicking = false;
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            _disableGameTicking = true;
            lock (_gameAndUiThreadLock)
            {

            }
            _disableGameTicking = false;
        }
        #endregion

    }
}

