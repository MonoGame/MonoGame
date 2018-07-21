// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using Android.Opengl;
using Javax.Microedition.Khronos.Egl;
using MonoGame.Utilities;

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
            Android.Util.Log.Verbose("GL", "Loading Entry Points");
            BindAPI = FuncLoader.LoadFunction<BindAPIDelegate>(GL.Library, "eglBindAPI");
            var supportsFullGL = BindAPI(RenderApi.GL);
			if (!supportsFullGL) {
                BindAPI (RenderApi.ES);
				BoundApi = RenderApi.ES;
			}
            Android.Util.Log.Verbose("GL", "Bound {0}", BoundApi);
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
            var lib = GL.Library;

            if (GL.SupportedVersion == 3)
            {
                yield return new GLESVersion { Major = 3, Minor = 2 };
                yield return new GLESVersion { Major = 3, Minor = 1 };
                yield return new GLESVersion { Major = 3, Minor = 0 };
            }
            if (GL.SupportedVersion == 2)
            {
                // We pass -1 becuase when requesting a GLES 2.0 context we 
                // dont provide the Minor version.
                yield return new GLESVersion { Major = 2, Minor = -1 };
            }
            yield return new GLESVersion();
        }
    }
}

