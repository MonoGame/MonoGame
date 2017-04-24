// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Text;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Describes current joystick state.
    /// </summary>
    public struct JoystickState
    {
        /// <summary>
        /// Gets a value indicating whether the joystick is connected.
        /// </summary>
        /// <value><c>true</c> if the joystick is connected; otherwise, <c>false</c>.</value>
        public bool IsConnected { get; internal set; }

        /// <summary>
        /// Gets the joystick axis values.
        /// </summary>
        /// <value>An array list of ints that indicate axis values.</value>
        public int[] Axes { get; internal set; }

        /// <summary>
        /// Gets the joystick button values.
        /// </summary>
        /// <value>An array list of ButtonState that indicate button values.</value>
        public ButtonState[] Buttons { get; internal set; }

        /// <summary>
        /// Gets the joystick hat values.
        /// </summary>
        /// <value>An array list of <see cref="JoystickHat"/> that indicate hat values.</value>
        public JoystickHat[] Hats{ get; internal set; }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.JoystickState"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            var hash = 0;

            if (IsConnected)
            {
                unchecked
                {
                    foreach (var axis in Axes)
                        hash = (hash * 397) ^ axis;

                    for (int i = 0; i < Buttons.Length; i++)
                        hash = hash ^ ((int)Buttons[i] << (i % 32));

                    foreach (var hat in Hats)
                        hash = (hash * 397) ^ hat.GetHashCode();
                }
            }

            return hash;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickState"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickState"/>.</returns>
        public override string ToString()
        {
            var ret = new StringBuilder(54 - 2 + Axes.Length * 7 + Buttons.Length + Hats.Length * 5);
            ret.Append("[JoystickState: IsConnected=" + (IsConnected ? 1 : 0));

            if (IsConnected)
            {
                ret.Append(", Axes=");
                foreach (var axis in Axes)
                    ret.Append((axis > 0 ? "+" : "") + axis.ToString("00000") + " ");
                ret.Length--;

                ret.Append(", Buttons=");
                foreach (var button in Buttons)
                    ret.Append((int)button);

                ret.Append(", Hats=");
                foreach (var hat in Hats)
                    ret.Append(hat + " ");
                ret.Length--;
            }

            ret.Append("]");
            return ret.ToString();
        }
    }
}

