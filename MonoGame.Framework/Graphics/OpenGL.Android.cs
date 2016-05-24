// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace OpenGL
{
    public partial class GL
    {
		// internal for Android is not used on other platforms
		// it allows us to use either GLES or Full GL (if the GPU supports it)
		internal delegate bool BindAPIDelegate (RenderApi api);
		internal static BindAPIDelegate BindAPI;

        static partial void LoadPlatformEntryPoints()
        {
            BindAPI = (BindAPIDelegate)Marshal.GetDelegateForFunctionPointer (EntryPointHelper.GetAddress ("eglBindAPI"), typeof(BindAPIDelegate));
			var supportsFullGL = BindAPI (RenderApi.GL);
			if (!supportsFullGL) {
				BindAPI (RenderApi.ES);
				BoundApi = RenderApi.ES;
			}
        }

        private static IGraphicsContext PlatformCreateContext (IWindowInfo info)
        {
            return new GraphicsContext(info);
        }
    }

	internal static class EntryPointHelper {
		
		static IntPtr libES1 = DL.Open("/system/lib/libGLESv1_CM.so");
		static IntPtr libES2 = DL.Open("/system/lib/libGLESv2.so");
		static IntPtr libGL = DL.Open("/system/lib/libGL.so");
	
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

		const string lib = "/system/lib/libdl.so";

		[DllImport(lib, EntryPoint = "dlopen")]
		internal static extern IntPtr Open(string filename, DLOpenFlags flags = DLOpenFlags.Lazy);

		[DllImport(lib, EntryPoint = "dlclose")]
		internal static extern int Close(IntPtr handle);

		[DllImport(lib, EntryPoint = "dlsym")]
		internal static extern IntPtr Symbol(IntPtr handle, String name);
	}
}

