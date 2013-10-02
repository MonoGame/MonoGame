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
#if WINDOWS || LINUX
using MouseInfo = OpenTK.Input.Mouse;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.AppKit;
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

        private static MouseState _defaultState = new MouseState();

#if (WINDOWS && OPENGL) || LINUX
	private static OpenTK.Input.MouseDevice _mouse = null;			
#endif

#if (WINDOWS && OPENGL)

        static OpenTK.GameWindow Window;

        internal static void setWindows(OpenTK.GameWindow window)
        {
            Window = window;
            _mouse = window.Mouse;        
        }

#elif (WINDOWS && DIRECTX)

        static System.Windows.Forms.Form Window;

        internal static void SetWindows(System.Windows.Forms.Form window)
        {
            Window = window;
        }
        
#elif LINUX         
        
        static OpenTK.GameWindow Window;

        internal static void setWindows(OpenTK.GameWindow window)
	{
            Window = window;
            
	    _mouse = window.Mouse;
	    _mouse.Move += HandleWindowMouseMove;
	}

        internal static void HandleWindowMouseMove (object sender, OpenTK.Input.MouseMoveEventArgs e)
	{          
	    UpdateStatePosition(e.X, e.Y);
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
#if (WINDOWS && OPENGL) 
                return IntPtr.Zero; // Suggest modify OpenTK.GameWindow to retrive handle.
#elif WINRT
                return IntPtr.Zero; // WinRT platform does not create traditionally window, so returns IntPtr.Zero.
#elif(WINDOWS && DIRECTX)
                return Window.Handle; 
#elif LINUX
                return IntPtr.Zero; // Suggest modify OpenTK.GameWindow to retrive handle.
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
        /// Gets mouse state information that includes position and button
        /// presses for the provided window
        /// </summary>
        /// <returns>Current state of the mouse.</returns>
        public static MouseState GetState(GameWindow window)
        {
#if MONOMAC
            //We need to maintain precision...
            window.MouseState.ScrollWheelValue = (int)ScrollWheelValue;

#elif (WINDOWS && OPENGL) || LINUX

	    // maybe someone is tring to get mouse before initialize
	    if (_mouse == null)
            return window.MouseState;

#if (WINDOWS && OPENGL)
            var p = new POINT();
            GetCursorPos(out p);
            var pc = Window.PointToClient(p.ToPoint());
            window.MouseState.X = pc.X;
            window.MouseState.Y = pc.Y;
#endif

            window.MouseState.LeftButton = _mouse[OpenTK.Input.MouseButton.Left] ? ButtonState.Pressed : ButtonState.Released;
			window.MouseState.RightButton = _mouse[OpenTK.Input.MouseButton.Right] ? ButtonState.Pressed : ButtonState.Released;
			window.MouseState.MiddleButton = _mouse[OpenTK.Input.MouseButton.Middle] ? ButtonState.Pressed : ButtonState.Released;;

		// WheelPrecise is divided by 120 (WHEEL_DELTA) in OpenTK (WinGLNative.cs)
		// We need to counteract it to get the same value XNA provides
	    window.MouseState.ScrollWheelValue = (int)( _mouse.WheelPrecise * 120 );
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

#if (WINDOWS && (OPENGL || DIRECTX)) || LINUX
            // correcting the coordinate system
            // Only way to set the mouse position !!!
            var pt = Window.PointToScreen(new System.Drawing.Point(x, y));
#elif WINDOWS
            var pt = new System.Drawing.Point(0, 0);
#endif

#if WINDOWS
            SetCursorPos(pt.X, pt.Y);
#elif LINUX
            OpenTK.Input.Mouse.SetPosition(pt.X, pt.Y);
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

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// (Suggestion : Make another class for mouse extensions)
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll", EntryPoint = "GetCursorPos")]
        internal static extern bool GetCursorPos(out POINT lpPoint);
      
#elif MONOMAC
        [DllImport (MonoMac.Constants.CoreGraphicsLibrary)]
        extern static void CGWarpMouseCursorPosition(PointF newCursorPosition);
        
        [DllImport (MonoMac.Constants.CoreGraphicsLibrary)]
        extern static void CGSetLocalEventsSuppressionInterval(double seconds);
#endif

    }
}

