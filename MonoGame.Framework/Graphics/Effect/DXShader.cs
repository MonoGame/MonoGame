using System;
using System.Runtime.InteropServices;

using MonoMac.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class DXShader
	{
		
		public ShaderType shaderType;
		public int shader;
		
		float[] uniforms_float4;
		int[] uniforms_int4;
		int[] uniforms_bool;
		int uniforms_float4_count = 0;
		int uniforms_int4_count = 0;
		int uniforms_bool_count = 0;
		
		MojoShader.MOJOSHADER_symbol[] symbols;
		
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
				
				
				switch(parseData.shader_type) {
				case MojoShader.MOJOSHADER_shaderType.MOJOSHADER_TYPE_PIXEL:
					shaderType = ShaderType.FragmentShader;
					break;
				case MojoShader.MOJOSHADER_shaderType.MOJOSHADER_TYPE_VERTEX:
					shaderType = ShaderType.VertexShader;
					break;
				default:
					throw new NotSupportedException();
				}
				
				
				symbols = UnmarshalArray<MojoShader.MOJOSHADER_symbol>(
						parseData.symbols, parseData.symbol_count);
				
				
				foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
					switch (symbol.register_set) {
					case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
						uniforms_bool_count = Math.Max (
							uniforms_bool_count, (int)(uniforms_bool_count+symbol.register_count));
						break;
					case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:
						uniforms_float4_count = Math.Max (
							uniforms_float4_count, (int)(uniforms_float4_count+symbol.register_count));
						break;
					case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
						uniforms_int4_count = Math.Max (
							uniforms_int4_count, (int)(uniforms_int4_count+symbol.register_count));
						break;
					default:
						break;
					}
				}
				
				uniforms_float4 = new float[uniforms_float4_count*4];
				uniforms_int4 = new int[uniforms_int4_count*4];
				uniforms_bool = new int[uniforms_bool_count];
				
				Console.WriteLine ( parseData.output );
				
				shader = GL.CreateShader (shaderType);
				GL.ShaderSource (shader, parseData.output);
				GL.CompileShader(shader);
				
				int compiled = 0;
				GL.GetShader (shader, ShaderParameter.CompileStatus, out compiled);
				if (compiled == (int)All.False) {
					throw new Exception("Shader Compilation Failed");
				}
				
			} finally {
				MojoShader.NativeMethods.MOJOSHADER_freeParseData(parseDataPtr);
			}
		}
		
		public void PopulateUniforms(EffectParameterCollection parameters) {
			//only populate modofied stuff?
			foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
				//todo: support array parameters
				EffectParameter parameter = parameters[symbol.name];
				switch (symbol.register_set) {
				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
					uniforms_bool[symbol.register_index] = (int)parameter.data;
					break;
				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:
					switch (parameter.ParameterClass) {
					case EffectParameterClass.Scalar:
						uniforms_float4[symbol.register_index*4] = (float)parameter.data;
						break;
					case EffectParameterClass.Vector:
					case EffectParameterClass.Matrix:
						for (int i=0; i<parameter.RowCount*parameter.ColumnCount; i++) {
							uniforms_float4[symbol.register_index*4+i] = ((float[])parameter.data)[i];
						}
						break;
					default:
						break;
					}
					break;
				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
					for (int i=0; i<4; i++) {
						uniforms_int4[symbol.register_index+i] = ((int[])parameter.data)[i];
					}
					break;
				default:
					break;
				}
			}
		}
		
		public void UploadUniforms(uint program) {
			string prefix;
			switch(shaderType) {
			case ShaderType.FragmentShader:
				prefix = "ps";
				break;
			case ShaderType.VertexShader:
				prefix = "vs";
				break;
			default:
				throw new NotImplementedException();
			}
				
				
			if (uniforms_float4_count > 0) {
				int vec4_loc = GL.GetUniformLocation (program, prefix+"_uniforms_vec4");
				GL.Uniform4 (vec4_loc, uniforms_float4_count, uniforms_float4);
			}
			if (uniforms_int4_count > 0) {
				int int4_loc = GL.GetUniformLocation (program, prefix+"_uniforms_ivec4");
				GL.Uniform4 (int4_loc, uniforms_int4_count, uniforms_int4);
			}
			if (uniforms_bool_count > 0) {
				int bool_loc = GL.GetUniformLocation (program, prefix+"_uniforms_bool");
				GL.Uniform1 (bool_loc, uniforms_bool_count, uniforms_bool);
			}
		}
		
	}
}

