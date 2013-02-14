using System;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
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
	internal partial class Shader
	{
#if OPENGL
		public ShaderType ShaderType { get; protected set; }

		public int ShaderHandle;

		// We keep this around for recompiling on context lost and debugging.
		protected string _glslCode;

		// Flag whether the shader needs to be recompiled
		internal bool NeedsRecompile = false;

		protected struct Attribute
		{
			public VertexElementUsage usage;
			public int index;
			public string name;
			public short format;

			public Attribute(VertexElementUsage usage_,
							 int index_,
							 string name_,
							 short format_)
			{
				usage = usage_;
				index = index_;
				name = name_;
				format = format_;
			}
		}

		protected Attribute[] _attributes;		

		static readonly float[] _posFixup = new float[4];
		
		public void Compile()
		{
			Threading.BlockOnUIThread (() =>
			{
				ShaderHandle = GL.CreateShader (ShaderType);
#if GLES
				GL.ShaderSource(ShaderHandle, 1, new string[] { _glslCode }, (int[])null);
#else
				GL.ShaderSource (ShaderHandle, _glslCode);
#endif
				GL.CompileShader (ShaderHandle);

				var compiled = 0;
#if GLES
				GL.GetShader(ShaderHandle, ShaderParameter.CompileStatus, ref compiled);
#else
				GL.GetShader (ShaderHandle, ShaderParameter.CompileStatus, out compiled);
#endif
				if (compiled == (int)All.False) {
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
					var log = GL.GetShaderInfoLog (ShaderHandle);
#endif
					Console.WriteLine (log);

					GL.DeleteShader (ShaderHandle);
					throw new InvalidOperationException ("Shader Compilation Failed");
				}
			});
		}
		
		public virtual void OnLink (int program)
		{
			if (ShaderType != ShaderType.VertexShader)
				return;

			// Bind the vertex attributes to the shader program.
			foreach (var attrb in _attributes) {
				switch (attrb.usage) {
				case VertexElementUsage.Color:
					GL.BindAttribLocation (program, GraphicsDevice.attributeColor, attrb.name);
					break;
				case VertexElementUsage.Position:
					GL.BindAttribLocation (program, GraphicsDevice.attributePosition + attrb.index, attrb.name);
					break;
				case VertexElementUsage.TextureCoordinate:
					GL.BindAttribLocation (program, GraphicsDevice.attributeTexCoord + attrb.index, attrb.name);
					break;
				case VertexElementUsage.Normal:
					GL.BindAttribLocation (program, GraphicsDevice.attributeNormal, attrb.name);
					break;
				case VertexElementUsage.BlendIndices:
					GL.BindAttribLocation (program, GraphicsDevice.attributeBlendIndicies, attrb.name);
					break;
				case VertexElementUsage.BlendWeight:
					GL.BindAttribLocation (program, GraphicsDevice.attributeBlendWeight, attrb.name);
					break;
				case VertexElementUsage.Binormal:
					GL.BindAttribLocation (program, GraphicsDevice.attributeBinormal, attrb.name);
					break;
				case VertexElementUsage.Tangent:
					GL.BindAttribLocation (program, GraphicsDevice.attributeBlendWeight, attrb.name);
					break;
				default:
					throw new NotImplementedException();
				}
			}
		}


		public virtual void Apply (GraphicsDevice graphicsDevice,
							int program, 
							EffectParameterCollection parameters,
							ConstantBuffer[] cbuffers)
		{									
			var textures = graphicsDevice.Textures;
			var samplerStates = graphicsDevice.SamplerStates;

			if (ShaderType == ShaderType.FragmentShader) {
				// Activate the textures.
				foreach (var sampler in _samplers) {
					// Set the sampler texture slot.
					//
					// TODO: This seems like it only needs to be done once!
					//
					var loc = GL.GetUniformLocation (program, sampler.name);
					GL.Uniform1 (loc, sampler.index);

					// TODO: Fix Volume samplers!
					// (are they really broken?)
					if (sampler.type == SamplerType.SamplerVolume)
						throw new NotImplementedException ();

					Texture tex = null;
					if (sampler.parameter >= 0) {
						var textureParameter = parameters [sampler.parameter];
						tex = textureParameter.Data as Texture;
					}

					if (tex == null) {
						//texutre 0 will be set in drawbatch :/
						if (sampler.index == 0)
							continue;

						//are smapler indexes always normal texture indexes?
						tex = (Texture)textures [sampler.index];
					}

					if (tex != null) {
						tex.glTextureUnit = ((TextureUnit)((int)TextureUnit.Texture0 + sampler.index));
						tex.Activate ();						
						samplerStates [sampler.index].Activate (tex.glTarget, tex.LevelCount > 1);
					}
				}
			}

			// Update and set the constants.
			for (var c = 0; c < _cbuffers.Length; c++) {
				var cb = cbuffers [_cbuffers [c]];
				cb.Apply (program, parameters);
			}

			if (ShaderType == ShaderType.VertexShader) {
				// Apply vertex shader fix:
				// The following two lines are appended to the end of vertex shaders
				// to account for rendering differences between OpenGL and DirectX:
				//
				// gl_Position.y = gl_Position.y * posFixup.y;
				// gl_Position.xy += posFixup.zw * gl_Position.ww;
				//
				// (the following paraphrased from wine, wined3d/state.c and wined3d/glsl_shader.c)
				//
				// - We need to flip along the y-axis in case of offscreen rendering.
				// - D3D coordinates refer to pixel centers while GL coordinates refer
				//   to pixel corners.
				// - D3D has a top-left filling convention. We need to maintain this
				//   even after the y-flip mentioned above.
				// In order to handle the last two points, we translate by
				// (63.0 / 128.0) / VPw and (63.0 / 128.0) / VPh. This is equivalent to
				// translating slightly less than half a pixel. We want the difference to
				// be large enough that it doesn't get lost due to rounding inside the
				// driver, but small enough to prevent it from interfering with any
				// anti-aliasing.
				//
				// OpenGL coordinates specify the center of the pixel while d3d coords specify
				// the corner. The offsets are stored in z and w in posFixup. posFixup.y contains
				// 1.0 or -1.0 to turn the rendering upside down for offscreen rendering. PosFixup.x
				// contains 1.0 to allow a mad.
	
				_posFixup [0] = 1.0f;
				_posFixup [1] = 1.0f;
				_posFixup [2] = (63.0f / 64.0f) / graphicsDevice.Viewport.Width;
				_posFixup [3] = -(63.0f / 64.0f) / graphicsDevice.Viewport.Height;
				//If we have a render target bound (rendering offscreen)
				if (graphicsDevice.GetRenderTargets ().Length > 0) {
					//flip vertically
					_posFixup [1] *= -1.0f;
					_posFixup [3] *= -1.0f;
				}
				var posFixupLoc = GL.GetUniformLocation (program, "posFixup"); // TODO: Look this up on link!
				GL.Uniform4 (posFixupLoc, 1, _posFixup);
			}
		}

#endif // OPENGL

	}
}

