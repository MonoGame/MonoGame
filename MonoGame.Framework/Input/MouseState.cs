// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Represents a mouse state with cursor position and button press information.
    /// </summary>
    public struct MouseState
    {
        private const byte LeftButtonFlag = 1;
        private const byte RightButtonFlag = 2;
        private const byte MiddleButtonFlag = 4;
        private const byte XButton1Flag = 8;
        private const byte XButton2Flag = 16;

        private int _x;
        private int _y;
        private int _scrollWheelValue;
        private int _horizontalScrollWheelValue;
        private byte _buttons;

        /// <summary>
        /// Initializes a new instance of the MouseState.
        /// </summary>
        /// <param name="x">Horizontal position of the mouse in relation to the window.</param>
        /// <param name="y">Vertical position of the mouse in relation to the window.</param>
        /// <param name="scrollWheel">Mouse scroll wheel's value.</param>
        /// <param name="leftButton">Left mouse button's state.</param>
        /// <param name="middleButton">Middle mouse button's state.</param>
        /// <param name="rightButton">Right mouse button's state.</param>
        /// <param name="xButton1">XBUTTON1's state.</param>
        /// <param name="xButton2">XBUTTON2's state.</param>
        /// <remarks>Normally <see cref="Mouse.GetState()"/> should be used to get mouse current state. The constructor is provided for simulating mouse input.</remarks>
        public MouseState(
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
            _buttons = (byte)(
                (leftButton == ButtonState.Pressed ? LeftButtonFlag : 0) |
                (rightButton == ButtonState.Pressed ? RightButtonFlag : 0) |
                (middleButton == ButtonState.Pressed ? MiddleButtonFlag : 0) |
                (xButton1 == ButtonState.Pressed ? XButton1Flag : 0) |
                (xButton2 == ButtonState.Pressed ? XButton2Flag : 0)
            );
            _horizontalScrollWheelValue = 0;
        }

        /// <summary>
        /// Initializes a new instance of the MouseState.
        /// </summary>
        /// <param name="x">Horizontal position of the mouse in relation to the window.</param>
        /// <param name="y">Vertical position of the mouse in relation to the window.</param>
        /// <param name="scrollWheel">Mouse scroll wheel's value.</param>
        /// <param name="leftButton">Left mouse button's state.</param>
        /// <param name="middleButton">Middle mouse button's state.</param>
        /// <param name="rightButton">Right mouse button's state.</param>
        /// <param name="xButton1">XBUTTON1's state.</param>
        /// <param name="xButton2">XBUTTON2's state.</param>
        /// <param name="horizontalScrollWheel">Mouse horizontal scroll wheel's value.</param>
        /// <remarks>Normally <see cref="Mouse.GetState()"/> should be used to get mouse current state. The constructor is provided for simulating mouse input.</remarks>
        public MouseState(
            int x,
            int y,
            int scrollWheel,
            ButtonState leftButton,
            ButtonState middleButton,
            ButtonState rightButton,
            ButtonState xButton1,
            ButtonState xButton2,
            int horizontalScrollWheel)
        {
            _x = x;
            _y = y;
            _scrollWheelValue = scrollWheel;
            _buttons = (byte)(
                (leftButton == ButtonState.Pressed ? LeftButtonFlag : 0) |
                (rightButton == ButtonState.Pressed ? RightButtonFlag : 0) |
                (middleButton == ButtonState.Pressed ? MiddleButtonFlag : 0) |
                (xButton1 == ButtonState.Pressed ? XButton1Flag : 0) |
                (xButton2 == ButtonState.Pressed ? XButton2Flag : 0)
            );
            _horizontalScrollWheelValue = horizontalScrollWheel;
        }

        /// <summary>
        /// Compares whether two MouseState instances are equal.
        /// </summary>
        /// <param name="left">MouseState instance on the left of the equal sign.</param>
        /// <param name="right">MouseState instance  on the right of the equal sign.</param>
        /// <returns>true if the instances are equal; false otherwise.</returns>
        public static bool operator ==(MouseState left, MouseState right)
        {
            return left._x == right._x &&
                   left._y == right._y &&
                   left._buttons == right._buttons &&
                   left._scrollWheelValue == right._scrollWheelValue &&
                   left._horizontalScrollWheelValue == right._horizontalScrollWheelValue;
        }

        /// <summary>
        /// Compares whether two MouseState instances are not equal.
        /// </summary>
        /// <param name="left">MouseState instance on the left of the equal sign.</param>
        /// <param name="right">MouseState instance  on the right of the equal sign.</param>
        /// <returns>true if the objects are not equal; false otherwise.</returns>
        public static bool operator !=(MouseState left, MouseState right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Compares whether current instance is equal to specified object.
        /// </summary>
        /// <param name="obj">The MouseState to compare.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is MouseState)
                return this == (MouseState)obj;
            return false;
        }

        /// <summary>
        /// Gets the hash code for MouseState instance.
        /// </summary>
        /// <returns>Hash code of the object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _x;
                hashCode = (hashCode * 397) ^ _y;
                hashCode = (hashCode * 397) ^ _scrollWheelValue;
                hashCode = (hashCode * 397) ^ _horizontalScrollWheelValue;
                hashCode = (hashCode * 397) ^ (int)_buttons;
                return hashCode;
            }
        }

        /// <summary>
        /// Gets horizontal position of the cursor in relation to the window.
        /// </summary>
        public int X
        {
            get
            {
                return _x;
            }
            internal set
            {
                _x = value;
            }
        }

        /// <summary>
        /// Gets vertical position of the cursor in relation to the window.
        /// </summary>
        public int Y
        {
            get
            {
                return _y;
            }
            internal set
            {
                _y = value;
            }
        }

        /// <summary>
        /// Gets cursor position.
        /// </summary>
        public Point Position
        {
            get { return new Point(_x, _y); }
        }

        /// <summary>
        /// Gets state of the left mouse button.
        /// </summary>
        public ButtonState LeftButton
        {
            get
            {
                return ((_buttons & LeftButtonFlag) > 0) ? ButtonState.Pressed : ButtonState.Released;
            }
            internal set
            {
                if (value == ButtonState.Pressed)
                {
                    _buttons = (byte)(_buttons | LeftButtonFlag);
                }
                else
                {
                    _buttons = (byte)(_buttons & (~LeftButtonFlag));
                }
            }
        }

        /// <summary>
        /// Gets state of the middle mouse button.
        /// </summary>
        public ButtonState MiddleButton
        {
            get
            {
                return ((_buttons & MiddleButtonFlag) > 0) ? ButtonState.Pressed : ButtonState.Released;
            }
            internal set
            {
                if (value == ButtonState.Pressed)
                {
                    _buttons = (byte)(_buttons | MiddleButtonFlag);
                }
                else
                {
                    _buttons = (byte)(_buttons & (~MiddleButtonFlag));
                }
            }
        }

        /// <summary>
        /// Gets state of the right mouse button.
        /// </summary>
        public ButtonState RightButton
        {
            get
            {
                return ((_buttons & RightButtonFlag) > 0) ? ButtonState.Pressed : ButtonState.Released;
            }
            internal set
            {
                if (value == ButtonState.Pressed)
                {
                    _buttons = (byte)(_buttons | RightButtonFlag);
                }
                else
                {
                    _buttons = (byte)(_buttons & (~RightButtonFlag));
                }
            }
        }

        /// <summary>
        /// Returns cumulative scroll wheel value since the game start.
        /// </summary>
        public int ScrollWheelValue
        {
            get
            {
                return _scrollWheelValue;
            }
            internal set { _scrollWheelValue = value; }
        }

        /// <summary>
        /// Returns the cumulative horizontal scroll wheel value since the game start
        /// </summary>
        public int HorizontalScrollWheelValue
        {
            get
            {
                return _horizontalScrollWheelValue;
            }
            internal set { _horizontalScrollWheelValue = value; }
        }

        /// <summary>
        /// Gets state of the XButton1.
        /// </summary>
        public ButtonState XButton1
        {
            get
            {
                return ((_buttons & XButton1Flag) > 0) ? ButtonState.Pressed : ButtonState.Released;
            }
            internal set
            {
                if (value == ButtonState.Pressed)
                {
                    _buttons = (byte)(_buttons | XButton1Flag);
                }
                else
                {
                    _buttons = (byte)(_buttons & (~XButton1Flag));
                }
            }
        }

        /// <summary>
        /// Gets state of the XButton2.
        /// </summary>
        public ButtonState XButton2
        {
            get
            {
                return ((_buttons & XButton2Flag) > 0) ? ButtonState.Pressed : ButtonState.Released;
            }
            internal set
            {
                if (value == ButtonState.Pressed)
                {
                    _buttons = (byte)(_buttons | XButton2Flag);
                }
                else
                {
                    _buttons = (byte)(_buttons & (~XButton2Flag));
                }
            }
        }
    }
}
