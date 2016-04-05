#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

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
#endregion License

using System;

#if MONOMAC || WINDOWS
using System.Runtime.InteropServices;
using System.Drawing;
#endif

#if OPENGL
#if DESKTOPGL
using MouseInfo = OpenTK.Input.Mouse;
#elif MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.Foundation;
using MonoMac.AppKit;
#else
using Foundation;
using AppKit;
using PointF = CoreGraphics.CGPoint;
#endif
#endif
#endif

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Allows reading position and button click information from mouse.
    /// </summary>
    public static class Mouse
    {
        internal static GameWindow PrimaryWindow = null;

        private static readonly MouseState _defaultState = new MouseState();

#if DESKTOPGL || ANGLE

        static OpenTK.INativeWindow Window;

        internal static void setWindows(GameWindow window)
        {
            PrimaryWindow = window;
            if (window is OpenTKGameWindow)
            {
                Window = (window as OpenTKGameWindow).Window;
            }
        }

#elif (WINDOWS && DIRECTX)

        static System.Windows.Forms.Form Window;

        internal static void SetWindows(System.Windows.Forms.Form window)
        {
            Window = window;
        }

#elif MONOMAC
        internal static GameWindow Window;
        internal static float ScrollWheelValue;
#endif

        /// <summary>
        /// Gets or sets the window handle for current mouse processing.
        /// </summary> 
        public static IntPtr WindowHandle 
        { 
            get
            { 
#if DESKTOPGL || ANGLE
                return Window.WindowInfo.Handle;
#elif WINRT
                return IntPtr.Zero; // WinRT platform does not create traditionally window, so returns IntPtr.Zero.
#elif(WINDOWS && DIRECTX)
                return Window.Handle; 
#elif MONOMAC
                return IntPtr.Zero;
#else
                return IntPtr.Zero;
#endif
            }
            set
            {
                // only for XNA compatibility, yet
            }
        }

        #region Public methods

        /// <summary>
        /// This API is an extension to XNA.
        /// Gets mouse state information that includes position and button
        /// presses for the provided window
        /// </summary>
        /// <returns>Current state of the mouse.</returns>
        public static MouseState GetState(GameWindow window)
        {
#if MONOMAC
            //We need to maintain precision...
            window.MouseState.ScrollWheelValue = (int)ScrollWheelValue;

#elif DESKTOPGL || ANGLE

            var state = OpenTK.Input.Mouse.GetCursorState();

            var clientBounds = window.ClientBounds;
            window.MouseState.X = state.X - clientBounds.X;
            window.MouseState.Y = state.Y - clientBounds.Y;

            window.MouseState.LeftButton = (ButtonState)state.LeftButton;
            window.MouseState.RightButton = (ButtonState)state.RightButton;
            window.MouseState.MiddleButton = (ButtonState)state.MiddleButton;
            window.MouseState.XButton1 = (ButtonState)state.XButton1;
            window.MouseState.XButton2 = (ButtonState)state.XButton2;

            // XNA uses the winapi convention of 1 click = 120 delta
            // OpenTK scales 1 click = 1.0 delta, so make that match
            window.MouseState.ScrollWheelValue = (int)(state.Scroll.Y * 120);
#endif

            return window.MouseState;
        }

        /// <summary>
        /// Gets mouse state information that includes position and button presses
        /// for the primary window
        /// </summary>
        /// <returns>Current state of the mouse.</returns>
        public static MouseState GetState()
        {
#if ANDROID

            // Before MouseState was changed to take in a 
            // gamewindow, Android seemed to never update 
            // the previous static MouseState that existed.
            // This implies that the default behavior is to return
            // default(MouseState); A static one is used to prevent
            // constant reallocations
            // This will need to change when MonoGame supports desktop Android.
            // Related discussion: https://github.com/mono/MonoGame/pull/1749

            return _defaultState;
#else
            if (PrimaryWindow != null)
                return GetState(PrimaryWindow);

            return _defaultState;
#endif
        }

        /// <summary>
        /// Sets mouse cursor's relative position to game-window.
        /// </summary>
        /// <param name="x">Relative horizontal position of the cursor.</param>
        /// <param name="y">Relative vertical position of the cursor.</param>
        public static void SetPosition(int x, int y)
        {
            UpdateStatePosition(x, y);

#if (WINDOWS && DIRECTX) || DESKTOPGL || ANGLE
            // correcting the coordinate system
            // Only way to set the mouse position !!!
            var pt = Window.PointToScreen(new System.Drawing.Point(x, y));
#elif WINDOWS
            var pt = new System.Drawing.Point(0, 0);
#endif

#if DESKTOPGL || ANGLE
            OpenTK.Input.Mouse.SetPosition(pt.X, pt.Y);
#elif WINDOWS
            SetCursorPos(pt.X, pt.Y);
#elif MONOMAC
            var mousePt = NSEvent.CurrentMouseLocation;
            NSScreen currentScreen = null;
            foreach (var screen in NSScreen.Screens) {
                if (screen.Frame.Contains(mousePt)) {
                    currentScreen = screen;
                    break;
                }
            }

            var point = new PointF(x, Window.ClientBounds.Height-y);
            var windowPt = Window.ConvertPointToView(point, null);
            var screenPt = Window.Window.ConvertBaseToScreen(windowPt);
            var flippedPt = new PointF(screenPt.X, currentScreen.Frame.Size.Height-screenPt.Y);
            flippedPt.Y += currentScreen.Frame.Location.Y;
            
            
            CGSetLocalEventsSuppressionInterval(0.0);
            CGWarpMouseCursorPosition(flippedPt);
            CGSetLocalEventsSuppressionInterval(0.25);
#elif WEB
            PrimaryWindow.MouseState.X = x;
            PrimaryWindow.MouseState.Y = y;
#endif
        }

        #endregion Public methods
    
        private static void UpdateStatePosition(int x, int y)
        {
            PrimaryWindow.MouseState.X = x;
            PrimaryWindow.MouseState.Y = y;
        }

#if WINDOWS

        [DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        internal static extern bool SetCursorPos(int X, int Y);

        /// <summary>
        /// Struct representing a point. 
        /// (Suggestion : Make another class for mouse extensions)
        /// </summary>
        [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;

            public System.Drawing.Point ToPoint()
            {
                return new System.Drawing.Point(X, Y);
            }

        }

#elif MONOMAC
#if PLATFORM_MACOS_LEGACY
        [DllImport (MonoMac.Constants.CoreGraphicsLibrary)]
        extern static void CGWarpMouseCursorPosition(PointF newCursorPosition);
        
        [DllImport (MonoMac.Constants.CoreGraphicsLibrary)]
        extern static void CGSetLocalEventsSuppressionInterval(double seconds);
#else
        [DllImport (ObjCRuntime.Constants.CoreGraphicsLibrary)]
        extern static void CGWarpMouseCursorPosition(CoreGraphics.CGPoint newCursorPosition);

        [DllImport (ObjCRuntime.Constants.CoreGraphicsLibrary)]
        extern static void CGSetLocalEventsSuppressionInterval(double seconds);
#endif
#endif

    }
}

