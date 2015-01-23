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
using System.Windows;
using System.Runtime.InteropServices;

using Windows.UI.Core;
using Windows.Graphics.Display;

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace MonoGame.Framework.WindowsPhone
{
    public class WindowsPhoneGameWindow : GameWindow
    {
        private DisplayOrientation _supportedOrientations;
        private DisplayOrientation _orientation;
        private Rectangle _clientBounds;
        private Rectangle _actualBounds;
        private Game _game;

        #region Internal Properties

        static internal double Width;
        static internal double Height;
        static internal PhoneApplicationPage Page;

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
            _supportedOrientations = orientations == DisplayOrientation.Default ? DisplayOrientation.Portrait : orientations;
        }

        #endregion

        public WindowsPhoneGameWindow(Game game)
        {
            _game = game;

            _orientation = ToOrientation(Page.Orientation);
            Page.OrientationChanged += Page_OrientationChanged;

            Page.Loaded += delegate
            {
                var frame = (PhoneApplicationFrame)Application.Current.RootVisual;

                frame.Obscured += (sender, e) => 
                { 
                    if (Game.Instance != null &&
                        (!e.IsLocked || PhoneApplicationService.Current.ApplicationIdleDetectionMode != IdleDetectionMode.Enabled)) 
                        Platform.IsActive = false;
                };
                frame.Unobscured += (sender, e) => { if (Game.Instance != null) Platform.IsActive = true; };
            };

            Page.SizeChanged += Page_SizeChanged; 

            PhoneApplicationService.Current.Activated += (sender, e) => { if (Game.Instance != null) Platform.IsActive = true; };
            PhoneApplicationService.Current.Launching += (sender, e) => { if (Game.Instance != null) Platform.IsActive = true; };
            PhoneApplicationService.Current.Deactivated += (sender, e) => { if (Game.Instance != null) Platform.IsActive = false; };
            PhoneApplicationService.Current.Closing += (sender, e) => { if (Game.Instance != null) Platform.IsActive = false; };

            SetClientBounds(Width, Height);
        }

        private void SetClientBounds(double width, double height)
        {
            _clientBounds = new Rectangle(0, 0, (int)width, (int)height);
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            SetClientBounds(clientWidth, clientHeight);
            _game.graphicsDeviceManager.ApplyChanges();

            this.UpdateBounds();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _actualBounds = new Rectangle(0,
                0,
                (int)(e.NewSize.Width * Application.Current.Host.Content.ScaleFactor / 100 + 0.5f),
                (int)(e.NewSize.Height * Application.Current.Host.Content.ScaleFactor / 100 + 0.5f));

            this.UpdateBounds();
        }

        private void UpdateBounds()
        {
            var manager = _game.graphicsDeviceManager;

            if (manager.GraphicsDevice == null)
                return;

            // Determine correct content size based on orientation difference
            int height;
            int width;
            if (_game.Window.CurrentOrientation == manager.GraphicsDevice.PresentationParameters.DisplayOrientation)
            {
                height = _actualBounds.Height;
                width = _actualBounds.Width;
            }
            else
            {
                height = _actualBounds.Width;
                width = _actualBounds.Height;
            }

            SetClientBounds(width, height);

            manager.PreferredBackBufferWidth = width;
            manager.PreferredBackBufferHeight = height;

            manager.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);

            // Set the new view state which will trigger the 
            // Game.ApplicationViewChanged event and signal
            // the client size changed event.
            OnClientSizeChanged();

            // We don't call ApplyChanges because we don't want the TouchPanel resolution to change
            //manager.ApplyChanges();
        }

        private static DisplayOrientation ToOrientation(PageOrientation orientations)
        {
            var result = DisplayOrientation.Default;
            if ((orientations & (PageOrientation)0x0004) != 0)
                result |= DisplayOrientation.Portrait;
            if ((orientations & (PageOrientation)0x0008) != 0)
                result |= DisplayOrientation.PortraitDown;
            if ((orientations & (PageOrientation)0x0010) != 0)
                result |= DisplayOrientation.LandscapeLeft;
            if ((orientations & (PageOrientation)0x0020) != 0)
                result |= DisplayOrientation.LandscapeRight;

            return result;
        }

        private void Page_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            DisplayOrientation orientation = ToOrientation(e.Orientation);
            // Don't change our orientation if it isn't supported
            if ((orientation & _supportedOrientations) == 0)
                return;

            // Set the new orientation.
            _orientation = orientation;

            // Call the user callback.
            OnOrientationChanged();

            // If we have a valid client bounds then update the graphics device.
            //if (_clientBounds.Width > 0 && _clientBounds.Height > 0)
            _game.graphicsDeviceManager.ApplyChanges();
        }

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

        #endregion
    }
}

