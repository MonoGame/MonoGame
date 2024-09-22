// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Utilities;

internal static partial class ReflectionHelpers
{
    // This helper caches the Marshal.SizeOf result
    // as it generates an allocation on each call.
    [Obsolete("This should be made private and use FastSizeOf<T>() below instead!")]
    internal static class SizeOf<T>
    {
        static int _sizeOf;

        static SizeOf()
        {
            _sizeOf = Marshal.SizeOf<T>();
        }

        public static int Get()
        {
            return _sizeOf;
        }
    }

    // Returns the size of the unmanaged type in bytes.
    internal static int FastSizeOf<T>()
    {
        return SizeOf<T>.Get();
    }

    internal static int ManagedSizeOf(Type type)
    {
        return Marshal.SizeOf(type);
    }
}
