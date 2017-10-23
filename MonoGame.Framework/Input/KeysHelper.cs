using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    internal static class KeysHelper
    {
        static Dictionary<int, Keys> _map;

        static KeysHelper()
        {
            _map = new Dictionary<int, Keys>();
            var allKeys = (Keys[])Enum.GetValues(typeof(Keys));
            foreach (var key in allKeys)
            {
                _map.Add((int)key, (key));
            }
        }

        /// <summary>
        /// Checks if specified value is valid Key.
        /// </summary>
        /// <param name="value">Keys base value</param>
        /// <returns>Returns true if value is valid Key, false otherwise</returns>
        public static bool IsKey(int value)
        {
            return _map.ContainsKey(value);
        }
    }
}
