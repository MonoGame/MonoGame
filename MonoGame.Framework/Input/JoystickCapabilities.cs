// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Describes joystick capabilities.
    /// </summary>
    public struct JoystickCapabilities
    {
        /// <summary>
        /// Gets a value indicating whether the joystick is connected.
        /// </summary>
        /// <value><c>true</c> if the joystick is connected; otherwise, <c>false</c>.</value>
        public bool IsConnected { get; internal set; }

        /// <summary>
        /// Gets the unique identifier of the joystick.
        /// </summary>
        /// <value>String representing the unique identifier of the joystick.</value>
        public string Identifier { get; internal set; }

        /// <summary>
        /// Gets the joystick's display name.
        /// </summary>
        /// <value>String representing the display name of the joystick.</value>
        public string DisplayName { get; internal set; }

        /// <summary>
        /// Gets a value indicating if the joystick is a gamepad.
        /// </summary>
        /// <value><c>true</c> if the joystick is a gamepad; otherwise, <c>false</c>.</value>
        public bool IsGamepad { get; internal set; }

        /// <summary>
        /// Gets the axis count.
        /// </summary>
        /// <value>The number of axes that the joystick possesses.</value>
        public int AxisCount { get; internal set; }

        /// <summary>
        /// Gets the button count.
        /// </summary>
        /// <value>The number of buttons that the joystick possesses.</value>
        public int ButtonCount { get; internal set; }

        /// <summary>
        /// Gets the hat count.
        /// </summary>
        /// <value>The number of hats/dpads that the joystick possesses.</value>
        public int HatCount { get; internal set; }

        /// <summary>
        /// Determines whether a specified instance of <see cref="Microsoft.Xna.Framework.Input.JoystickCapabilities"/>
        /// is equal to another specified <see cref="Microsoft.Xna.Framework.Input.JoystickCapabilities"/>.
        /// </summary>
        /// <param name="left">The first <see cref="Microsoft.Xna.Framework.Input.JoystickCapabilities"/> to compare.</param>
        /// <param name="right">The second <see cref="Microsoft.Xna.Framework.Input.JoystickCapabilities"/> to compare.</param>
        /// <returns><c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(JoystickCapabilities left, JoystickCapabilities right)
        {
            return left.IsConnected == right.IsConnected &&
               left.Identifier == right.Identifier &&
               left.IsGamepad == right.IsGamepad &&
               left.AxisCount == right.AxisCount &&
               left.ButtonCount == right.ButtonCount &&
               left.HatCount == right.HatCount;
        }

        /// <summary>
        /// Determines whether a specified instance of <see cref="Microsoft.Xna.Framework.Input.JoystickCapabilities"/>
        /// is not equal to another specified <see cref="Microsoft.Xna.Framework.Input.JoystickCapabilities"/>.
        /// </summary>
        /// <param name="left">The first <see cref="Microsoft.Xna.Framework.Input.JoystickCapabilities"/> to compare.</param>
        /// <param name="right">The second <see cref="Microsoft.Xna.Framework.Input.JoystickCapabilities"/> to compare.</param>
        /// <returns><c>true</c> if <c>left</c> and <c>right</c> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(JoystickCapabilities left, JoystickCapabilities right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return (obj is JoystickCapabilities) && (this == (JoystickCapabilities)obj);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities"/>.</returns>
        public override string ToString()
        {
            return "[JoystickCapabilities: IsConnected=" + IsConnected + ", Identifier=" + Identifier + ", DisplayName=" + DisplayName + ", IsGamepad=" + IsGamepad + " , AxisCount=" + AxisCount + ", ButtonCount=" + ButtonCount + ", HatCount=" + HatCount + "]";
        }
    }
}

