// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Holds the state of keystrokes by a keyboard.
    /// </summary>
	public struct KeyboardState
    {
        // Used for the common situation where GetPressedKeys will return an empty array
        static Keys[] empty = new Keys[0];

        #region Key Data

        // Array of 256 bits:
        uint keys0, keys1, keys2, keys3, keys4, keys5, keys6, keys7;

        bool InternalGetKey(Keys key)
        {
            uint mask = (uint)1 << (((int)key) & 0x1f);

            uint element;
            switch (((int)key) >> 5)
            {
                case 0: element = keys0; break;
                case 1: element = keys1; break;
                case 2: element = keys2; break;
                case 3: element = keys3; break;
                case 4: element = keys4; break;
                case 5: element = keys5; break;
                case 6: element = keys6; break;
                case 7: element = keys7; break;
                default: element = 0; break;
            }

            return (element & mask) != 0;
        }

        internal void InternalSetKey(Keys key)
        {
            uint mask = (uint)1 << (((int)key) & 0x1f);
            switch (((int)key) >> 5)
            {
                case 0: keys0 |= mask; break;
                case 1: keys1 |= mask; break;
                case 2: keys2 |= mask; break;
                case 3: keys3 |= mask; break;
                case 4: keys4 |= mask; break;
                case 5: keys5 |= mask; break;
                case 6: keys6 |= mask; break;
                case 7: keys7 |= mask; break;
            }
        }

        internal void InternalClearKey(Keys key)
        {
            uint mask = (uint)1 << (((int)key) & 0x1f);
            switch (((int)key) >> 5)
            {
                case 0: keys0 &= ~mask; break;
                case 1: keys1 &= ~mask; break;
                case 2: keys2 &= ~mask; break;
                case 3: keys3 &= ~mask; break;
                case 4: keys4 &= ~mask; break;
                case 5: keys5 &= ~mask; break;
                case 6: keys6 &= ~mask; break;
                case 7: keys7 &= ~mask; break;
            }
        }

        internal void InternalClearAllKeys()
        {
            keys0 = 0;
            keys1 = 0;
            keys2 = 0;
            keys3 = 0;
            keys4 = 0;
            keys5 = 0;
            keys6 = 0;
            keys7 = 0;
        }

        #endregion


        #region XNA Interface

        /// <summary>
        /// Gets the current state of the Caps Lock key.
        /// </summary>
        public bool CapsLock { get; private set; }

        /// <summary>
        /// Gets the current state of the Num Lock key.
        /// </summary>
        public bool NumLock { get; private set; }

        internal KeyboardState(List<Keys> keys, bool capsLock = false, bool numLock = false) : this()
        {
            CapsLock = capsLock;
            NumLock = numLock;

            keys0 = 0;
            keys1 = 0;
            keys2 = 0;
            keys3 = 0;
            keys4 = 0;
            keys5 = 0;
            keys6 = 0;
            keys7 = 0;

            if (keys != null)
                foreach (Keys k in keys)
                    InternalSetKey(k);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardState"/> class.
        /// </summary>
        /// <param name="keys">List of keys to be flagged as pressed on initialization.</param>
        /// <param name="capsLock">Caps Lock state.</param>
        /// <param name="numLock">Num Lock state.</param>
        public KeyboardState(Keys[] keys, bool capsLock = false, bool numLock = false) : this()
        {
            CapsLock = capsLock;
            NumLock = numLock;

            keys0 = 0;
            keys1 = 0;
            keys2 = 0;
            keys3 = 0;
            keys4 = 0;
            keys5 = 0;
            keys6 = 0;
            keys7 = 0;

            if (keys != null)
                foreach (Keys k in keys)
                    InternalSetKey(k);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardState"/> class.
        /// </summary>
        /// <param name="keys">List of keys to be flagged as pressed on initialization.</param>
        public KeyboardState(params Keys[] keys) : this()
        {
            CapsLock = false;
            NumLock = false;

            keys0 = 0;
            keys1 = 0;
            keys2 = 0;
            keys3 = 0;
            keys4 = 0;
            keys5 = 0;
            keys6 = 0;
            keys7 = 0;

            if (keys != null)
                foreach (Keys k in keys)
                    InternalSetKey(k);
        }

        /// <summary>
        /// Returns the state of a specified key.
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <returns>The state of the key.</returns>
        public KeyState this[Keys key]
        {
            get { return InternalGetKey(key) ? KeyState.Down : KeyState.Up; }
        }

        /// <summary>
        /// Gets whether given key is currently being pressed.
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <returns>true if the key is pressed; false otherwise.</returns>
        public bool IsKeyDown(Keys key)
        {
            return InternalGetKey(key);
        }

        /// <summary>
        /// Gets whether given key is currently being not pressed.
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <returns>true if the key is not pressed; false otherwise.</returns>
        public bool IsKeyUp(Keys key)
        {
            return !InternalGetKey(key);
        }

        #endregion


        #region GetPressedKeys()

        private static uint CountBits(uint v)
        {
            // http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
            v = v - ((v >> 1) & 0x55555555);                    // reuse input as temporary
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333);     // temp
            return ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
        }

        private static int AddKeysToArray(uint keys, int offset, Keys[] pressedKeys, int index)
        {
            for (int i = 0; i < 32; i++)
            {
                if ((keys & (1 << i)) != 0)
                    pressedKeys[index++] = (Keys)(offset + i);
            }
            return index;
        }

        /// <summary>
        /// Returns an array of values holding keys that are currently being pressed.
        /// </summary>
        /// <returns>The keys that are currently being pressed.</returns>
        public Keys[] GetPressedKeys()
        {
            uint count = CountBits(keys0) + CountBits(keys1) + CountBits(keys2) + CountBits(keys3)
                    + CountBits(keys4) + CountBits(keys5) + CountBits(keys6) + CountBits(keys7);
            if (count == 0)
                return empty;
            Keys[] keys = new Keys[count];

            int index = 0;
            if (keys0 != 0) index = AddKeysToArray(keys0, 0 * 32, keys, index);
            if (keys1 != 0) index = AddKeysToArray(keys1, 1 * 32, keys, index);
            if (keys2 != 0) index = AddKeysToArray(keys2, 2 * 32, keys, index);
            if (keys3 != 0) index = AddKeysToArray(keys3, 3 * 32, keys, index);
            if (keys4 != 0) index = AddKeysToArray(keys4, 4 * 32, keys, index);
            if (keys5 != 0) index = AddKeysToArray(keys5, 5 * 32, keys, index);
            if (keys6 != 0) index = AddKeysToArray(keys6, 6 * 32, keys, index);
            if (keys7 != 0) index = AddKeysToArray(keys7, 7 * 32, keys, index);

            return keys;
        }

        #endregion


        #region Objet and Equality

        /// <summary>
        /// Gets the hash code for <see cref="KeyboardState"/> instance.
        /// </summary>
        /// <returns>Hash code of the object.</returns>
        public override int GetHashCode()
        {
            return (int)(keys0 ^ keys1 ^ keys2 ^ keys3 ^ keys4 ^ keys5 ^ keys6 ^ keys7);
        }

        /// <summary>
        /// Compares whether two <see cref="KeyboardState"/> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="KeyboardState"/> instance to the left of the equality operator.</param>
        /// <param name="b"><see cref="KeyboardState"/> instance to the right of the equality operator.</param>
        /// <returns>true if the instances are equal; false otherwise.</returns>
        public static bool operator ==(KeyboardState a, KeyboardState b)
        {
            return a.keys0 == b.keys0
                && a.keys1 == b.keys1
                && a.keys2 == b.keys2
                && a.keys3 == b.keys3
                && a.keys4 == b.keys4
                && a.keys5 == b.keys5
                && a.keys6 == b.keys6
                && a.keys7 == b.keys7;
        }

        /// <summary>
        /// Compares whether two <see cref="KeyboardState"/> instances are not equal.
        /// </summary>
        /// <param name="a"><see cref="KeyboardState"/> instance to the left of the inequality operator.</param>
        /// <param name="b"><see cref="KeyboardState"/> instance to the right of the inequality operator.</param>
        /// <returns>true if the instances are different; false otherwise.</returns>
        public static bool operator !=(KeyboardState a, KeyboardState b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Compares whether current instance is equal to specified object.
        /// </summary>
        /// <param name="obj">The <see cref="KeyboardState"/> to compare.</param>
        /// <returns>true if the provided <see cref="KeyboardState"/> instance is same with current; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is KeyboardState && this == (KeyboardState)obj;
        }

        #endregion

    }
}
