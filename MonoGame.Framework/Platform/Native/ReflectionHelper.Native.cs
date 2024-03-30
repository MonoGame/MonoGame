// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Utilities;

internal static partial class ReflectionHelpers
{
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

    internal static int ManagedSizeOf(Type type)
    {
        return Marshal.SizeOf(type);
    }
}
