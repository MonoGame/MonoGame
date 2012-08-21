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

#if WINDOWS || MONOMAC || LINUX
using MouseInfo = OpenTK.Input.Mouse;
#else
using Microsoft.Xna.Framework.Input.Touch;
#endif

#if WINRT
using Windows.Devices.Input;
#endif

namespace Microsoft.Xna.Framework.Input
{
    public static class Mouse
    {
		internal static MouseState State;

#if WINRT
        private static readonly MouseCapabilities _mouseCapabilities = new MouseCapabilities();
#endif

#if WINDOWS || MONOMAC || LINUX
		private static OpenTK.Input.MouseDevice _mouse = null;			
#else
        private static int _mouseTouchId = -1;
#endif

#if WINDOWS

        static OpenTK.GameWindow Window;

        internal static void setWindows(OpenTK.GameWindow window)
        {
            Window = window;
            _mouse = window.Mouse;        
        }

#elif MONOMAC || LINUX

        internal static void UpdateMouseInfo(OpenTK.Input.MouseDevice mouse)
		{
			_mouse = mouse;
			_mouse.Move += HandleWindowMouseMove;
		}

        internal static void HandleWindowMouseMove (object sender, OpenTK.Input.MouseMoveEventArgs e)
		{
			SetPosition(e.X, e.Y);
		}

#endif

        #region Public interface

        public static IntPtr WindowHandle { get; set; }
		
        public static MouseState GetState()
        {
#if WINDOWS || MONOMAC || LINUX

			// maybe someone is tring to get mouse before initialize
			if (_mouse == null)
                return State;

#if WINDOWS
            var p = new POINT();
            GetCursorPos(out p);
            var pc = Window.PointToClient(p.ToPoint());
            State.X = pc.X;
            State.Y = pc.Y;
#endif

            State.LeftButton = _mouse[OpenTK.Input.MouseButton.Left] ? ButtonState.Pressed : ButtonState.Released;
			State.RightButton = _mouse[OpenTK.Input.MouseButton.Right] ? ButtonState.Pressed : ButtonState.Released;
			State.MiddleButton = _mouse[OpenTK.Input.MouseButton.Middle] ? ButtonState.Pressed : ButtonState.Released;;

			// WheelPrecise is divided by 120 (WHEEL_DELTA) in OpenTK (WinGLNative.cs)
			// We need to counteract it to get the same value XNA provides
			State.ScrollWheelValue = (int)( _mouse.WheelPrecise * 120 );

#else

#if WINRT
            // NOTE: Some WinRT devices have a mouse driver installed even when
            // there is no physical mouse connected to the device.  You can fix 
            // this by disabling the mouse in the Device Manager.

            // If we have a mouse then don't bother faking events.
            if (_mouseCapabilities.MousePresent != 0)
                return State;
#endif

            // Release all the buttons.
            State.LeftButton = ButtonState.Released;
            State.RightButton = ButtonState.Released;
            State.MiddleButton = ButtonState.Released;

            // Look for a new or previous touch point.
            var touchState = TouchPanel.GetState();
            foreach (var touch in touchState)
            {
                // Skip released touch points.
                if (    touch.State != TouchLocationState.Pressed &&
                        touch.State != TouchLocationState.Moved)
                    continue;

                // Look for a new touch or the last touch point.
                if (_mouseTouchId == -1 || touch.Id == _mouseTouchId)
                {
                    // Store the touch point for the next pass.
                    _mouseTouchId = touch.Id;

                    // Set the touch state.
                    State.X = (int)touch.Position.X;
                    State.Y = (int)touch.Position.Y;
                    State.LeftButton = ButtonState.Pressed;
                    break;
                }
            }

            // If we didn't find the touch then 
            // look for a new one next time.
            if (State.LeftButton == ButtonState.Released)
                _mouseTouchId = -1;

#endif

            return State;
        }

        public static void SetPosition(int x, int y)
        {
            State.X = x;
            State.Y = y;
			
#if WINDOWS
            ///correcting the coordinate system
            ///Only way to set the mouse position !!!
            System.Drawing.Point pt = Window.PointToScreen(new System.Drawing.Point(x, y));
            SetCursorPos(pt.X, pt.Y);
#elif MONOMAC || LINUX
			// TODO propagate change to opentk mouse object (requires opentk 1.1)
			//throw new NotImplementedException("Feature not implemented.");
#endif			
        }

        #endregion // Public interface

#if WINDOWS

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int X, int Y);

        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct POINT
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
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
      
#endif

    }
}

