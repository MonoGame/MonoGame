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
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Phone.Controls;


namespace MonoGame.Framework.WindowsPhone
{
    public class WindowsPhoneGameWindow : GameWindow
    {
        private static DisplayOrientation _orientation;
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

        public static PhoneApplicationPage AppPage;

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // We don't want to trigger orientation changes 
            // when no preference is being changed.
            //if (_supportedOrientations == orientations)
            //    return;
            
            //_supportedOrientations = orientations;
            var supported = DisplayOrientations.None;

            if (orientations == DisplayOrientation.Default)
            {
                // Make the decision based on the preferred backbuffer dimensions.
                var manager = Platform.Game.graphicsDeviceManager;
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
            
        }

        #endregion

        public WindowsPhoneGameWindow()
        {
            //_orientation = ToOrientation(DisplayProperties.CurrentOrientation);
            AppPage.OrientationChanged += DisplayProperties_OrientationChanged;

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

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            SetClientBounds(clientWidth, clientHeight);
            TouchPanel.DisplayHeight = ClientBounds.Height;
            TouchPanel.DisplayWidth = ClientBounds.Width;
        }

        private DisplayOrientation ToOrientation(PageOrientation orientation)
        {
            var result = (DisplayOrientation)0;

            switch (orientation)
            {
                case PageOrientation.None:
                case PageOrientation.Portrait:
                case PageOrientation.PortraitUp:
                    result |= DisplayOrientation.Portrait;
                    break;

                case PageOrientation.Landscape:
                case PageOrientation.LandscapeLeft:
                    result |= DisplayOrientation.LandscapeLeft;
                    break;

                case PageOrientation.LandscapeRight:
                    result |= DisplayOrientation.LandscapeRight;
                    break;

                default:
                    result |= DisplayOrientation.Default;
                    break;
            }

            return result;
        }

        public void DisplayProperties_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            var newOrientation = ToOrientation(e.Orientation);
            
            var largestBound = Math.Max(_clientBounds.Width, _clientBounds.Height);
            var smallest = Math.Min(_clientBounds.Width, _clientBounds.Height);

            if (newOrientation == DisplayOrientation.LandscapeLeft ||
                newOrientation == DisplayOrientation.LandscapeRight)
            {
                _clientBounds.Width = largestBound;
                _clientBounds.Height = smallest;
            }
            else
            {
                _clientBounds.Width = smallest;
                _clientBounds.Height = largestBound;
            }
            
            // Set the new orientation.   
            _orientation = newOrientation;

            // Call the user callback.
            OnOrientationChanged();

            var device = Platform.Game.graphicsDeviceManager.GraphicsDevice;
            var vp = device.Viewport.Bounds;
            Matrix rotMatrix;
            // Calculate new rotation matrices.
            switch (_orientation)
            {
                case DisplayOrientation.Portrait:
                    device.RotationMatrix2D = device.RotationMatrix3D = Matrix.Identity;
                    break;

                case DisplayOrientation.LandscapeLeft:
                    device.RotationMatrix2D = Matrix.Multiply(
                        Matrix.CreateRotationZ(MathHelper.PiOver2),
                        Matrix.CreateTranslation(vp.Height, 0, 0));
                    device.RotationMatrix3D = new Matrix(
                        0.0f, -1.0f, 0.0f, 0.0f,
                        1.0f, 0.0f, 0.0f, 0.0f,
                        0.0f, 0.0f, 1.0f, 0.0f,
                        0.0f, 0.0f, 0.0f, 1.0f);
                    break;

                case DisplayOrientation.LandscapeRight:
                    device.RotationMatrix2D = Matrix.Multiply(
                        Matrix.CreateRotationZ(3 * MathHelper.PiOver2),
                        Matrix.CreateTranslation(0, vp.Width, 0));
                    device.RotationMatrix3D = new Matrix(
                        0.0f, 1.0f, 0.0f, 0.0f,
                        -1.0f, 0.0f, 0.0f, 0.0f,
                        0.0f, 0.0f, 1.0f, 0.0f,
                        0.0f, 0.0f, 0.0f, 1.0f);
                    break;

                default:
                    rotMatrix = Matrix.Identity;
                    break;
            }

            // If we have a valid client bounds then update the graphics device.
            if (_clientBounds.Width > 0 && _clientBounds.Height > 0)
                Platform.Game.graphicsDeviceManager.ApplyChanges();
        }

        #region Public Methods

        public void Dispose()
        {
            //window.Dispose();
        }

        protected override void SetTitle(string title) {   }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        #endregion
    }
}

