// MonoGame - Copyright (C) MonoGame Foundation, Inc
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
        private const byte CapsLockModifier = 1;
        private const byte NumLockModifier = 2;

        // Used for the common situation where GetPressedKeys will return an empty array
        private static Keys[] empty = new Keys[0];

        #region Key Data

        // Array of 256 bits:
        private uint _keys0, _keys1, _keys2, _keys3, _keys4, _keys5, _keys6, _keys7;
        private byte _modifiers;

        bool InternalGetKey(Keys key)
        {
            uint mask = (uint)1 << (((int)key) & 0x1f);

            uint element;
            switch (((int)key) >> 5)
            {
                case 0: element = _keys0; break;
                case 1: element = _keys1; break;
                case 2: element = _keys2; break;
                case 3: element = _keys3; break;
                case 4: element = _keys4; break;
                case 5: element = _keys5; break;
                case 6: element = _keys6; break;
                case 7: element = _keys7; break;
                default: element = 0; break;
            }

            return (element & mask) != 0;
        }

        internal void InternalSetKey(Keys key)
        {
            uint mask = (uint)1 << (((int)key) & 0x1f);
            switch (((int)key) >> 5)
            {
                case 0: _keys0 |= mask; break;
                case 1: _keys1 |= mask; break;
                case 2: _keys2 |= mask; break;
                case 3: _keys3 |= mask; break;
                case 4: _keys4 |= mask; break;
                case 5: _keys5 |= mask; break;
                case 6: _keys6 |= mask; break;
                case 7: _keys7 |= mask; break;
            }
        }

        internal void InternalClearKey(Keys key)
        {
            uint mask = (uint)1 << (((int)key) & 0x1f);
            switch (((int)key) >> 5)
            {
                case 0: _keys0 &= ~mask; break;
                case 1: _keys1 &= ~mask; break;
                case 2: _keys2 &= ~mask; break;
                case 3: _keys3 &= ~mask; break;
                case 4: _keys4 &= ~mask; break;
                case 5: _keys5 &= ~mask; break;
                case 6: _keys6 &= ~mask; break;
                case 7: _keys7 &= ~mask; break;
            }
        }

        internal void InternalClearAllKeys()
        {
            _keys0 = 0;
            _keys1 = 0;
            _keys2 = 0;
            _keys3 = 0;
            _keys4 = 0;
            _keys5 = 0;
            _keys6 = 0;
            _keys7 = 0;
        }

        #endregion


        #region XNA Interface

        internal KeyboardState(List<Keys> keys, bool capsLock = false, bool numLock = false) : this()
        {
            _keys0 = 0;
            _keys1 = 0;
            _keys2 = 0;
            _keys3 = 0;
            _keys4 = 0;
            _keys5 = 0;
            _keys6 = 0;
            _keys7 = 0;
            _modifiers = (byte)(0 | (capsLock ? CapsLockModifier : 0) | (numLock ? NumLockModifier : 0));

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
            _keys0 = 0;
            _keys1 = 0;
            _keys2 = 0;
            _keys3 = 0;
            _keys4 = 0;
            _keys5 = 0;
            _keys6 = 0;
            _keys7 = 0;
            _modifiers = (byte)(0 | (capsLock ? CapsLockModifier : 0) | (numLock ? NumLockModifier : 0));

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
            _keys0 = 0;
            _keys1 = 0;
            _keys2 = 0;
            _keys3 = 0;
            _keys4 = 0;
            _keys5 = 0;
            _keys6 = 0;
            _keys7 = 0;
            _modifiers = 0;

            if (keys != null)
                foreach (Keys k in keys)
                    InternalSetKey(k);
        }

        /// <summary>
        /// Gets the current state of the Caps Lock key.
        /// </summary>
        public bool CapsLock
        {
            get
            {
                return (_modifiers & CapsLockModifier) > 0;
            }
        }

        /// <summary>
        /// Gets the current state of the Num Lock key.
        /// </summary>
        public bool NumLock
        {
            get
            {
                return (_modifiers & NumLockModifier) > 0;
            }
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

        /// <summary>
        /// Returns the number of pressed keys in this <see cref="KeyboardState"/>.
        /// </summary>
        /// <returns>An integer representing the number of keys currently pressed in this <see cref="KeyboardState"/>.</returns>
        public int GetPressedKeyCount()
        {
            uint count = CountBits(_keys0) + CountBits(_keys1) + CountBits(_keys2) + CountBits(_keys3)
                    + CountBits(_keys4) + CountBits(_keys5) + CountBits(_keys6) + CountBits(_keys7);
            return (int)count;
        }

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
            uint count = CountBits(_keys0) + CountBits(_keys1) + CountBits(_keys2) + CountBits(_keys3)
                    + CountBits(_keys4) + CountBits(_keys5) + CountBits(_keys6) + CountBits(_keys7);
            if (count == 0)
                return empty;
            Keys[] keys = new Keys[count];

            int index = 0;
            if (_keys0 != 0) index = AddKeysToArray(_keys0, 0 * 32, keys, index);
            if (_keys1 != 0) index = AddKeysToArray(_keys1, 1 * 32, keys, index);
            if (_keys2 != 0) index = AddKeysToArray(_keys2, 2 * 32, keys, index);
            if (_keys3 != 0) index = AddKeysToArray(_keys3, 3 * 32, keys, index);
            if (_keys4 != 0) index = AddKeysToArray(_keys4, 4 * 32, keys, index);
            if (_keys5 != 0) index = AddKeysToArray(_keys5, 5 * 32, keys, index);
            if (_keys6 != 0) index = AddKeysToArray(_keys6, 6 * 32, keys, index);
            if (_keys7 != 0) index = AddKeysToArray(_keys7, 7 * 32, keys, index);

            return keys;
        }

        /// <summary>
        /// Fills an array of values holding keys that are currently being pressed.
        /// </summary>
        /// <param name="keys">The keys array to fill.
        /// This array is not cleared, and it must be equal to or larger than the number of keys pressed.</param>
        public void GetPressedKeys(Keys[] keys)
        {
            if (keys == null)
                throw new System.ArgumentNullException("keys");

            uint count = CountBits(_keys0) + CountBits(_keys1) + CountBits(_keys2) + CountBits(_keys3)
                    + CountBits(_keys4) + CountBits(_keys5) + CountBits(_keys6) + CountBits(_keys7);
            if (count > keys.Length)
            {
                throw new System.ArgumentOutOfRangeException("keys",
                    "The supplied array cannot fit the number of pressed keys. Call GetPressedKeyCount() to get the number of pressed keys.");
            }

            int index = 0;
            if (_keys0 != 0 && index < keys.Length) index = AddKeysToArray(_keys0, 0 * 32, keys, index);
            if (_keys1 != 0 && index < keys.Length) index = AddKeysToArray(_keys1, 1 * 32, keys, index);
            if (_keys2 != 0 && index < keys.Length) index = AddKeysToArray(_keys2, 2 * 32, keys, index);
            if (_keys3 != 0 && index < keys.Length) index = AddKeysToArray(_keys3, 3 * 32, keys, index);
            if (_keys4 != 0 && index < keys.Length) index = AddKeysToArray(_keys4, 4 * 32, keys, index);
            if (_keys5 != 0 && index < keys.Length) index = AddKeysToArray(_keys5, 5 * 32, keys, index);
            if (_keys6 != 0 && index < keys.Length) index = AddKeysToArray(_keys6, 6 * 32, keys, index);
            if (_keys7 != 0 && index < keys.Length) index = AddKeysToArray(_keys7, 7 * 32, keys, index);
        }

        #endregion


        #region Object and Equality

        /// <summary>
        /// Gets the hash code for <see cref="KeyboardState"/> instance.
        /// </summary>
        /// <returns>Hash code of the object.</returns>
        public override int GetHashCode()
        {
            return (int)(_keys0 ^ _keys1 ^ _keys2 ^ _keys3 ^ _keys4 ^ _keys5 ^ _keys6 ^ _keys7);
        }

        /// <summary>
        /// Compares whether two <see cref="KeyboardState"/> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="KeyboardState"/> instance to the left of the equality operator.</param>
        /// <param name="b"><see cref="KeyboardState"/> instance to the right of the equality operator.</param>
        /// <returns>true if the instances are equal; false otherwise.</returns>
        public static bool operator ==(KeyboardState a, KeyboardState b)
        {
            return a._keys0 == b._keys0
                && a._keys1 == b._keys1
                && a._keys2 == b._keys2
                && a._keys3 == b._keys3
                && a._keys4 == b._keys4
                && a._keys5 == b._keys5
                && a._keys6 == b._keys6
                && a._keys7 == b._keys7;
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
