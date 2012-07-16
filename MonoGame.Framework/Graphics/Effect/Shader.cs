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

        public readonly ShaderType ShaderType;

        public int ShaderHandle;

#if DEBUG
        // We only keep around the GLSL code for debugging.
        private string _glslCode;
#endif

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

        internal Shader(GraphicsDevice device, BinaryReader reader)
        {
            var isVertexShader = reader.ReadBoolean();

#if OPENGL
            if (isVertexShader)
                ShaderType = ShaderType.VertexShader;
            else
                ShaderType = ShaderType.FragmentShader;
#endif // OPENGL

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
            var glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode);

            var attributeCount = (int)reader.ReadByte();
            _attributes = new Attribute[attributeCount];
            for (var a = 0; a < attributeCount; a++)
            {
                _attributes[a].name = reader.ReadString();
                _attributes[a].usage = (VertexElementUsage)reader.ReadByte();
                _attributes[a].index = reader.ReadByte();
                _attributes[a].format = reader.ReadInt16();
            }

            
            Threading.BlockOnUIThread(() =>
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
            });

#endif // OPENGL
        }

#if OPENGL
        
        public void OnLink(int program) 
        {
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
		}


        public void Apply(  GraphicsDevice graphicsDevice,
                            int program, 
                            EffectParameterCollection parameters,
		                    ConstantBuffer[] cbuffers) 
        {
			if (ShaderType == ShaderType.FragmentShader) 
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

            // Update and set the constants.
            for (var c = 0; c < _cbuffers.Length; c++)
            {
                var cb = cbuffers[_cbuffers[c]];
                cb.Apply(program, parameters);
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

            // TODO: This has to be deferred like setting shaders 
            // and be done from the GraphicsDevice.ApplyState.  This
            // also forces us to come up with a ConstantBuffer API.

            // Update and set the constants.
            for (var c = 0; c < _cbuffers.Length; c++)
            {
                var cb = cbuffers[_cbuffers[c]];
                cb.Apply(_vertexShader != null, c, parameters);
            }
        }
		
#endif // DIRECTX
	}
}

