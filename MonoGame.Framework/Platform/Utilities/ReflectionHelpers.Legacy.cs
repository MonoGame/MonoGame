using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Utilities
{
    internal static partial class ReflectionHelpers
    {
        /// <summary>
        /// Generics handler for Marshal.SizeOf
        /// </summary>
        internal static class SizeOf<T>
        {
            static int _sizeOf;

            static SizeOf()
            {
                var type = typeof(T);
                _sizeOf = Marshal.SizeOf(type);
            }

            static public int Get()
            {
                return _sizeOf;
            }
        }

        /// <summary>
        /// Fallback handler for Marshal.SizeOf(type)
        /// </summary>
        internal static int ManagedSizeOf(Type type)
        {
            return Marshal.SizeOf(type);
        }
    }
}
