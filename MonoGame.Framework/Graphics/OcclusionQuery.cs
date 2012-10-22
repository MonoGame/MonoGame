using System;
using System.Runtime.InteropServices;


#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class OcclusionQuery : GraphicsResource
	{
#if OPENGL
		private uint glQueryId;
#endif

		public OcclusionQuery (GraphicsDevice graphicsDevice)
		{
			this.graphicsDevice = graphicsDevice;
#if OPENGL
			GL.GenQueries (1, out glQueryId);
#elif DIRECTX
#endif
		}

		public void Begin ()
		{
#if OPENGL
			GL.BeginQuery (QueryTarget.SamplesPassed, glQueryId);
#elif DIRECTX
#endif

		}

		public void End ()
		{
#if OPENGL
			GL.EndQuery (QueryTarget.SamplesPassed);
#elif DIRECTX
#endif

		}

		public override void Dispose ()
		{
#if OPENGL
			GL.DeleteQueries (1, ref glQueryId);
#elif DIRECTX
#endif

		}

		public bool IsComplete {
			get {
				int[] resultReady = {0};
#if MONOMAC               
				GetQueryObjectiv(glQueryId,
				                 (int)GetQueryObjectParam.QueryResultAvailable,
				                 resultReady);
#elif OPENGL
                GL.GetQueryObject(glQueryId, GetQueryObjectParam.QueryResultAvailable, resultReady);
#elif DIRECTX                
#endif
				return resultReady[0] != 0;
			}
		}
		public int PixelCount {
			get {
				int[] result = {0};
#if MONOMAC
				GetQueryObjectiv(glQueryId,
				                 (int)GetQueryObjectParam.QueryResult,
				                 result);
#elif OPENGL
                GL.GetQueryObject(glQueryId, GetQueryObjectParam.QueryResultAvailable, result);
#elif DIRECTX             
#endif
                return result[0];
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

