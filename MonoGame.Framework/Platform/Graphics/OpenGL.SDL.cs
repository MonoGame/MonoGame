// MonoGame - Copyright (C) MonoGame Foundation, Inc
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
#if GLES
            BoundApi = RenderApi.ES;
#else
            BoundApi = RenderApi.GL;
#endif
        }


        private static T LoadFunction<T>(string function, bool throwIfNotFound = false)
        {
            nint ret = Sdl.GL.GetProcAddress(function);

            if (ret == IntPtr.Zero)
            {
                if (throwIfNotFound)
                    throw new EntryPointNotFoundException(function);

                return default;
            }

            return Marshal.GetDelegateForFunctionPointer<T>(ret);
        }

        private static IGraphicsContext PlatformCreateContext (IWindowInfo info)
        {
            return new GraphicsContext(info);
        }
    }
}
