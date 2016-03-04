// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if MONOMAC || WINDOWS
using System.Runtime.InteropServices;
using System.Drawing;
#endif

#if OPENGL && MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.Foundation;
using MonoMac.AppKit;
#else
using Foundation;
using AppKit;
using PointF = CoreGraphics.CGPoint;
#endif
#endif

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Allows reading position and button click information from mouse.
    /// </summary>
    public static class Mouse
    {
        internal static GameWindow PrimaryWindow;

        private static readonly MouseState _defaultState = new MouseState();

#if DESKTOPGL || ANGLE

        internal static bool BorderSet;
        internal static int ScrollY;

        internal static void SetWindows(GameWindow window)
        {
            PrimaryWindow = window;
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
                return PrimaryWindow.Handle;
#elif(WINDOWS && DIRECTX)
                return Window.Handle;
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
<<<<<<< 3de0427aae6b8ee7db1511ba73387b404f7f5b75

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
=======
            
            int x, y;
            
            var state = Sdl.Mouse.GetState(out x, out y);
            
            window.MouseState.X = x;
            window.MouseState.Y = y;

            window.MouseState.LeftButton = (state.HasFlag(Sdl.Mouse.Button.Left)) ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.MiddleButton = (state.HasFlag(Sdl.Mouse.Button.Middle)) ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.RightButton = (state.HasFlag(Sdl.Mouse.Button.Right)) ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.XButton1 = (state.HasFlag(Sdl.Mouse.Button.X1Mask)) ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.XButton2 = (state.HasFlag(Sdl.Mouse.Button.X2Mask)) ? ButtonState.Pressed : ButtonState.Released;

            window.MouseState.ScrollWheelValue = ScrollY;
>>>>>>> [SDL] Base SDL Implementation
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

#if (WINDOWS && DIRECTX)
            // correcting the coordinate system
            // Only way to set the mouse position !!!
            var pt = Window.PointToScreen(new System.Drawing.Point(x, y));
#elif WINDOWS
            var pt = new System.Drawing.Point(0, 0);
#endif

#if DESKTOPGL || ANGLE
            Sdl.Mouse.WarpInWindow(PrimaryWindow.Handle, x, y);
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

        /// <summary>
        /// Sets the cursor image to the specified MouseCursor.
        /// </summary>
        /// <param name="cursor">Mouse cursor to use for the cursor image.</param>
        public static void SetCursor (MouseCursor cursor)
        {
#if DESKTOPGL
            Sdl.Mouse.SetCursor (cursor.Handle);
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

