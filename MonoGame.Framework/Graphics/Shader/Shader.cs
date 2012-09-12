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
    internal enum SamplerType
    {
        Sampler2D,
        SamplerCube,
        SamplerVolume,
    }

    // TODO: We should convert the sampler info below 
    // into the start of a Shader reflection API.

    internal struct SamplerInfo
    {
        public SamplerType type;
        public int index;
        public string name;

        // TODO: This should be moved to EffectPass.
        public int parameter;
    }

    internal class Shader : GraphicsResource
	{
#if OPENGL

        // The shader handle.
	    private int _shaderHandle = -1;

        // We keep this around for recompiling on context lost and debugging.
        private readonly string _glslCode;

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

        public SamplerInfo[] Samplers { get; private set; }

	    public int[] CBuffers { get; private set; }

        public ShaderStage Stage { get; private set; }
		
        internal Shader(GraphicsDevice device, BinaryReader reader)
        {
            graphicsDevice = device;
            graphicsDevice.DeviceResetting += graphicsDevice_DeviceResetting;

            var isVertexShader = reader.ReadBoolean();
            Stage = isVertexShader ? ShaderStage.Vertex : ShaderStage.Pixel;

            var shaderLength = (int)reader.ReadUInt16();
            var shaderBytecode = reader.ReadBytes(shaderLength);

            var samplerCount = (int)reader.ReadByte();
            Samplers = new SamplerInfo[samplerCount];
            for (var s = 0; s < samplerCount; s++)
            {
                Samplers[s].type = (SamplerType)reader.ReadByte();
                Samplers[s].index = reader.ReadByte();
#if OPENGL
                Samplers[s].name = reader.ReadString();
#endif
                Samplers[s].parameter = reader.ReadByte();
            }

            var cbufferCount = (int)reader.ReadByte();
            CBuffers = new int[cbufferCount];
            for (var c = 0; c < cbufferCount; c++)
                CBuffers[c] = reader.ReadByte();

#if DIRECTX

            var d3dDevice = device._d3dDevice;
            if (isVertexShader)
            {
                _vertexShader = new VertexShader(d3dDevice, shaderBytecode, null);

                // We need the bytecode later for allocating the
                // input layout from the vertex declaration.
                Bytecode = shaderBytecode;
                
                HashKey = MonoGame.Utilities.Hash.ComputeHash(Bytecode);
            }
            else
                _pixelShader = new PixelShader(d3dDevice, shaderBytecode);

#endif // DIRECTX

#if OPENGL
            _glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode);

            HashKey = MonoGame.Utilities.Hash.ComputeHash(shaderBytecode);

            var attributeCount = (int)reader.ReadByte();
            _attributes = new Attribute[attributeCount];
            for (var a = 0; a < attributeCount; a++)
            {
                _attributes[a].name = reader.ReadString();
                _attributes[a].usage = (VertexElementUsage)reader.ReadByte();
                _attributes[a].index = reader.ReadByte();
                _attributes[a].format = reader.ReadInt16();
            }

#endif // OPENGL
        }

#if OPENGL
        internal int GetShaderHandle()
        {
            // If the shader has already been created then return it.
            if (_shaderHandle != -1)
                return _shaderHandle;
            
            //
            _shaderHandle = GL.CreateShader(Stage == ShaderStage.Vertex ? ShaderType.VertexShader : ShaderType.FragmentShader);
#if GLES
			GL.ShaderSource(_shaderHandle, 1, new string[] { _glslCode }, (int[])null);
#else
            GL.ShaderSource(_shaderHandle, _glslCode);
#endif
            GL.CompileShader(_shaderHandle);

            var compiled = 0;
#if GLES
			GL.GetShader(_shaderHandle, ShaderParameter.CompileStatus, ref compiled);
#else
            GL.GetShader(_shaderHandle, ShaderParameter.CompileStatus, out compiled);
#endif
            if (compiled == (int)All.False)
            {
#if GLES
                string log = "";
                int length = 0;
				GL.GetShader(_shaderHandle, ShaderParameter.InfoLogLength, ref length);
                if (length > 0)
                {
                    var logBuilder = new StringBuilder(length);
					GL.GetShaderInfoLog(_shaderHandle, length, ref length, logBuilder);
                    log = logBuilder.ToString();
                }
#else
                var log = GL.GetShaderInfoLog(_shaderHandle);
#endif
                Console.WriteLine(log);

                GL.DeleteShader(_shaderHandle);
                _shaderHandle = -1;

                throw new InvalidOperationException("Shader Compilation Failed");
            }

            return _shaderHandle;
        }

        internal void BindVertexAttributes(int program)
        {
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

        internal void ApplySamplerTextureUnits(int program)
        {
            // Assign the texture unit index to the sampler uniforms.
            foreach (var sampler in Samplers)
            {
                var loc = GL.GetUniformLocation(program, sampler.name);
                if (loc != -1)
                    GL.Uniform1(loc, sampler.index);
            }
        }

#endif // OPENGL

        void graphicsDevice_DeviceResetting(object sender, EventArgs e)
        {
#if OPENGL
            if (_shaderHandle != -1)
            {
                GL.DeleteShader(_shaderHandle);
                _shaderHandle = -1;
            }
#endif
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                graphicsDevice.DeviceResetting -= graphicsDevice_DeviceResetting;

#if OPENGL
                if (_shaderHandle != -1)
                    GL.DeleteShader(_shaderHandle);
#endif
            }

            base.Dispose();
        }
	}
}

