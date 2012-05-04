using System;
using System.Runtime.InteropServices;
using System.IO;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif WINRT
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
#elif PSS
enum ShaderType //FIXME: Major Hack
{
	VertexShader,
	FragmentShader
}
#elif GLES
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
#if OPENGL

        public readonly ShaderType ShaderType;

        public readonly int ShaderHandle;

#if DEBUG
        // We only keep around the GLSL code for debugging.
        private readonly string _glslCode;
#endif

        private readonly string _uniforms_float4_name;
        private readonly string _uniforms_int4_name;
        private readonly string _uniforms_bool_name;

#elif DIRECTX

        private VertexShader _vertexShader;

        private PixelShader _pixelShader;

        private byte[] _shaderBytecode;

#endif

        private readonly float[] _uniforms_float4;
        private readonly int[] _uniforms_int4;
        private readonly int[] _uniforms_bool;

        private enum RegisterSet
        {
            Bool,
            Int4,
            Float4,
            Sampler,
        }

        public enum SamplerType
        {
            Sampler2D,
            SamplerCube,
            SamplerVolume,
        }

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
            public short format;
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

#elif DIRECTX
            _pixelShader = cloneSource._pixelShader;
            _vertexShader = cloneSource._vertexShader;
            _shaderBytecode = cloneSource._shaderBytecode;
#endif
            _symbols = cloneSource._symbols;
            _samplers = cloneSource._samplers;
            _attributes = cloneSource._attributes;

            // Clone the mutable types.
            _uniforms_float4 = (float[])cloneSource._uniforms_float4.Clone();
            _uniforms_int4 = (int[])cloneSource._uniforms_int4.Clone();
            _uniforms_bool = (int[])cloneSource._uniforms_bool.Clone();
        }

        internal DXShader(GraphicsDevice device, BinaryReader reader)
        {
            var isVertexShader = reader.ReadBoolean();

#if OPENGL
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

#endif

            var shaderLength = (int)reader.ReadUInt16();
            var shaderBytecode = reader.ReadBytes(shaderLength);

#if OPENGL
            var glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode);
#endif

            var bool_count = (int)reader.ReadByte();
            var int4_count = (int)reader.ReadByte();
            var float4_count = (int)reader.ReadByte();

            _uniforms_bool = new int[bool_count];
            _uniforms_int4 = new int[int4_count * 4];
            _uniforms_float4 = new float[float4_count * 4];

            var symbolCount = (int)reader.ReadByte();
            _symbols = new Symbol[symbolCount];
            for (var s = 0; s < symbolCount; s++)
            {
                _symbols[s].name = reader.ReadString();
                _symbols[s].register_set = (RegisterSet)reader.ReadByte();
                _symbols[s].register_index = reader.ReadByte();
                _symbols[s].register_count = reader.ReadByte();
            }

            var samplerCount = (int)reader.ReadByte();
            _samplers = new Sampler[samplerCount];
            for (var s = 0; s < samplerCount; s++)
            {
                _samplers[s].name = reader.ReadString();
                _samplers[s].parameter = reader.ReadString();
                _samplers[s].type = (SamplerType)reader.ReadByte();
                _samplers[s].index = reader.ReadByte();
            }

            var attributeCount = (int)reader.ReadByte();
            _attributes = new Attribute[attributeCount];
            for (var a = 0; a < attributeCount; a++)
            {
                _attributes[a].name = reader.ReadString();
                _attributes[a].usage = (VertexElementUsage)reader.ReadByte();
                _attributes[a].index = reader.ReadByte();
                _attributes[a].format = reader.ReadInt16();
            }

#if OPENGL
            Threading.Begin();
            try
            {
                ShaderHandle = GL.CreateShader(ShaderType);
#if GLES
                GL.ShaderSource(ShaderHandle, 1, new string[] { glslCode }, (int[])null);
#else
                GL.ShaderSource(ShaderHandle, glslCode);
#endif
                GL.CompileShader(ShaderHandle);

#if DEBUG
                // When debugging store this for later inspection.
                _glslCode = glslCode;
#endif

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
#elif DIRECTX

            var d3dDevice = device._d3dDevice;
            if (isVertexShader)
            {
                _vertexShader = new VertexShader(d3dDevice, shaderBytecode, null);

                // We need the bytecode later for allocating the
                // input layout from the vertex declaration.
                _shaderBytecode = shaderBytecode;
            }
            else
                _pixelShader = new PixelShader(d3dDevice, shaderBytecode);

#endif
        }

		public void OnLink(int program) 
        {
#if OPENGL
            if (ShaderType != ShaderType.VertexShader)
                return;

			// Bind the vertex attributes to the shader program.
			foreach (var attrb in _attributes) 
            {
				switch (attrb.usage) 
                {
                    case VertexElementUsage.Color:
					    GL.BindAttribLocation(program, GraphicsDevice.attributeColor, attrb.name);
    					break;
                    case VertexElementUsage.Position:
	    				GL.BindAttribLocation(program, GraphicsDevice.attributePosition + attrb.index, attrb.name);
		    			break;
                    case VertexElementUsage.TextureCoordinate:
			    		GL.BindAttribLocation(program, GraphicsDevice.attributeTexCoord + attrb.index, attrb.name);
				    	break;
				    case VertexElementUsage.Normal:
    					GL.BindAttribLocation(program, GraphicsDevice.attributeNormal, attrb.name);
	    				break;
                    case VertexElementUsage.BlendIndices:
					    GL.BindAttribLocation(program, GraphicsDevice.attributeBlendIndicies, attrb.name);
					    break;
                    case VertexElementUsage.BlendWeight:
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
			// TODO: It would be much better if we kept track
            // of dirty states and only update those parameters.

            foreach (var symbol in _symbols) 
            {
                var parameter = parameters[symbol.name];

				switch (symbol.register_set) 
                {
                    case RegisterSet.Bool:
                    {
                        if (parameter.Elements.Count > 0)
                            throw new NotImplementedException();
                        _uniforms_bool[symbol.register_index * 4] = (int)parameter.Data;

                        break;
                    }

                    case RegisterSet.Float4:
                    {
                        var data = parameter.GetValueSingleArray();
                        switch (parameter.ParameterClass)
                        {
                            case EffectParameterClass.Scalar:
                                if (parameter.Elements.Count > 0)
                                    throw new NotImplementedException();

                                for (int i = 0; i < data.Length; i++)
                                    _uniforms_float4[symbol.register_index * 4 + i] = (float)data[i];
                                break;
                            case EffectParameterClass.Vector:
                            case EffectParameterClass.Matrix:
                                var rows = Math.Min(symbol.register_count, parameter.RowCount);
                                if (parameter.Elements.Count > 0)
                                {
                                    //rows = Math.Min (symbol.register_count, parameter.Elements.Count*parameter.RowCount);
                                    if (symbol.register_count * 4 != data.Length)
                                        throw new NotImplementedException();

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

                            default:
                                throw new NotImplementedException();
                        }

                        break;
                    }

                    // We deal with samplers a little further down.
                    case RegisterSet.Sampler:
					    break;

                    case RegisterSet.Int4:
                    default:
					    throw new NotImplementedException();

                } // switch (symbol.register_set)

            } // foreach (var symbol in _symbols)
			
#if DIRECTX

            var d3dContext = graphicsDevice._d3dContext;
            if (_pixelShader != null)
            {

                foreach (var sampler in _samplers)
                {
                    var param = parameters[sampler.name];
                    var texture = param.Data as Texture;
                    graphicsDevice.Textures[sampler.index] = texture;
                }

                d3dContext.PixelShader.Set(_pixelShader);
            }
            else
            {
                d3dContext.VertexShader.Set(_vertexShader);

                // Give the shader bytecode to the device so it
                // can generate the input layout at draw time.
                graphicsDevice._vertexShaderBytecode = _shaderBytecode;
            }

#elif OPENGL

            var textures = graphicsDevice.Textures;
			var samplerStates = graphicsDevice.SamplerStates;

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
                    //
                    // TODO: This seems like it only needs to be done once!
                    //
                    var loc = GL.GetUniformLocation(program, sampler.name);
					GL.Uniform1(loc, sampler.index);

                    // TODO: Fix 3D and Volume samplers!
                    if (sampler.type != SamplerType.Sampler2D)
                        continue;

					Texture tex = null;
                    if (sampler.parameter.Length > 0) 
                    {
                        var textureParameter = parameters[sampler.parameter];
                        tex = textureParameter.Data as Texture;
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

