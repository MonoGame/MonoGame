using System;
using System.Runtime.InteropServices;
using System.IO;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif PSS
enum ShaderType //FIXME: Major Hack
{
	VertexShader,
	FragmentShader
}
#else
using System.Text;
using OpenTK.Graphics.ES20;
using ShaderType = OpenTK.Graphics.ES20.All;
using ShaderParameter = OpenTK.Graphics.ES20.All;
using TextureUnit = OpenTK.Graphics.ES20.All;
using TextureTarget = OpenTK.Graphics.ES20.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	internal class DXShader
	{
        public readonly ShaderType ShaderType;
        public readonly int ShaderHandle;

        private readonly string _glslCode;

        private readonly string _uniforms_float4_name;
        private readonly float[] _uniforms_float4;

        private readonly string _uniforms_int4_name;
        private readonly int[] _uniforms_int4;

        private readonly string _uniforms_bool_name;
        private readonly int[] _uniforms_bool;

        private readonly MojoShader.MOJOSHADER_symbol[] _symbols;
        private readonly MojoShader.MOJOSHADER_sampler[] _samplers;
        private readonly MojoShader.MOJOSHADER_attribute[] _attributes;

        private readonly DXPreshader _preshader;

        private struct Sampler
        {
            public SamplerType type;
            public int index;
            public string name;
            public string parameter;
        }

        private struct Symbol
        {
            public string name;
            public RegisterSet register_set;
            public int register_index;
            public int register_count;
        }

        private struct Attribute
        {
            public VertexElementUsage usage;
            public int index;
            public string name;
        }

        private readonly Symbol[] _symbols;
        private readonly Sampler[] _samplers;
        private readonly Attribute[] _attributes;

        internal DXShader(DXShader cloneSource)
        {
            // Share all the immutable types.
#if OPENGL
            ShaderType = cloneSource.ShaderType;
            ShaderHandle = cloneSource.ShaderHandle;
            _uniforms_float4_name = cloneSource._uniforms_float4_name;
            _uniforms_int4_name = cloneSource._uniforms_int4_name;
            _uniforms_bool_name = cloneSource._uniforms_bool_name;
#if DEBUG 
            _glslCode = cloneSource._glslCode;
#endif
#endif
            _symbols = cloneSource._symbols;
            _samplers = cloneSource._samplers;
            _attributes = cloneSource._attributes;

            // Clone the mutable types.
            _uniforms_float4 = (float[])cloneSource._uniforms_float4.Clone();
            _uniforms_int4 = (int[])cloneSource._uniforms_int4.Clone();
            _uniforms_bool = (int[])cloneSource._uniforms_bool.Clone();
        }

        internal DXShader(BinaryReader reader)
        {
            var isVertexShader = reader.ReadBoolean();
            if (isVertexShader)
            {
                ShaderType = ShaderType.VertexShader;
                _uniforms_float4_name = "vs_uniforms_vec4";
                _uniforms_int4_name = "vs_uniforms_ivec4";
                _uniforms_bool_name = "vs_uniforms_bool";
            }
            else
            {
				ShaderType = ShaderType.FragmentShader;
                _uniforms_float4_name = "ps_uniforms_vec4";
                _uniforms_int4_name = "ps_uniforms_ivec4";
                _uniforms_bool_name = "ps_uniforms_bool";
            }
#elif DIRECTX

            if (reader.ReadBoolean())
                _preshader = DXPreshader.CreatePreshader(reader);

            _glslCode = reader.ReadString();

            var bool_count = (int)reader.ReadByte();
            var int4_count = (int)reader.ReadByte();
            var float4_count = (int)reader.ReadByte();

            _uniforms_bool = new int[bool_count];
            _uniforms_int4 = new int[int4_count * 4];
            _uniforms_float4 = new float[float4_count * 4];

            var symbolCount = (int)reader.ReadByte();
            _symbols = new MojoShader.MOJOSHADER_symbol[symbolCount];
            for (var s=0; s < symbolCount; s++)
            {
                _symbols[s].name = reader.ReadString();
                _symbols[s].register_set = (RegisterSet)reader.ReadByte();
                _symbols[s].register_index = reader.ReadByte();
                _symbols[s].register_count = reader.ReadByte();
            }

            var samplerCount = (int)reader.ReadByte();
            _samplers = new MojoShader.MOJOSHADER_sampler[samplerCount];
            for (var s = 0; s < samplerCount; s++)
            {
                _samplers[s].name = reader.ReadString();
                _samplers[s].parameter = reader.ReadString();
                _samplers[s].type = (SamplerType)reader.ReadByte();
                _samplers[s].index = reader.ReadByte();
            }

            var attributeCount = (int)reader.ReadByte();
            _attributes = new MojoShader.MOJOSHADER_attribute[attributeCount];
            for (var a = 0; a < attributeCount; a++)
            {
                _attributes[a].name = reader.ReadString();
                _attributes[a].usage = (VertexElementUsage)reader.ReadByte();
                _attributes[a].index = reader.ReadByte();
            }

#if OPENGL
            Threading.Begin();
            try
            {
                ShaderHandle = GL.CreateShader(ShaderType);
#if IPHONE || ANDROID
                GL.ShaderSource(ShaderHandle, 1, new string[] { _glslCode }, (int[])null);
#else
                GL.ShaderSource(ShaderHandle, _glslCode);
#endif
                GL.CompileShader(ShaderHandle);

                var compiled = 0;
#if GLES
                GL.GetShader(ShaderHandle, ShaderParameter.CompileStatus, ref compiled);
#else
                GL.GetShader(ShaderHandle, ShaderParameter.CompileStatus, out compiled);
#endif
                if (compiled == (int)All.False)
                {
#if GLES
                    string log = "";
                    int length = 0;
                    GL.GetShader(ShaderHandle, ShaderParameter.InfoLogLength, ref length);
                    if (length > 0)
                    {
                        var logBuilder = new StringBuilder(length);
                        GL.GetShaderInfoLog(ShaderHandle, length, ref length, logBuilder);
                        log = logBuilder.ToString();
                    }
#else
                    var log = GL.GetShaderInfoLog(ShaderHandle);
#endif
                    Console.WriteLine(log);

                    GL.DeleteShader(ShaderHandle);
                    throw new InvalidOperationException("Shader Compilation Failed");
                }
            }
            finally
            {
                Threading.End();
            }
		}
            /*
            var d3dDevice = GraphicsDevice.Current._d3dDevice;
            if (isVertexShader)
            {
                _vertexShader = new VertexShader(d3dDevice, shaderBytecode, null);

                // TODO: We need the specifics about the input format for DX!
                var elements = new InputElement[_attributes.Length];
                for (var i=0; i < _attributes.Length; i++)
                {
                    var attrib = _attributes[i];
                    elements[i] = new InputElement(attrib.name, 0, Format.R32G32B32A32_Float, attrib.index, 0);
                }

                _inputLayout = new InputLayout(d3dDevice, shaderBytecode, elements);
            }
            else
                _pixelShader = new PixelShader(d3dDevice, shaderBytecode);
            */
#endif
        }

		public void OnLink(int program) 
        {
            if (ShaderType != ShaderType.VertexShader)
                return;

			//bind attributes
			foreach (var attrb in _attributes) 
            {
				switch (attrb.usage) 
                {
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
#endif
		}
		
		public void Apply(  int program, 
                            EffectParameterCollection parameters,
		                    GraphicsDevice graphicsDevice) 
        {			
			var vp = graphicsDevice.Viewport;
			var textures = graphicsDevice.Textures;
			var samplerStates = graphicsDevice.SamplerStates;
			
			//Populate the uniform register arrays
			//TODO: not necessarily packed contiguously, get info from mojoshader somehow
			var bool_index = 0;
            var float4_index = 0;
            var int4_index = 0;
			
			//TODO: only populate modified stuff?
            foreach (var symbol in _symbols) 
            {
                var parameter = parameters[symbol.name];

				switch (symbol.register_set) 
                {
				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
					if (parameter.Elements.Count > 0)
						throw new NotImplementedException();
					_uniforms_bool[bool_index*4] = (int)parameter.data;
					bool_index += (int)symbol.register_count;
					break;

				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:

                    var data = parameter.GetValueSingleArray();

					switch (parameter.ParameterClass) 
                    {
					case EffectParameterClass.Scalar:
						if (parameter.Elements.Count > 0)
							throw new NotImplementedException();

						for (int i=0; i<data.Length; i++)
							_uniforms_float4[float4_index*4+i] = (float)data[i];
						break;
					case EffectParameterClass.Vector:
					case EffectParameterClass.Matrix:
                        var rows = Math.Min(symbol.register_count, parameter.RowCount);
						if (parameter.Elements.Count > 0) 
                        {
							//rows = Math.Min (symbol.register_count, parameter.Elements.Count*parameter.RowCount);
							if (symbol.register_count*4 != data.Length)
								throw new NotImplementedException();

                            for (var i = 0; i < data.Length; i++)
								_uniforms_float4[float4_index*4+i] = data[i];
						} 
                        else 
                        {
                            for (var y = 0; y < rows; y++)
                            {
								for (int x=0; x<parameter.ColumnCount; x++)
									_uniforms_float4[(float4_index+y)*4+x] = (float)data[y*parameter.ColumnCount+x];
							}
						}
						break;
					default:
						throw new NotImplementedException();
					}
					float4_index += (int)symbol.register_count;
					break;

                                    for (var i = 0; i < data.Length; i++)
                                        _uniforms_float4[symbol.register_index * 4 + i] = data[i];
                                }
                                else
                                {
                                    for (var y = 0; y < rows; y++)
                                        for (int x = 0; x < parameter.ColumnCount; x++)
                                            _uniforms_float4[(symbol.register_index + y) * 4 + x] = (float)data[y * parameter.ColumnCount + x];
                                }
                                break;

                case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
                default:
					throw new NotImplementedException();
				}

            } // foreach (var symbol in symbols)
			
			// Execute the preshader.
			if (_preshader != null)
				_preshader.Run(parameters, _uniforms_float4);
			
			// Upload the uniforms.				
            if (_uniforms_float4.Length > 0)
            {
                var vec4_loc = GL.GetUniformLocation(program, _uniforms_float4_name);
                GL.Uniform4(vec4_loc, _uniforms_float4.Length / 4, _uniforms_float4);
			}
            if (_uniforms_int4_name.Length > 0) 
            {
                var int4_loc = GL.GetUniformLocation(program, _uniforms_int4_name);
                GL.Uniform4(int4_loc, _uniforms_int4.Length / 4, _uniforms_int4);
			}
            if (_uniforms_bool.Length > 0) 
            {
                var bool_loc = GL.GetUniformLocation(program, _uniforms_bool_name);
                GL.Uniform1(bool_loc, _uniforms_bool.Length, _uniforms_bool);
			}
			
			if (ShaderType == ShaderType.FragmentShader) 
            {
                // Activate the textures.
				foreach (var sampler in _samplers) 
                {
                    // Set the sampler texture slot.
                    var loc = GL.GetUniformLocation(program, sampler.name);
					GL.Uniform1(loc, sampler.index);
                    
                    if (sampler.type != MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_2D)
                        continue;

					MojoShader.MOJOSHADER_symbol? samplerSymbol = null;
					foreach (var symbol in _symbols) 
                    {
						if (symbol.register_set ==
							    MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER &&
							symbol.register_index == sampler.index) 
                        {
							samplerSymbol = symbol;
							break;
						}
					}
						
					Texture tex = null;
					if (samplerSymbol.HasValue) 
                    {
						var samplerState = (DXEffectObject.d3dx_sampler)parameters[samplerSymbol.Value.name].data;
						if (samplerState.state_count > 0) 
                        {
							var textureName = samplerState.states[0].parameter.name;
							var textureParameter = parameters[textureName];
							if (textureParameter != null)
								tex = (Texture)textureParameter.data;
						}
					}

					if (tex == null) 
                    {
						//texutre 0 will be set in drawbatch :/
						if (sampler.index == 0)
							continue;

						//are smapler indexes always normal texture indexes?
						tex = (Texture)textures [sampler.index];
					}

					GL.ActiveTexture( (TextureUnit)((int)TextureUnit.Texture0 + sampler.index) );
					tex.Activate();						
					samplerStates[sampler.index].Activate(tex.glTarget, tex.LevelCount > 1);
				}
			}
#endif // OPENGL
        }
		
	}
}

