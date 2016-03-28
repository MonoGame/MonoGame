using System;
using System.Runtime.InteropServices;
using System.Security;

namespace OpenGL
{
    public partial class GL
    {
        static partial void LoadPlatformEntryPoints()
        {
            BoundApi = RenderApi.GL;
        }


        static partial void CreateContext (ref IGraphicsContext context)
        {
            context = null;
        }
    }

    internal class EntryPointHelper {

        private const string NativeLibName = "SDL2.dll";

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "SDL_GL_GetProcAddress", ExactSpelling = true)]
        public static extern IntPtr GetProcAddress(IntPtr proc);
        public static IntPtr GetAddress(string proc)
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

