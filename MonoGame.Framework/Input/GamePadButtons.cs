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
    }
}

