// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.Graphics.Display;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
    partial class MetroGameWindow : GameWindow
    {
        private DisplayOrientation _supportedOrientations;
        private DisplayOrientation _orientation;
        private CoreWindow _coreWindow;
        private Rectangle _clientBounds;
#if !WINDOWS_PHONE81
        private ApplicationViewState _currentViewState;
#endif
        private InputEvents _windowEvents;


        private Vector2 _backBufferScale;

        #region Internal Properties

        internal Game Game { get; set; }

        internal bool IsExiting { get; set; }

        #endregion

        #region Public Properties

        public override IntPtr Handle { get { return Marshal.GetIUnknownForObject(_coreWindow); } }

        public override string ScreenDeviceName { get { return String.Empty; } } // window.Title

        public override Rectangle ClientBounds { get { return _clientBounds; } }

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

        private MetroGamePlatform Platform { get { return Game.Instance.Platform as MetroGamePlatform; } }

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

            DisplayProperties.AutoRotationPreferences = supported;
        }

        #endregion

        static public MetroGameWindow Instance { get; private set; }

        static MetroGameWindow()
        {
            Instance = new MetroGameWindow();
        }

        public void Initialize(CoreWindow coreWindow, UIElement inputElement, TouchQueue touchQueue)
        {
            _coreWindow = coreWindow;
            _windowEvents = new InputEvents(_coreWindow, inputElement, touchQueue);

            _orientation = ToOrientation(DisplayProperties.CurrentOrientation);
            DisplayProperties.OrientationChanged += DisplayProperties_OrientationChanged;

            _coreWindow.SizeChanged += Window_SizeChanged;
            _coreWindow.Closed += Window_Closed;

            _coreWindow.Activated += Window_FocusChanged;
#if !WINDOWS_PHONE81
            _currentViewState = ApplicationView.Value;
#endif
            var bounds = _coreWindow.Bounds;
            SetClientBounds(bounds.Width, bounds.Height);

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

        private void SetClientBounds(double width, double height)
        {
            var dpi = DisplayProperties.LogicalDpi;
            var pwidth = (int)Math.Round(width * dpi / 96.0);
            var pheight = (int)Math.Round(height * dpi / 96.0);

            _clientBounds = new Rectangle(0, 0, pwidth, pheight);
        }

        private void Window_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            var manager = Game.graphicsDeviceManager;

            // If we haven't calculated the back buffer scale then do it now.
            if (_backBufferScale == Vector2.Zero)
            {
                // Make sure the scale is calculated in terms of the same orientation as the preferred back buffer
                float clientWidth;
                float clientHeight;
                if (manager.PreferredBackBufferWidth > manager.PreferredBackBufferHeight)
                {
                    clientWidth = (float)Math.Max(_clientBounds.Width, _clientBounds.Height);
                    clientHeight = (float)Math.Min(_clientBounds.Width, _clientBounds.Height);
                }
                else
                {
                    clientWidth = (float)Math.Min(_clientBounds.Width, _clientBounds.Height);
                    clientHeight = (float)Math.Max(_clientBounds.Width, _clientBounds.Height);
                }
                _backBufferScale = new Vector2( manager.PreferredBackBufferWidth / clientWidth, 
                                                manager.PreferredBackBufferHeight / clientHeight);
            }

            // Set the new client bounds.
            SetClientBounds(args.Size.Width, args.Size.Height);

            // Set the default new back buffer size and viewport, but this
            // can be overloaded by the two events below.
            
            var newWidth = (int)((_backBufferScale.X * _clientBounds.Width) + 0.5f);
            var newHeight = (int)((_backBufferScale.Y * _clientBounds.Height) + 0.5f);
            manager.PreferredBackBufferWidth = newWidth;
            manager.PreferredBackBufferHeight = newHeight;
            if(manager.GraphicsDevice!=null)
            manager.GraphicsDevice.Viewport = new Viewport(0, 0, newWidth, newHeight);            

            // If we have a valid client bounds then 
            // update the graphics device.
            if (_clientBounds.Width > 0 && _clientBounds.Height > 0)
                manager.ApplyChanges();

            // Set the new view state which will trigger the 
            // Game.ApplicationViewChanged event and signal
            // the client size changed event.
#if !WINDOWS_PHONE81
            Platform.ViewState = ApplicationView.Value;
#endif
            OnClientSizeChanged();
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

        private void DisplayProperties_OrientationChanged(object sender)
        {
            // Set the new orientation.
            _orientation = ToOrientation(DisplayProperties.CurrentOrientation);

            // Call the user callback.
            OnOrientationChanged();

            // If we have a valid client bounds then update the graphics device.
            if (_clientBounds.Width > 0 && _clientBounds.Height > 0)
                Game.graphicsDeviceManager.ApplyChanges();
        }

        protected override void SetTitle(string title)
        {
            // NOTE: There is no concept of a window 
            // title in a Metro application.
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
#if !WINDOWS_PHONE81
    [CLSCompliant(false)]
    public class ViewStateChangedEventArgs : EventArgs
    {
        public readonly ApplicationViewState ViewState;

        public ViewStateChangedEventArgs(ApplicationViewState newViewstate)
        {
            ViewState = newViewstate;
        }
    }
#endif
}

