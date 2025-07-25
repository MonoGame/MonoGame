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
            nint ret = Sdl.GL.GL_GetProcAddress_Import(function);
            Console.WriteLine(function+" "+ret);

            if (ret == IntPtr.Zero)
            {
                if (throwIfNotFound)
                    throw new EntryPointNotFoundException(function);

                return default;
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
