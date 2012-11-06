/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

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

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Windows.UI.Core;
using Windows.Graphics.Display;
using Microsoft.Xna.Framework.Graphics;
using System.Windows;


namespace MonoGame.Framework.WindowsPhone
{
    public class WindowsPhoneGameWindow : GameWindow
    {
        private DisplayOrientation _orientation;
        private Rectangle _clientBounds;

        #region Internal Properties

        static internal double Width;
        static internal double Height;

        internal bool IsExiting { get; set; }

        #endregion

        #region Public Properties

        public override IntPtr Handle { get { return IntPtr.Zero; } }

        public override string ScreenDeviceName { get { return String.Empty; } } // window.Title

        public override Rectangle ClientBounds { get { return _clientBounds; } }

        public override bool AllowUserResizing
        {
            get { return false; }
            set 
            {
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return _orientation; }
        }

        private static WindowsPhoneGamePlatform Platform { get { return Game.Instance.Platform as WindowsPhoneGamePlatform; } }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            /*
            // We don't want to trigger orientation changes 
            // when no preference is being changed.
            if (_supportedOrientations == orientations)
                return;
            
            _supportedOrientations = orientations;
            var supported = DisplayOrientations.None;

            if (orientations == DisplayOrientation.Default)
            {
                // Make the decision based on the preferred backbuffer dimensions.
                var manager = _game.graphicsDeviceManager;
                if (manager.PreferredBackBufferWidth > manager.PreferredBackBufferHeight)
                    supported = DisplayOrientations.Landscape | DisplayOrientations.LandscapeFlipped;
                else
                    supported = DisplayOrientations.Portrait | DisplayOrientations.PortraitFlipped;                    
            }
            else
            {
                if ((orientations & DisplayOrientation.LandscapeLeft) != 0)
                    supported |= DisplayOrientations.Landscape;
                if ((orientations & DisplayOrientation.LandscapeRight) != 0)
                    supported |= DisplayOrientations.LandscapeFlipped;
                if ((orientations & DisplayOrientation.Portrait) != 0)
                    supported |= DisplayOrientations.Portrait;
                if ((orientations & DisplayOrientation.PortraitUpsideDown) != 0)
                    supported |= DisplayOrientations.PortraitFlipped;
            }

            //DisplayProperties.AutoRotationPreferences = supported;
            */
        }

        #endregion

        public WindowsPhoneGameWindow()
        {
            //_orientation = ToOrientation(DisplayProperties.CurrentOrientation);
            //DisplayProperties.OrientationChanged += DisplayProperties_OrientationChanged;

            //inputElement.SizeChanged += Window_SizeChanged;

            //inputElement.GotFocus += Window_FocusChanged;
            //inputElement.LostFocus += Window_FocusChanged;

            SetClientBounds(Width, Height);
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
        }

        private void SetClientBounds(double width, double height)
        {
            var dpi = DisplayProperties.LogicalDpi;
            var pwidth = width * dpi / 96.0;
            var pheight = height * dpi / 96.0;

            _clientBounds = new Rectangle(0, 0, (int)pwidth, (int)pheight);
        }

        /*
        private void Window_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            var manager = _game.graphicsDeviceManager;

            // If we haven't calculated the back buffer scale then do it now.
            if (_backBufferScale == Vector2.Zero)
            {
                _backBufferScale = new Vector2( manager.PreferredBackBufferWidth/(float)_clientBounds.Width, 
                                                manager.PreferredBackBufferHeight/(float)_clientBounds.Height);
            }

            // Set the new client bounds.
            SetClientBounds(args.Size.Width, args.Size.Height);

            // Set the default new back buffer size and viewport, but this
            // can be overloaded by the two events below.
            
            var newWidth = (int)((_backBufferScale.X * _clientBounds.Width) + 0.5f);
            var newHeight = (int)((_backBufferScale.Y * _clientBounds.Height) + 0.5f);
            manager.PreferredBackBufferWidth = newWidth;
            manager.PreferredBackBufferHeight = newHeight;

            manager.GraphicsDevice.Viewport = new Viewport(0, 0, newWidth, newHeight);            

            // Set the new view state which will trigger the 
            // Game.ApplicationViewChanged event and signal
            // the client size changed event.
            OnClientSizeChanged();

            // If we have a valid client bounds then 
            // update the graphics device.
            if (_clientBounds.Width > 0 && _clientBounds.Height > 0)
                manager.ApplyChanges();
        }
        */

        private static DisplayOrientation ToOrientation(DisplayOrientations orientation)
        {
            var result = (DisplayOrientation)0;

            if (DisplayProperties.NativeOrientation == orientation)
                result |= DisplayOrientation.Default;

            switch (orientation)
            {
                default:
                case DisplayOrientations.None:
                    result |= DisplayOrientation.Default;
                    break;

                case DisplayOrientations.Landscape:
                    result |= DisplayOrientation.LandscapeLeft;
                    break;

                case DisplayOrientations.LandscapeFlipped:
                    result |= DisplayOrientation.LandscapeRight;
                    break;

                case DisplayOrientations.Portrait:
                    result |= DisplayOrientation.Portrait;
                    break;

                case DisplayOrientations.PortraitFlipped:
                    result |= DisplayOrientation.PortraitUpsideDown;
                    break;
            }

            return result;
        }

        /*
        private void DisplayProperties_OrientationChanged(object sender)
        {
            // Set the new orientation.
            _orientation = ToOrientation(DisplayProperties.CurrentOrientation);

            // Call the user callback.
            OnOrientationChanged();

            // If we have a valid client bounds then update the graphics device.
            if (_clientBounds.Width > 0 && _clientBounds.Height > 0)
                _game.graphicsDeviceManager.ApplyChanges();
        }
        */

        protected override void SetTitle(string title)
        {
            // NOTE: There is no concept of a window 
            // title in a Metro application.
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

