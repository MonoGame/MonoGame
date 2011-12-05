using System;
using System.Runtime.InteropServices;

using MonoMac.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class DXShader
	{
		
		public ShaderType gl_shaderType;
		public int gl_shader;
		
		T[] UnmarshalArray<T>(IntPtr ptr, int count) {
			Type type = typeof(T);
			T[] ret = new T[count];
			for (int i=0; i<count; i++) {
				ret[i] = (T)Marshal.PtrToStructure (ptr, type);
				ptr = IntPtr.Add (ptr, Marshal.SizeOf (type));
			}
			return ret;
		}
		
		public DXShader (byte[] shaderData)
		{
			IntPtr parseDataPtr = MojoShader.NativeMethods.MOJOSHADER_parse(
					"glsl120",
					shaderData,
					shaderData.Length,
					IntPtr.Zero,
					0,
					IntPtr.Zero,
					IntPtr.Zero,
					IntPtr.Zero);
			
			try {
				MojoShader.MOJOSHADER_parseData parseData =
					(MojoShader.MOJOSHADER_parseData)Marshal.PtrToStructure(
						parseDataPtr,
						typeof(MojoShader.MOJOSHADER_parseData));
				
				if (parseData.error_count > 0) {
					MojoShader.MOJOSHADER_error[] errors =
						UnmarshalArray<MojoShader.MOJOSHADER_error>(
							parseData.errors, parseData.error_count);
					throw new Exception(errors[0].error);
				}
				MojoShader.MOJOSHADER_symbol[] symbols = 
					UnmarshalArray<MojoShader.MOJOSHADER_symbol>(
						parseData.symbols, parseData.symbol_count);
				
				
				switch(parseData.shader_type) {
				case MojoShader.MOJOSHADER_shaderType.MOJOSHADER_TYPE_PIXEL:
					gl_shaderType = ShaderType.FragmentShader;
					break;
				case MojoShader.MOJOSHADER_shaderType.MOJOSHADER_TYPE_VERTEX:
					gl_shaderType = ShaderType.VertexShader;
					break;
				default:
					throw new NotSupportedException();
				}
				
				
				gl_shader = GL.CreateShader (gl_shaderType);
				GL.ShaderSource (gl_shader, parseData.output);
				GL.CompileShader(gl_shader);
				
				int compiled = 0;
				GL.GetShader (gl_shader, ShaderParameter.CompileStatus, out compiled);
				if (compiled == (int)All.False) {
					throw new Exception("Shader Compilation Failed");
				}
				
			} finally {
				MojoShader.NativeMethods.MOJOSHADER_freeParseData(parseDataPtr);
			}
		}
	}
}

