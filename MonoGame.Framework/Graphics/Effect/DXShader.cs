using System;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#else
using System.Text;
using OpenTK.Graphics.ES20;
#if IPHONE || ANDROID
using ShaderType = OpenTK.Graphics.ES20.All;
using ShaderParameter = OpenTK.Graphics.ES20.All;
using TextureUnit = OpenTK.Graphics.ES20.All;
using TextureTarget = OpenTK.Graphics.ES20.All;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	internal class DXShader
	{
		public ShaderType shaderType;
		public int shaderHandle;
		
		string glslCode;
		
		float[] uniforms_float4;
		int[] uniforms_int4;
		int[] uniforms_bool;
		int uniforms_float4_count = 0;
		int uniforms_int4_count = 0;
		int uniforms_bool_count = 0;
		
		MojoShader.MOJOSHADER_symbol[] symbols;
		MojoShader.MOJOSHADER_sampler[] samplers;
		MojoShader.MOJOSHADER_attribute[] attributes;
		
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
			
			//try to put the symbols in the order they are eventually packed into the uniform arrays
			//this /should/ be done by pulling the info from mojoshader
			Array.Sort (symbols, delegate (MojoShader.MOJOSHADER_symbol a, MojoShader.MOJOSHADER_symbol b) {
				uint va = a.register_index;
				if (a.info.elements == 1) va += 1024; //hax. mojoshader puts array objects first
				uint vb = b.register_index;
				if (b.info.elements == 1) vb += 1024;
				return va.CompareTo(vb);
			});//(a, b) => ((int)(a.info.elements > 1))a.register_index.CompareTo(b.register_index));
			
			samplers = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_sampler>(
					parseData.samplers, parseData.sampler_count);
			
			attributes = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_attribute>(
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
			
#if GLSLOPTIMIZER
			//glslCode = GLSLOptimizer.Optimize (glslCode, shaderType);
#endif
			
#if IPHONE || ANDROID
			glslCode = glslCode.Replace("#version 110\n", "");
			glslCode = "precision mediump float;\nprecision mediump int;\n" + glslCode;
#endif
            Threading.Begin();
            try
            {
                shaderHandle = GL.CreateShader(shaderType);
#if IPHONE || ANDROID
                GL.ShaderSource(shaderHandle, 1, new string[] { glslCode }, (int[])null);
#else			
                GL.ShaderSource(shaderHandle, glslCode);
#endif
                GL.CompileShader(shaderHandle);

                int compiled = 0;
#if IPHONE || ANDROID
                GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, ref compiled);
#else
                GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out compiled);
#endif
                if (compiled == (int)All.False)
                {
#if IPHONE || ANDROID
                    string log = "";
                    int length = 0;
                    GL.GetShader(shaderHandle, ShaderParameter.InfoLogLength, ref length);
                    if (length > 0)
                    {
                        var logBuilder = new StringBuilder(length);
                        GL.GetShaderInfoLog(shaderHandle, length, ref length, logBuilder);
                        log = logBuilder.ToString();
                    }
#else
                    string log = GL.GetShaderInfoLog(shaderHandle);
#endif
                    Console.WriteLine(log);

                    GL.DeleteShader(shaderHandle);
                    throw new InvalidOperationException("Shader Compilation Failed");
                }
            }
            finally
            {
                Threading.End();
            }
            //MojoShader.NativeMethods.MOJOSHADER_freeParseData(parseDataPtr);
			//TODO: dispose properly - DXPreshader holds unmanaged data
		}

		public void OnLink(int program) {
			if (shaderType == ShaderType.VertexShader) {
				//bind attributes
				foreach (MojoShader.MOJOSHADER_attribute attrb in attributes) {
					switch (attrb.usage) {
					case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_COLOR:
						GL.BindAttribLocation(program, GraphicsDevice.attributeColor, attrb.name);
						break;
					case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_POSITION:
						GL.BindAttribLocation(program, GraphicsDevice.attributePosition + attrb.index, attrb.name);
						break;
					case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_TEXCOORD:
						GL.BindAttribLocation(program, GraphicsDevice.attributeTexCoord + attrb.index, attrb.name);
						break;
					case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_NORMAL:
						GL.BindAttribLocation(program, GraphicsDevice.attributeNormal, attrb.name);
						break;
					case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_BLENDINDICES:
						GL.BindAttribLocation(program, GraphicsDevice.attributeBlendIndicies, attrb.name);
						break;
					case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_BLENDWEIGHT:
						GL.BindAttribLocation(program, GraphicsDevice.attributeBlendWeight, attrb.name);
						break;
					default:
						throw new NotImplementedException();
					}
				}
			}
		}
		
		public void Apply(int program,
		                  EffectParameterCollection parameters,
		                  GraphicsDevice graphicsDevice) {
			
			Viewport vp = graphicsDevice.Viewport;
			TextureCollection textures = graphicsDevice.Textures;
			SamplerStateCollection samplerStates = graphicsDevice.SamplerStates;
			
			//Populate the uniform register arrays
			//TODO: not necessarily packed contiguously, get info from mojoshader somehow
			int bool_index = 0;
			int float4_index = 0;
			int int4_index = 0;
			
			//TODO: only populate modified stuff?
			foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
				//todo: support array parameters
				EffectParameter parameter = parameters[symbol.name];
				switch (symbol.register_set) {
				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
					if (parameter.Elements.Count > 0) {
						throw new NotImplementedException();
					}
					uniforms_bool[bool_index*4] = (int)parameter.data;
					bool_index += (int)symbol.register_count;
					break;
				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:
					
					Single[] data = parameter.GetValueSingleArray();
					
					switch (parameter.ParameterClass) {
					case EffectParameterClass.Scalar:
						if (parameter.Elements.Count > 0) {
							throw new NotImplementedException();
						}
						for (int i=0; i<data.Length; i++) {
							uniforms_float4[float4_index*4+i] = (float)data[i];
						}
						break;
					case EffectParameterClass.Vector:
					case EffectParameterClass.Matrix:
						long rows = Math.Min (symbol.register_count, parameter.RowCount);
						if (parameter.Elements.Count > 0) {
							//rows = Math.Min (symbol.register_count, parameter.Elements.Count*parameter.RowCount);
							if (symbol.register_count*4 != data.Length) {
								throw new NotImplementedException();
							}
							for (int i=0; i<data.Length; i++) {
								uniforms_float4[float4_index*4+i] = data[i];
							}
						} else {
							for (int y=0; y<rows; y++) {
								for (int x=0; x<parameter.ColumnCount; x++) {
									uniforms_float4[(float4_index+y)*4+x] = (float)data[y*parameter.ColumnCount+x];
								}
							}
						}
						break;
					default:
						throw new NotImplementedException();
					}
					float4_index += (int)symbol.register_count;
					break;
				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
					throw new NotImplementedException();
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
						MojoShader.MOJOSHADER_symbol? samplerSymbol = null;
						foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
							if (symbol.register_set ==
							    	MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER &&
							    symbol.register_index == sampler.index) {
								
								samplerSymbol = symbol;
								break;
							}
						}
						
						Texture tex = null;
						if (samplerSymbol.HasValue) {
							DXEffectObject.d3dx_sampler samplerState =
								(DXEffectObject.d3dx_sampler)parameters[samplerSymbol.Value.name].data;
							if (samplerState.state_count > 0) {
								string textureName = samplerState.states[0].parameter.name;
								EffectParameter textureParameter = parameters[textureName];
								if (textureParameter != null) {
									tex = (Texture)textureParameter.data;
								}
							}
						}
						if (tex == null) {
							//texutre 0 will be set in drawbatch :/
							if (sampler.index == 0) {
								continue;
							}
							//are smapler indexes always normal texture indexes?
							tex = (Texture)textures [sampler.index];
						}

						GL.ActiveTexture( (TextureUnit)((int)TextureUnit.Texture0 + sampler.index) );

						tex.Activate ();
						
						samplerStates[sampler.index].Activate(tex.glTarget, tex.LevelCount > 1);
						
					}
	
				}
			}
		}
		
	}
}

