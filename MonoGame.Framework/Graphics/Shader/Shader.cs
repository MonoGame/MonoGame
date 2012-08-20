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
	internal class Shader
	{
#if OPENGL

        internal int ShaderHandle;

        // We keep this around for recompiling on context lost and debugging.
        private string _glslCode;

        // Flag whether the shader needs to be recompiled
        internal bool NeedsRecompile = false;

        private struct Attribute
        {
            public VertexElementUsage usage;
            public int index;
            public string name;
            public short format;
        }

        private readonly Attribute[] _attributes;

#elif DIRECTX

        internal VertexShader _vertexShader;

        internal PixelShader _pixelShader;

        public byte[] Bytecode { get; private set; }

#endif

        /// <summary>
        /// A hash value which can be used to compare shaders.
        /// </summary>
        internal int HashKey { get; private set; }

        private enum SamplerType
        {
            Sampler2D,
            SamplerCube,
            SamplerVolume,
        }

        private struct Sampler
        {
            public SamplerType type;
            public int index;
            public int parameter;

#if OPENGL
            public string name;
#endif
        }

        private readonly Sampler[] _samplers;

        private readonly int[] _cbuffers;
        
        public ShaderStage Stage { get; private set; }
		
        internal Shader(GraphicsDevice device, BinaryReader reader)
        {
            var isVertexShader = reader.ReadBoolean();
            Stage = isVertexShader ? ShaderStage.Vertex : ShaderStage.Pixel;

            var shaderLength = (int)reader.ReadUInt16();
            var shaderBytecode = reader.ReadBytes(shaderLength);

            var samplerCount = (int)reader.ReadByte();
            _samplers = new Sampler[samplerCount];
            for (var s = 0; s < samplerCount; s++)
            {
                _samplers[s].type = (SamplerType)reader.ReadByte();
                _samplers[s].index = reader.ReadByte();
#if OPENGL
                _samplers[s].name = reader.ReadString();
#endif
                _samplers[s].parameter = (int)reader.ReadByte();
            }

            var cbufferCount = (int)reader.ReadByte();
            _cbuffers = new int[cbufferCount];
            for (var c = 0; c < cbufferCount; c++)
                _cbuffers[c] = (int)reader.ReadByte();

#if DIRECTX

            var d3dDevice = device._d3dDevice;
            if (isVertexShader)
            {
                _vertexShader = new VertexShader(d3dDevice, shaderBytecode, null);

                // We need the bytecode later for allocating the
                // input layout from the vertex declaration.
                Bytecode = shaderBytecode;
                
                HashKey = Effect.ComputeHash(Bytecode);
            }
            else
                _pixelShader = new PixelShader(d3dDevice, shaderBytecode);

#endif // DIRECTX

#if OPENGL
            _glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode);

            HashKey = ShaderProgramCache.Hash(shaderBytecode);

            var attributeCount = (int)reader.ReadByte();
            _attributes = new Attribute[attributeCount];
            for (var a = 0; a < attributeCount; a++)
            {
                _attributes[a].name = reader.ReadString();
                _attributes[a].usage = (VertexElementUsage)reader.ReadByte();
                _attributes[a].index = reader.ReadByte();
                _attributes[a].format = reader.ReadInt16();
            }

            CompileShader();

#endif // OPENGL
        }

#if OPENGL
        internal void CompileShader()
        {
            Threading.BlockOnUIThread(() =>
            {
                ShaderHandle = GL.CreateShader(Stage == ShaderStage.Vertex ? ShaderType.VertexShader : ShaderType.FragmentShader);
#if GLES
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
            });
        }
        
        public void OnLink(int program) 
        {
            if (Stage != ShaderStage.Vertex)
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
		}


        public void Apply(  GraphicsDevice graphicsDevice,
                            int program, 
                            EffectParameterCollection parameters,
		                    ConstantBuffer[] cbuffers) 
        {
			if (Stage == ShaderStage.Pixel) 
            {
                graphicsDevice.PixelShader = this;

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

                    if (sampler.parameter >= 0) 
                    {
                        var textureParameter = parameters[sampler.parameter];
                        
                        // TODO: The texture could be NULL here if we're using SpriteBatch
                        // which in that case we are making more work by setting this to
                        // NULL then setting it back to possibly the same texture.
                        //
                        // Maybe we should not set the texture if it is null?  But in
                        // that case then maybe we would be breaking state?  Null texture
                        // rendering is undefined right?  Does it hurt then?

                        var texture = textureParameter.Data as Texture;
                        graphicsDevice.Textures[sampler.index] = texture;
					}
				}
			}
            else
            {
                graphicsDevice.VertexShader = this;
            }

            // Update the constant buffers with the parameter state
            // and then set them on the graphics device.
            for (var c = 0; c < _cbuffers.Length; c++)
            {
                var cb = cbuffers[_cbuffers[c]];
                cb.Update(parameters);
                graphicsDevice.SetConstantBuffer(Stage, c, cb);
            }
        }

#endif // OPENGL

#if DIRECTX

        public void Apply(  GraphicsDevice graphicsDevice, 
                            EffectParameterCollection parameters,
                            ConstantBuffer[] cbuffers )
        {
            if (_pixelShader != null)
            {
                graphicsDevice.PixelShader = this;

                // TODO: We can move the samplers info out to the 
                // EffectPass as they are effect specific and have 
                // nothing to do with the shader.
                foreach (var sampler in _samplers)
                {
                    var param = parameters[sampler.parameter];
                    var texture = param.Data as Texture;
                    graphicsDevice.Textures[sampler.index] = texture;
                }
            }
            else
            {
                graphicsDevice.VertexShader = this;
            }

            // Update the constant buffers with the parameter state
            // and then set them on the graphics device.
            for (var c = 0; c < _cbuffers.Length; c++)
            {
                // TODO: Like the sampler info above i think we should
                // move the constant buffer info out to EffectPass.  
                // 
                // Eventually all we should have in Shader is an optional
                // and light reflection API for constants and that is it.
                //

                var cb = cbuffers[_cbuffers[c]];
                cb.Update(parameters);
                graphicsDevice.SetConstantBuffer(Stage, c, cb);
            }
        }
		
#endif // DIRECTX
	}
}

