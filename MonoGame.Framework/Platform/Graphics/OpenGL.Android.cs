// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using Android.Opengl;
using Javax.Microedition.Khronos.Egl;
using MonoGame.Framework.Utilities;

namespace MonoGame.OpenGL
{
    internal partial class GL
    {
		// internal for Android is not used on other platforms
		// it allows us to use either GLES or Full GL (if the GPU supports it)
		internal delegate bool BindAPIDelegate (RenderApi api);
		internal static BindAPIDelegate BindAPI;

        public static IntPtr Library;
        public static IntPtr libES1 = FuncLoader.LoadLibrary("libGLESv1_CM.so");
        public static IntPtr libES2 = FuncLoader.LoadLibrary("libGLESv2.so");
        public static IntPtr libES3 = FuncLoader.LoadLibrary("libGLESv3.so");
        public static IntPtr libGL = FuncLoader.LoadLibrary("libGL.so");

        static partial void LoadPlatformEntryPoints()
        {
            Android.Util.Log.Verbose("GL", "Loading Entry Points");

            var eglBindLoaded = false;
            try
            {
                BindAPI = FuncLoader.LoadFunction<BindAPIDelegate>(libGL, "eglBindAPI", true);
                eglBindLoaded = true;
            }
            catch { }

            var supportsFullGL = eglBindLoaded && BindAPI (RenderApi.GL);
            if (!supportsFullGL) {
                if (eglBindLoaded)
                    BindAPI (RenderApi.ES);
                BoundApi = RenderApi.ES;
            }
                
            Android.Util.Log.Verbose("GL", "Bound {0}", BoundApi);

            if (GL.BoundApi == GL.RenderApi.ES && libES3 != IntPtr.Zero)
                Library = libES3;

            if (GL.BoundApi == GL.RenderApi.ES && libES2 != IntPtr.Zero)
                Library = libES2;
            else if (GL.BoundApi == GL.RenderApi.GL && libGL != IntPtr.Zero)
                Library = libGL;
        }

        private static T LoadFunction<T>(string function, bool throwIfNotFound = false)
        {
            return FuncLoader.LoadFunction<T>(Library, function, throwIfNotFound);
        }

        private static IGraphicsContext PlatformCreateContext (IWindowInfo info)
        {
            return null;//new GraphicsContext(info);
        }
    }

    struct GLESVersion
    {
        const int EglContextClientVersion = 0x3098;
        const int EglContextMinorVersion = 0x30fb;

        public int Major;
        public int Minor;

        internal int[] GetAttributes()
        {
            int minor = Minor > -1 ? EglContextMinorVersion : EGL10.EglNone;
            return new int[] { EglContextClientVersion, Major, minor, Minor, EGL10.EglNone };
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", Major, Minor == -1 ? 0 : Minor);
        }

        internal static IEnumerable<GLESVersion> GetSupportedGLESVersions()
        {
            if (GL.libES3 != IntPtr.Zero)
            {
                yield return new GLESVersion { Major = 3, Minor = 2 };
                yield return new GLESVersion { Major = 3, Minor = 1 };
                yield return new GLESVersion { Major = 3, Minor = 0 };
            }
            if (GL.libES2 != IntPtr.Zero)
            {
                // We pass -1 becuase when requesting a GLES 2.0 context we
                // dont provide the Minor version.
                yield return new GLESVersion { Major = 2, Minor = -1 };
            }
            yield return new GLESVersion();
        }
    }
}
