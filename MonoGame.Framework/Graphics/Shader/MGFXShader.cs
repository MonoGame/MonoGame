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
#elif PSM
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
	internal class MGFXShader : Shader
	{

		internal MGFXShader (GraphicsDevice device, BinaryReader reader)
		{
			var isVertexShader = reader.ReadBoolean ();

#if OPENGL
			if (isVertexShader)
				ShaderType = ShaderType.VertexShader;
			else
				ShaderType = ShaderType.FragmentShader;
#endif // OPENGL

			var shaderLength = (int)reader.ReadUInt16 ();
			var shaderBytecode = reader.ReadBytes (shaderLength);

			var samplerCount = (int)reader.ReadByte ();
			_samplers = new Sampler[samplerCount];
			for (var s = 0; s < samplerCount; s++) {
				_samplers [s].type = (SamplerType)reader.ReadByte ();
				_samplers [s].index = reader.ReadByte ();
#if OPENGL
				_samplers [s].name = reader.ReadString ();
#endif
				_samplers [s].parameter = (int)reader.ReadByte ();
			}

			var cbufferCount = (int)reader.ReadByte ();
			_cbuffers = new int[cbufferCount];
			for (var c = 0; c < cbufferCount; c++)
				_cbuffers [c] = (int)reader.ReadByte ();

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
			var attributeCount = (int)reader.ReadByte ();
			_attributes = new Attribute[attributeCount];
			for (var a = 0; a < attributeCount; a++) {
				_attributes [a].name = reader.ReadString ();
				_attributes [a].usage = (VertexElementUsage)reader.ReadByte ();
				_attributes [a].index = reader.ReadByte ();
				_attributes [a].format = reader.ReadInt16 ();
			}

			_glslCode = System.Text.Encoding.ASCII.GetString (shaderBytecode);
			Compile ();
#endif // OPENGL
		}
	}
}

