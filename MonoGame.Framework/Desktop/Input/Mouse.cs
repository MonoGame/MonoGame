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
﻿
using System;

#if WINRT
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.System;
#else
using MouseInfo = OpenTK.Input.Mouse;
#endif

namespace Microsoft.Xna.Framework.Input
{
	// TODO on opentk 1.1 release this class should be reviewed to decouple it from GameWindow
	// OpenTK.Input.Mouse and OpenTK.Input.Mouse.GetState should be enough
	
	// TODO verify if why mouse middle button is laggy (maybe it's my mouse or the opentk implementation)
	
	public static class Mouse
	{
#if !WINRT
		private static OpenTK.Input.MouseDevice _mouse = null;			
#endif

#if WINRT
        static internal CoreWindow Window;
#elif WINDOWS
        static OpenTK.GameWindow Window;
        internal static void setWindows(OpenTK.GameWindow window)
        {
            Window = window;
            _mouse = window.Mouse;        
        }
#else
        private static int _x, _y;
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

        public static MouseState GetState ()
		{				
#if !WINRT
			// maybe someone is tring to get mouse before initialize
			if (_mouse == null)
			{
				return new MouseState(0, 0);
			}
#endif
	
#if WINRT
            var pos = Window.PointerPosition;

            // TODO: Probably the wrong way to convert to pixels!
            var ms = new MouseState((int)pos.X, (int)pos.Y);

#elif WINDOWS                
            //MouseState ms = new MouseState(Window.Mouse.X, Window.Mouse.Y);
            POINT p = new POINT();
            GetCursorPos(out p);
            System.Drawing.Point pc = Window.PointToClient(p.ToPoint());
            MouseState ms = new MouseState(pc.X, pc.Y);
#else
            MouseState ms = new MouseState(_x, _y);
#endif   
    
#if WINRT
            ms.LeftButton = Window.GetAsyncKeyState(VirtualKey.LeftButton) == CoreVirtualKeyStates.Down ? ButtonState.Pressed : ButtonState.Released;
            ms.RightButton = Window.GetAsyncKeyState(VirtualKey.RightButton) == CoreVirtualKeyStates.Down ? ButtonState.Pressed : ButtonState.Released;
            ms.MiddleButton = Window.GetAsyncKeyState(VirtualKey.MiddleButton) == CoreVirtualKeyStates.Down ? ButtonState.Pressed : ButtonState.Released;
            ms.ScrollWheelValue = 0; // TODO: How do i get the scroll wheel state?
#else
            ms.LeftButton = _mouse[OpenTK.Input.MouseButton.Left] ? ButtonState.Pressed : ButtonState.Released;
			ms.RightButton = _mouse[OpenTK.Input.MouseButton.Right] ? ButtonState.Pressed : ButtonState.Released;
			ms.MiddleButton = _mouse[OpenTK.Input.MouseButton.Middle] ? ButtonState.Pressed : ButtonState.Released;;
			ms.ScrollWheelValue = _mouse.Wheel;
#endif			

			return ms;
		}

#if WINRT
        public static void SetPosition(int x, int y)
        {
            // TODO: How do i do this in WinRT... i can't right?
        }   
#elif WINDOWS
        public static void SetPosition(int x, int y)
        {
            ///correcting the coordinate system
            ///Only way to set the mouse position !!!
            System.Drawing.Point pt = Window.PointToScreen(new System.Drawing.Point(x, y));
            SetCursorPos(pt.X, pt.Y);
        }       
#else
		public static void SetPosition (int x, int y)
		{
			// TODO propagate change to opentk mouse object (requires opentk 1.1)
			//throw new NotImplementedException("Feature not implemented.");
			_x = x;
			_y = y;
        }

#endif

#if !WINRT && WINDOWS
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

        #endregion
    }
}

