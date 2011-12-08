using System;
using System.Runtime.InteropServices;

using MonoMac.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class DXShader
	{
		
		public ShaderType shaderType;
		public int shader;
		
		string newOutput;
		
		float[] uniforms_float4;
		int[] uniforms_int4;
		int[] uniforms_bool;
		int uniforms_float4_count = 0;
		int uniforms_int4_count = 0;
		int uniforms_bool_count = 0;
		
		float[] posFixup = new float[4];
		
		MojoShader.MOJOSHADER_symbol[] symbols;
		MojoShader.MOJOSHADER_sampler[] samplers;
		
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
						DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_error>(
							parseData.errors, parseData.error_count);
					throw new Exception(errors[0].error);
				}
				
				if (parseData.preshader != IntPtr.Zero) {
					throw new NotImplementedException();
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
				
				symbols = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_symbol>(
						parseData.symbols, parseData.symbol_count);
				
				samplers = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_sampler>(
						parseData.samplers, parseData.sampler_count);
				
				MojoShader.MOJOSHADER_attribute[] attributes = 
					DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_attribute>(
						parseData.attributes, parseData.attribute_count);
				
				foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
					switch (symbol.register_set) {
					case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
						uniforms_bool_count += (int)symbol.register_count;
						break;
					case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:
						uniforms_float4_count += (int)symbol.register_count;
						break;
					case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
						uniforms_int4_count += (int)symbol.register_count;
						break;
					default:
						break;
					}
				}
				
				uniforms_float4 = new float[uniforms_float4_count*4];
				uniforms_int4 = new int[uniforms_int4_count*4];
				uniforms_bool = new int[uniforms_bool_count];
				
				newOutput = parseData.output;
				foreach (MojoShader.MOJOSHADER_attribute attrb in attributes) {
					if (shaderType == ShaderType.VertexShader) {
						switch (attrb.usage) {
						case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_COLOR:
							newOutput = newOutput.Replace ("attribute vec4 "+attrb.name+";", "")
								.Replace (attrb.name, "gl_Color");
							break;
						case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_POSITION:
							newOutput = newOutput.Replace ("attribute vec4 "+attrb.name+";", "")
								.Replace (attrb.name, "gl_Vertex");
							break;
						case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_TEXCOORD:
							newOutput = newOutput.Replace ("attribute vec4 "+attrb.name+";", "")
								.Replace (attrb.name, "gl_MultiTexCoord0");
							break;
						case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_NORMAL:
							newOutput = newOutput.Replace ("attribute vec4 "+attrb.name+";", "")
								.Replace (attrb.name, "gl_Normal");
							break;
						default:
							throw new NotImplementedException();
						}
					}
				}
				
				Console.WriteLine ( newOutput );
				
				shader = GL.CreateShader (shaderType);
				GL.ShaderSource (shader, newOutput);
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
		
		public void PopulateUniforms(EffectParameterCollection parameters, Viewport vp) {
			//TODO: not necessarily packed contiguously, get info from mojoshader somehow
			int bool_index = 0;
			int float4_index = 0;
			int int4_index = 0;
			
			//only populate modofied stuff?
			foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
				//todo: support array parameters
				EffectParameter parameter = parameters[symbol.name];
				switch (symbol.register_set) {
				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
					uniforms_bool[bool_index*4] = (int)parameter.data;
					bool_index += (int)symbol.register_count;
					break;
				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:
					switch (parameter.ParameterClass) {
					case EffectParameterClass.Scalar:
						uniforms_float4[float4_index*4] = (float)parameter.data;
						break;
					case EffectParameterClass.Vector:
					case EffectParameterClass.Matrix:
						for (int i=0; i<parameter.RowCount*parameter.ColumnCount; i++) {
							uniforms_float4[float4_index*4+i] = ((float[])parameter.data)[i];
						}
						break;
					default:
						throw new NotImplementedException();
						//break;
					}
					float4_index += (int)symbol.register_count;
					break;
				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
					for (int i=0; i<4; i++) {
						uniforms_int4[int4_index+i] = ((int[])parameter.data)[i];
					}
					int4_index += (int)symbol.register_count;
					break;
				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER:
					break; //handled by ActivateTextures
				default:
					throw new NotImplementedException();
				}
			}
			
			//calc data for directx projection fix
			if (shaderType == ShaderType.VertexShader) {
				posFixup[0] = 1.0f;
				posFixup[1] = 1.0f;
				posFixup[2] = (63.0f / 64.0f) / vp.Width;
				posFixup[3] = -(63.0f / 64.0f) / vp.Height;
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
			
			//Send data for directx projection fix
			if (shaderType == ShaderType.VertexShader) {
				int posFixup_loc = GL.GetUniformLocation (program, "posFixup");
				GL.Uniform4 (posFixup_loc, 1, posFixup);
			}
		}
		
		public void ActivateTextures(uint program, TextureCollection textures) {
			foreach (MojoShader.MOJOSHADER_sampler sampler in samplers) {
				int loc = GL.GetUniformLocation (program, sampler.name);
				
				//set the sampler texture slot
				GL.Uniform1 (loc, sampler.index);
				
				if (sampler.type == MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_2D) {
					//are smapler indexes normal texture indexes?
					
					if (sampler.index >= textures._textures.Count) {
						continue;
					}
					
					Texture tex = textures [sampler.index];
					GL.ActiveTexture( (TextureUnit)((int)TextureUnit.Texture0 + sampler.index) );
					
					//just to tex.Apply() instead?
					GL.BindTexture (TextureTarget.Texture2D, tex._textureId);
					
				}

			}
		}
		
	}
}

