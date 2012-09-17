using System;
using System.Runtime.InteropServices;


#if MAC
using MonoMac.OpenGL;
#elif OPENGL
using OpenTK.Graphics.OpenGL;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class OcclusionQuery : GraphicsResource
	{
		private uint glQueryId;

		public OcclusionQuery (GraphicsDevice graphicsDevice)
		{
			this.graphicsDevice = graphicsDevice;

			GL.GenQueries (1, out glQueryId);
		}

		public void Begin ()
		{
			GL.BeginQuery (QueryTarget.SamplesPassed, glQueryId);
		}

		public void End ()
		{
			GL.EndQuery (QueryTarget.SamplesPassed);
		}

		public override void Dispose ()
		{
			GL.DeleteQueries (1, ref glQueryId);
		}

		public bool IsComplete {
			get {
				int[] resultReady = {0};
				GetQueryObjectiv(glQueryId,
				                 (int)GetQueryObjectParam.QueryResultAvailable,
				                 resultReady);
				return resultReady[0] != 0;
			}
		}
		public int PixelCount {
			get {
				int[] result = {0};
				GetQueryObjectiv(glQueryId,
				                 (int)GetQueryObjectParam.QueryResult,
				                 result);
				return result[0];
			}
		}

		//MonoMac doesn't export this. Grr.
		const string OpenGLLibrary = "/System/Library/Frameworks/OpenGL.framework/OpenGL";

		[System.Security.SuppressUnmanagedCodeSecurity()]
		[DllImport(OpenGLLibrary, EntryPoint = "glGetQueryObjectiv", ExactSpelling = true)]
		extern static unsafe void GetQueryObjectiv(UInt32 id, int pname, [OutAttribute] Int32[] @params);
	}
}

