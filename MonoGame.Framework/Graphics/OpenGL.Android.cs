// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Security;
using Android.Opengl;

namespace MonoGame.OpenGL
{
    internal partial class GL
    {
		// internal for Android is not used on other platforms
		// it allows us to use either GLES or Full GL (if the GPU supports it)
		internal delegate bool BindAPIDelegate (RenderApi api);
		internal static BindAPIDelegate BindAPI;

        static partial void LoadPlatformEntryPoints()
        {
            var ptr = EntryPointHelper.GetAddress("eglBindAPI");
            if (ptr != IntPtr.Zero)
                BindAPI = (BindAPIDelegate)Marshal.GetDelegateForFunctionPointer (ptr, typeof(BindAPIDelegate));
            var supportsFullGL = ptr != IntPtr.Zero && BindAPI (RenderApi.GL);
			if (!supportsFullGL) {
                if (ptr != IntPtr.Zero)
				    BindAPI (RenderApi.ES);
				BoundApi = RenderApi.ES;
			}
        }

        private static IGraphicsContext PlatformCreateContext (IWindowInfo info)
        {
            return null;//new GraphicsContext(info);
        }
    }

	internal static class EntryPointHelper {
		
		static IntPtr libES1 = DL.Open("libGLESv1_CM.so");
		static IntPtr libES2 = DL.Open("libGLESv2.so");
		static IntPtr libGL = DL.Open("libGL.so");
	
		public static IntPtr GetAddress(String function)
		{
            if (GL.BoundApi == GL.RenderApi.ES && libES2 != IntPtr.Zero)
			{
				return DL.Symbol(libES2, function);
			}
			else if (GL.BoundApi == GL.RenderApi.GL && libGL != IntPtr.Zero)
			{
				return DL.Symbol(libGL, function);
			}
			return IntPtr.Zero;
		}
	}
	
	
	internal class DL
	{
		internal enum DLOpenFlags
		{
			Lazy = 0x0001,
			Now = 0x0002,
			Global = 0x0100,
			Local = 0x0000,
		}

		const string lib = "dl";

		[DllImport(lib, EntryPoint = "dlopen")]
		internal static extern IntPtr Open(string filename, DLOpenFlags flags = DLOpenFlags.Lazy);

		[DllImport(lib, EntryPoint = "dlclose")]
		internal static extern int Close(IntPtr handle);

		[DllImport(lib, EntryPoint = "dlsym")]
		internal static extern IntPtr Symbol(IntPtr handle, String name);
	}
}

