using System;
using System.Runtime.InteropServices;


#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif ANGLE
// Review and enable for GLES (ANDROID and IOS)
using OpenTK.Graphics.ES30;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class OcclusionQuery : GraphicsResource
	{
#if OPENGL
		private int glQueryId;
#endif

		public OcclusionQuery (GraphicsDevice graphicsDevice)
		{
			this.GraphicsDevice = graphicsDevice;
#if OPENGL
			GL.GenQueries (1, out glQueryId);
            GraphicsExtensions.CheckGLError();
#elif DIRECTX
#endif
		}

		public void Begin ()
		{
#if OPENGL
#if GLES 
			GL.BeginQuery (QueryTarget.AnySamplesPassed, glQueryId);
#else
			GL.BeginQuery (QueryTarget.SamplesPassed, glQueryId);
#endif
            GraphicsExtensions.CheckGLError();
#elif DIRECTX
#endif

		}

		public void End ()
		{
#if OPENGL
#if GLES 
			GL.EndQuery (QueryTarget.AnySamplesPassed);
#else
			GL.EndQuery (QueryTarget.SamplesPassed);
#endif
            GraphicsExtensions.CheckGLError();
#elif DIRECTX
#endif

		}

		protected override void Dispose(bool disposing)
		{
            if (!IsDisposed)
            {
#if OPENGL
                GraphicsDevice.AddDisposeAction(() =>
                    {
                        GL.DeleteQueries(1, ref glQueryId);
                        GraphicsExtensions.CheckGLError();
                    });
#elif DIRECTX
#endif
            }
            base.Dispose(disposing);
		}

		public bool IsComplete {
			get {
				int resultReady;
#if MONOMAC               
				GetQueryObjectiv(glQueryId,
				                 (int)GetQueryObjectParam.QueryResultAvailable,
				                 out resultReady);
#elif OPENGL
                GL.GetQueryObject(glQueryId, GetQueryObjectParam.QueryResultAvailable, out resultReady);
                GraphicsExtensions.CheckGLError();
#elif DIRECTX                
#endif
				return resultReady != 0;
			}
		}
		public int PixelCount {
			get {
				int result;
#if MONOMAC
				GetQueryObjectiv(glQueryId,
				                 (int)GetQueryObjectParam.QueryResult,
				                 out result);
#elif OPENGL
                GL.GetQueryObject(glQueryId, GetQueryObjectParam.QueryResultAvailable, out result);
                GraphicsExtensions.CheckGLError();
#elif DIRECTX             
#endif
                return result;
			}
        }

#if MONOMAC
		//MonoMac doesn't export this. Grr.
		const string OpenGLLibrary = "/System/Library/Frameworks/OpenGL.framework/OpenGL";

		[System.Security.SuppressUnmanagedCodeSecurity()]
		[DllImport(OpenGLLibrary, EntryPoint = "glGetQueryObjectiv", ExactSpelling = true)]
		extern static unsafe void GetQueryObjectiv(UInt32 id, int pname, [OutAttribute] Int32[] @params);
#endif
    }
}

