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

namespace Microsoft.Xna.Framework.Input
{    
    /// <summary>
    /// Represents the state of a mouse input device, including mouse cursor position and buttons pressed.
    /// </summary>
	public struct MouseState
	{
		int _x, _y;
		int _scrollWheelValue;
		ButtonState _leftButton;
		ButtonState _rightButton;
		ButtonState _middleButton;
		
        /// <summary>
        /// Initializes a new instance of the MouseState class.
        /// </summary>
        /// <param name="x">Horizontal mouse position.</param>
        /// <param name="y">Vertical mouse position.</param>
        /// <param name="scrollWheel">Mouse scroll wheel value.</param>
        /// <param name="leftButton">Left mouse button state.</param>
        /// <param name="middleButton">Middle mouse button state.</param>
        /// <param name="rightButton">Right mouse button state.</param>
        /// <param name="xButton1">XBUTTON1 state.</param>
        /// <param name="xButton2">XBUTTON2 state.</param>
        /// <remarks>Games normally use GetState to get the true mouse state. This constructor is used instead to simulate mouse input for passing within the game's own input subsystem.</remarks>
		public MouseState (
			int x,
			int y,
			int scrollWheel,
			ButtonState leftButton,
			ButtonState middleButton,
			ButtonState rightButton,
			ButtonState xButton1,
			ButtonState xButton2)
		{
			_x = x;
			_y = y;
			_scrollWheelValue = scrollWheel;
			_leftButton = leftButton;
			_middleButton = middleButton;
			_rightButton = rightButton;
		}
		
        /// <summary>
        /// Determines whether two MouseState instances are equal.
        /// </summary>
        /// <param name="left">Object on the left of the equal sign.</param>
        /// <param name="right">Object on the right of the equal sign.</param>
        /// <returns>true if the instances are equal; false otherwise.</returns>
		public static bool operator ==(MouseState left, MouseState right)
		{
			return left._x == right._x &&
				   left._y == right._y &&
				   left._leftButton == right._leftButton &&
				   left._middleButton == right._middleButton &&
				   left._rightButton == right._rightButton &&
                   left._scrollWheelValue == right._scrollWheelValue;
		}
		
        /// <summary>
        /// Determines whether two MouseState instances are not equal.
        /// </summary>
        /// <param name="left">Object on the left of the equal sign.</param>
        /// <param name="right">Object on the right of the equal sign.</param>
        /// <returns>true if the objects are not equal; false otherwise.</returns>
		public static bool operator !=(MouseState left, MouseState right)
		{
			return !(left == right);
		}

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">Object with which to make the comparison.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is MouseState)
                return this == (MouseState)obj;
            return false;
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        /// <returns>Hash code for this object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Specifies the horizontal position of the mouse cursor.
        /// </summary>
		public int X {
			get {
				return _x;
			}
			internal set {
				_x = value;
			}
		}

        /// <summary>
        /// Specifies the vertical position of the mouse cursor.
        /// </summary>
		public int Y {
			get {
				return _y;
			}
			internal set {
				_y = value;
			}
		}

        /// <summary>
        /// Returns the state of the left mouse button.
        /// </summary>
		public ButtonState LeftButton { 
			get {
				return _leftButton;
			}
			internal set { _leftButton = value; }
		}

        /// <summary>
        /// Returns the state of the middle mouse button.
        /// </summary>
		public ButtonState MiddleButton { 
			get {
				return _middleButton;
			}
			internal set { _middleButton = value; }			
		}

        /// <summary>
        /// Returns the state of the right mouse button.
        /// </summary>
		public ButtonState RightButton { 
			get {
				return _rightButton;
			}
			internal set { _rightButton = value; }
		}

        /// <summary>
        /// Gets the cumulative mouse scroll wheel value since the game was started.
        /// </summary>
		public int ScrollWheelValue { 
			get {
				return _scrollWheelValue;
			}
			internal set { _scrollWheelValue = value; }
		}

        /// <summary>
        /// Returns the state of XBUTTON1.
        /// </summary>
        /// <remarks>XBUTTON1 and XBUTTON2 are additional buttons used on many mouse devices, often for forward and backward navigation in Web browsers. They return the same data as standard mouse buttons.</remarks>
		public ButtonState XButton1 { 
			get {
				return ButtonState.Released;
			}
		}

        /// <summary>
        /// Returns the state of XBUTTON2.
        /// </summary>
        /// <remarks>XBUTTON1 and XBUTTON2 are additional buttons used on many mouse devices, often for forward and backward navigation in Web browsers. They return the same data as standard mouse buttons.</remarks>
		public ButtonState XButton2 { 
			get {
				return ButtonState.Released;
			}
		}
	}
}

