// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;

namespace MonoGame.OpenGL
{
    partial class GL
    {
        static partial void LoadPlatformEntryPoints()
        {
            BoundApi = RenderApi.GL;
        }

        private static T LoadFunction<T>(string function, bool throwIfNotFound = false)
        {
            var ret = Sdl.GL.GetProcAddress(function);

            if (ret == IntPtr.Zero)
            {
                if (throwIfNotFound)
                    throw new EntryPointNotFoundException(function);

                return default(T);
            }

#if NETSTANDARD
            return Marshal.GetDelegateForFunctionPointer<T>(ret);
#else
            return (T)(object)Marshal.GetDelegateForFunctionPointer(ret, typeof(T));
#endif
        }

        private static IGraphicsContext PlatformCreateContext (IWindowInfo info)
        {
            return new GraphicsContext(info);
        }
    }
}

