// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Holds the state of keystrokes by a keyboard.
    /// </summary>
	public struct KeyboardState
    {
        private const byte CapsLockToggle = 1;
        private const byte NumLockToggle = 2;

        /// <summary>
        /// Returns a <see cref="KeyboardState"/> with no keys or modifiers set.
        /// </summary>
        public static KeyboardState Empty = default;

        // Used for the common situation where GetPressedKeys will return an empty array
        private static Keys[] empty = new Keys[0];

        // Used to mask out modifier keys when checking for any key presses
        private static KeyboardState modifiers = new KeyboardState(Keys.LeftShift, Keys.RightShift, Keys.LeftControl, Keys.RightControl, Keys.LeftAlt, Keys.RightAlt);

        #region Key Data

        // Array of 256 bits:
        private uint _keys0, _keys1, _keys2, _keys3, _keys4, _keys5, _keys6, _keys7;
        private byte _toggles;

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

        internal KeyboardState(uint keys0, uint keys1, uint keys2, uint keys3, uint keys4, uint keys5, uint keys6, uint keys7, byte toggles) : this()
        {
            _keys0 = keys0;
            _keys1 = keys1;
            _keys2 = keys2;
            _keys3 = keys3;
            _keys4 = keys4;
            _keys5 = keys5;
            _keys6 = keys6;
            _keys7 = keys7;
            _toggles = toggles;
        }

        internal KeyboardState(List<Keys> keys, bool capsLock = false, bool numLock = false) : this()
        {
            _toggles = (byte)(0 | (capsLock ? CapsLockToggle : 0) | (numLock ? NumLockToggle : 0));

            if (keys != null)
                for (var i = 0; i < keys.Count; i++)
                    InternalSetKey(keys[i]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardState"/> class.
        /// </summary>
        /// <param name="keys">List of keys to be flagged as pressed on initialization.</param>
        /// <param name="capsLock">Caps Lock state.</param>
        /// <param name="numLock">Num Lock state.</param>
        public KeyboardState(Keys[] keys, bool capsLock = false, bool numLock = false) : this()
        {
            _toggles = (byte)(0 | (capsLock ? CapsLockToggle : 0) | (numLock ? NumLockToggle : 0));

            if (keys != null)
                for (var i = 0; i < keys.Length; i++)
                    InternalSetKey(keys[i]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardState"/> class.
        /// </summary>
        /// <param name="keys">List of keys to be flagged as pressed on initialization.</param>
        public KeyboardState(params Keys[] keys) : this()
        {
            if (keys != null)
                for (var i = 0; i < keys.Length; i++)
                    InternalSetKey(keys[i]);
        }

        /// <summary>
        /// Gets the current state of the Caps Lock key.
        /// </summary>
        public bool CapsLock
        {
            get
            {
                return (_toggles & CapsLockToggle) > 0;
            }
        }

        /// <summary>
        /// Gets the current state of the Num Lock key.
        /// </summary>
        public bool NumLock
        {
            get
            {
                return (_toggles & NumLockToggle) > 0;
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
        /// Gets whether any key is currently pressed.
        /// </summary>
        /// <param name="includeModifiers">Whether to include modifier keys in the query.</param>
        /// <returns>true if any key is pressed; false otherwise.</returns>
        public bool AnyKeyDown(bool includeModifiers = true)
        {
            if(includeModifiers)
                return (_keys0 | _keys1 | _keys2 | _keys3 | _keys4 | _keys5 | _keys6 | _keys7) != 0;
            else
                return (_keys0 & ~modifiers._keys0) != 0
                    || (_keys1 & ~modifiers._keys1) != 0
                    || (_keys2 & ~modifiers._keys2) != 0
                    || (_keys3 & ~modifiers._keys3) != 0
                    || (_keys4 & ~modifiers._keys4) != 0
                    || (_keys5 & ~modifiers._keys5) != 0
                    || (_keys6 & ~modifiers._keys6) != 0
                    || (_keys7 & ~modifiers._keys7) != 0;
        }

        /// <summary>
        /// Gets whether any of the specified keys are currently pressed.
        /// </summary>
        /// <param name="keys">The keys to query.</param>
        /// <param name="includeModifiers">Whether to include modifier keys in the query.</param>
        /// <returns>true if any of the specified keys are pressed; false otherwise.</returns>
        public bool AnyKeyDown(KeyboardState keys, bool includeModifiers = true)
        {
            return (this & keys).AnyKeyDown(includeModifiers);
        }

        /// <summary>
        /// Gets whether any key, excluding the specified keys, is currently pressed.
        /// </summary>
        /// <param name="keys">The keys to query.</param>
        /// <param name="includeModifiers">Whether to include modifier keys in the query.</param>
        /// <returns>true if any key, excluding the specified keys, is pressed; false otherwise.</returns>
        public bool AnyKeyDownExcept(KeyboardState keys, bool includeModifiers = true)
        {
            return (this & ~keys).AnyKeyDown(includeModifiers);
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

        /// <summary>
        /// Performs a bitwise AND operation between two <see cref="KeyboardState"/> instances.
        /// </summary>
        /// <param name="a"><see cref="KeyboardState"/> instance to the left of the bitwise AND operator.</param>
        /// <param name="b"><see cref="KeyboardState"/> instance to the right of the bitwise AND operator.</param>
        /// <returns>A <see cref="KeyboardState"/> containing only the keys and toggles present in both instances.</returns>
        public static KeyboardState operator &(KeyboardState a, KeyboardState b)
        {
            return new KeyboardState(
                a._keys0 & b._keys0,
                a._keys1 & b._keys1,
                a._keys2 & b._keys2,
                a._keys3 & b._keys3,
                a._keys4 & b._keys4,
                a._keys5 & b._keys5,
                a._keys6 & b._keys6,
                a._keys7 & b._keys7,
                (byte)(a._toggles & b._toggles)
            );
        }

        /// <summary>
        /// Performs a bitwise OR operation between two <see cref="KeyboardState"/> instances.
        /// </summary>
        /// <param name="a"><see cref="KeyboardState"/> instance to the left of the bitwise OR operator.</param>
        /// <param name="b"><see cref="KeyboardState"/> instance to the right of the bitwise OR operator.</param>
        /// <returns>A <see cref="KeyboardState"/> containing all the keys and toggles present in either instance.</returns>
        public static KeyboardState operator |(KeyboardState a, KeyboardState b)
        {
            return new KeyboardState(
                a._keys0 | b._keys0,
                a._keys1 | b._keys1,
                a._keys2 | b._keys2,
                a._keys3 | b._keys3,
                a._keys4 | b._keys4,
                a._keys5 | b._keys5,
                a._keys6 | b._keys6,
                a._keys7 | b._keys7,
                (byte)(a._toggles | b._toggles)
            );
        }

        /// <summary>
        /// Performs a bitwise XOR operation between two <see cref="KeyboardState"/> instances.
        /// </summary>
        /// <param name="a"><see cref="KeyboardState"/> instance to the left of the bitwise XOR operator.</param>
        /// <param name="b"><see cref="KeyboardState"/> instance to the right of the bitwise XOR operator.</param>
        /// <returns>A <see cref="KeyboardState"/> containing only the keys and toggles present in one instance but not both.</returns>
        public static KeyboardState operator ^(KeyboardState a, KeyboardState b)
        {
            return new KeyboardState(
                a._keys0 ^ b._keys0,
                a._keys1 ^ b._keys1,
                a._keys2 ^ b._keys2,
                a._keys3 ^ b._keys3,
                a._keys4 ^ b._keys4,
                a._keys5 ^ b._keys5,
                a._keys6 ^ b._keys6,
                a._keys7 ^ b._keys7,
                (byte)(a._toggles ^ b._toggles)
            );
        }

        /// <summary>
        /// Performs a bitwise NOT operation on a <see cref="KeyboardState"/> instance.
        /// </summary>
        /// <param name="obj"><see cref="KeyboardState"/> instance to perform the bitwise NOT operation on.</param>
        /// <returns>A <see cref="KeyboardState"/> with all the keys and toggles inverted.</returns>
        public static KeyboardState operator ~(KeyboardState obj)
        {
            return new KeyboardState(
                ~obj._keys0,
                ~obj._keys1,
                ~obj._keys2,
                ~obj._keys3,
                ~obj._keys4,
                ~obj._keys5,
                ~obj._keys6,
                ~obj._keys7,
                (byte)~obj._toggles
            );
        }

        #endregion

    }
}