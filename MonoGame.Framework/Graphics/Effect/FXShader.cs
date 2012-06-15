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
	internal class FXShader : Shader
	{
		public FXShader (GraphicsDevice device, DXShaderData shaderData)
		{
			if (shaderData.IsVertexShader) {
				ShaderType = ShaderType.VertexShader;
			} else {
				ShaderType = ShaderType.FragmentShader;
			}

			_samplers = new Sampler[shaderData._samplers.Length];
			for (int i=0; i<_samplers.Length; i++) {
				_samplers [i].type = (SamplerType)shaderData._samplers [i].type;
				_samplers [i].index = shaderData._samplers [i].index;
				_samplers [i].name = shaderData._samplers [i].samplerName;
				_samplers [i].parameter = shaderData._samplers [i].parameter;
			}

			_cbuffers = (int[])shaderData._cbuffers.Clone ();



			_attributes = new Attribute[shaderData._attributes.Length];
			for (int i=0; i<_attributes.Length; i++) {
				_attributes [i].name = shaderData._attributes [i].name;
				_attributes [i].usage = shaderData._attributes [i].usage;
				_attributes [i].index = shaderData._attributes [i].index;
				_attributes [i].format = shaderData._attributes [i].format;
			}

			var glslCode = System.Text.Encoding.ASCII.GetString (shaderData.ShaderCode);
			SetGLSL (glslCode);

		}
	}
}

