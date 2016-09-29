// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    public struct GamePadButtons
    {
        internal Buttons buttons;

        public ButtonState A
        {
            get
            {
                return ((buttons & Buttons.A) == Buttons.A) ? ButtonState.Pressed : ButtonState.Released;
            }
        }
        public ButtonState B
        {
            get
            {
                return ((buttons & Buttons.B) == Buttons.B) ? ButtonState.Pressed : ButtonState.Released;
            }
        }
        public ButtonState Back
        {
            get
            {
                return ((buttons & Buttons.Back) == Buttons.Back) ? ButtonState.Pressed : ButtonState.Released;
            }
        }
        public ButtonState X
        {
            get
            {
                return ((buttons & Buttons.X) == Buttons.X) ? ButtonState.Pressed : ButtonState.Released;
            }
        }
        public ButtonState Y
        {
            get
            {
                return ((buttons & Buttons.Y) == Buttons.Y) ? ButtonState.Pressed : ButtonState.Released;
            }
        }
        public ButtonState Start
        {
            get
            {
                return ((buttons & Buttons.Start) == Buttons.Start) ? ButtonState.Pressed : ButtonState.Released;
            }
        }
        public ButtonState LeftShoulder
        {
            get
            {
                return ((buttons & Buttons.LeftShoulder) == Buttons.LeftShoulder) ? ButtonState.Pressed : ButtonState.Released;
            }
        }
        public ButtonState LeftStick
        {
            get
            {
                return ((buttons & Buttons.LeftStick) == Buttons.LeftStick) ? ButtonState.Pressed : ButtonState.Released;
            }
        }
        public ButtonState RightShoulder
        {
            get
            {
                return ((buttons & Buttons.RightShoulder) == Buttons.RightShoulder) ? ButtonState.Pressed : ButtonState.Released;
            }
        }
        public ButtonState RightStick
        {
            get
            {
                return ((buttons & Buttons.RightStick) == Buttons.RightStick) ? ButtonState.Pressed : ButtonState.Released;
            }
        }
        public ButtonState BigButton
        {
            get
            {
                return ((buttons & Buttons.BigButton) == Buttons.BigButton) ? ButtonState.Pressed : ButtonState.Released;
            }
        }
		
        public GamePadButtons(Buttons buttons)
        {
            this.buttons = buttons;
        }
        internal GamePadButtons(params Buttons[] buttons)
            : this()
        {
            foreach (Buttons b in buttons)
                this.buttons |= b;
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="GamePadButtons"/> are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, false.</returns>
        public static bool operator ==(GamePadButtons left, GamePadButtons right)
        {
            return left.buttons == right.buttons;
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="GamePadButtons"/> are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
        public static bool operator !=(GamePadButtons left, GamePadButtons right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare to this instance.</param>
        /// <returns>true if <paramref name="obj"/> is a <see cref="GamePadButtons"/> and has the same value as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return (obj is GamePadButtons) && (this == (GamePadButtons)obj);
        }

		public override int GetHashCode ()
		{
			return (int)this.buttons;
		}
    }
}

