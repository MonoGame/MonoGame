// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace MonoGame.OpenGL
{
    internal partial class GL
    {
        static partial void LoadPlatformEntryPoints()
        {
            BoundApi = RenderApi.GL;
        }

        private static IGraphicsContext PlatformCreateContext (IWindowInfo info)
        {
            return new GraphicsContext(info);
        }
    }

    internal class EntryPointHelper
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Sdl.NativeLibName, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "SDL_GL_GetProcAddress", ExactSpelling = true)]
        internal static extern IntPtr GetProcAddress(IntPtr proc);
        internal static IntPtr GetAddress(string proc)
        {
            IntPtr p = Marshal.StringToHGlobalAnsi(proc);
            try
            {
                var addr = GetProcAddress(p);
                if (addr == IntPtr.Zero)
                    throw new EntryPointNotFoundException (proc);
                return addr;
            }
            finally
            {
                Marshal.FreeHGlobal(p);
            }
        }
    }
}

