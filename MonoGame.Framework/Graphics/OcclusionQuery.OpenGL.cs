// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if MONOMAC
using System.Runtime.InteropServices;
#if PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
#else
using OpenTK.Graphics.OpenGL;
#endif
#elif DESKTOPGL
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES30;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    partial class OcclusionQuery
    {
        private int glQueryId;

        private void PlatformConstruct()
        {
            GL.GenQueries(1, out glQueryId);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformBegin()
        {
#if GLES
            GL.BeginQuery(QueryTarget.AnySamplesPassed, glQueryId);
#else
            GL.BeginQuery(QueryTarget.SamplesPassed, glQueryId);
#endif
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformEnd()
        {
#if GLES
            GL.EndQuery(QueryTarget.AnySamplesPassed);
#else
            GL.EndQuery(QueryTarget.SamplesPassed);
#endif
            GraphicsExtensions.CheckGLError();
        }

        private bool PlatformGetResult(out int pixelCount)
        {
            int resultReady = 0;
#if MONOMAC
            GetQueryObjectiv(glQueryId, (int)GetQueryObjectParam.QueryResultAvailable, out resultReady);
#else
            GL.GetQueryObject(glQueryId, GetQueryObjectParam.QueryResultAvailable, out resultReady);
#endif
            GraphicsExtensions.CheckGLError();

            if (resultReady == 0)
            {
                pixelCount = 0;
                return false;
            }

#if MONOMAC
            GetQueryObjectiv(glQueryId, (int)GetQueryObjectParam.QueryResult, out pixelCount);
#elif OPENGL
            GL.GetQueryObject(glQueryId, GetQueryObjectParam.QueryResult, out pixelCount);
#endif
            GraphicsExtensions.CheckGLError();

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                Threading.BlockOnUIThread(() =>
                {
                    GL.DeleteQueries(1, ref glQueryId);
                    GraphicsExtensions.CheckGLError();
                });
            }

            base.Dispose(disposing);
        }

#if MONOMAC
        //MonoMac doesn't export this. Grr.
        const string OpenGLLibrary = "/System/Library/Frameworks/OpenGL.framework/OpenGL";
        
        [System.Security.SuppressUnmanagedCodeSecurity()]
        [DllImport(OpenGLLibrary, EntryPoint = "glGetQueryObjectiv", ExactSpelling = true)]
        extern static unsafe void GetQueryObjectiv(int id, int pname, out int @params);
#endif
    }
}

