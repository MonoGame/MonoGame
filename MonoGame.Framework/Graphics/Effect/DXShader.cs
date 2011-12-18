using System;
using System.Runtime.InteropServices;

using MonoMac.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class DXShader
	{
		
		public ShaderType shaderType;
		public int shader;
		
		string glslCode;
		
		float[] uniforms_float4;
		int[] uniforms_int4;
		int[] uniforms_bool;
		int uniforms_float4_count = 0;
		int uniforms_int4_count = 0;
		int uniforms_bool_count = 0;
		
		MojoShader.MOJOSHADER_symbol[] symbols;
		MojoShader.MOJOSHADER_sampler[] samplers;
		
		DXPreshader preshader;
		
		public DXShader (byte[] shaderData)
		{
			IntPtr parseDataPtr = MojoShader.NativeMethods.MOJOSHADER_parse(
					"glsl",
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
					preshader = new DXPreshader(parseData.preshader);
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
				
				Array.Sort (symbols, (a, b) => a.register_index.CompareTo(b.register_index));
				
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
				
				glslCode = parseData.output;
				
				//MojoShader wants us to handle vertex input attributes ourselves
				//to get around some directx fetures that opengl's entry points
				//don't support. We just hack around it for now
				foreach (MojoShader.MOJOSHADER_attribute attrb in attributes) {
					if (shaderType == ShaderType.VertexShader) {
						switch (attrb.usage) {
						case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_COLOR:
							glslCode = glslCode.Replace ("attribute vec4 "+attrb.name+";",
							                               "#define "+attrb.name+" gl_Color");
							break;
						case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_POSITION:
							glslCode = glslCode.Replace ("attribute vec4 "+attrb.name+";",
							                               "#define "+attrb.name+" gl_Vertex");
							break;
						case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_TEXCOORD:
							glslCode = glslCode.Replace ("attribute vec4 "+attrb.name+";",
							                               "#define "+attrb.name+" gl_MultiTexCoord0");
							break;
						case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_NORMAL:
							glslCode = glslCode.Replace ("attribute vec4 "+attrb.name+";",
							                               "#define "+attrb.name+" gl_Normal");
							break;
						default:
							throw new NotImplementedException();
						}
					}
				}
				
				glslCode = GLSLOptimizer.Optimize (glslCode, shaderType);
				
				shader = GL.CreateShader (shaderType);
				GL.ShaderSource (shader, glslCode);
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
		
		
		public void Apply(uint program,
		                  EffectParameterCollection parameters,
		                  Viewport vp,
		                  TextureCollection textures) {
			
			//Populate the uniform register arrays
			//TODO: not necessarily packed contiguously, get info from mojoshader somehow
			int bool_index = 0;
			int float4_index = 0;
			int int4_index = 0;
			
			//only populate modified stuff?
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
						for (int y=0; y<Math.Min (symbol.register_count, parameter.RowCount); y++) {
							for (int x=0; x<parameter.ColumnCount; x++) {
								uniforms_float4[(float4_index+y)*4+x] = ((float[])parameter.data)[y*4+x];
							}
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
			
			//execute the preshader
			if (preshader != null) {
				preshader.Run (parameters, uniforms_float4);
			}
			
			
			//Upload the uniforms
			
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
			
			if (shaderType == ShaderType.FragmentShader) {
				//activate textures
				foreach (MojoShader.MOJOSHADER_sampler sampler in samplers) {
					int loc = GL.GetUniformLocation (program, sampler.name);
					
					//set the sampler texture slot
					GL.Uniform1 (loc, sampler.index);
					
					if (sampler.type == MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_2D) {
						MojoShader.MOJOSHADER_symbol? samplerSymbol;
						foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
							if (symbol.register_set ==
							    	MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER &&
							    symbol.register_index == sampler.index) {
								
								samplerSymbol = symbol;
								break;
							}
						}
						
						Texture2D tex = null;
						if (samplerSymbol.HasValue) {
							DXEffectObject.d3dx_sampler samplerState =
								(DXEffectObject.d3dx_sampler)parameters[samplerSymbol.Value.name].data;
							if (samplerState.state_count > 0) {
								string textureName = samplerState.states[0].parameter.name;
								EffectParameter textureParameter = parameters[textureName];
								if (textureParameter != null) {
									tex = (Texture2D)textureParameter.data;
								}
							}
						}
						if (tex == null) {
							//texutre 0 will be set in drawbatch :/
							if (sampler.index == 0) {
								continue;
							}
							//are smapler indexes always normal texture indexes?
							tex = (Texture2D)textures [sampler.index];
						}

						GL.ActiveTexture( (TextureUnit)((int)TextureUnit.Texture0 + sampler.index) );
						
						//just to tex.Apply() instead?
						GL.BindTexture (TextureTarget.Texture2D, tex._textureId);
						
					}
	
				}
			}
		}
		
	}
}

