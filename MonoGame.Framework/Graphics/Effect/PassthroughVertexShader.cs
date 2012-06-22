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
	internal class PassthroughVertexShader : Shader
	{
		public PassthroughVertexShader ()
		{
			ShaderType = ShaderType.VertexShader;

			_cbuffers = new int[0];
			Attribute[] attributes = {
				new Attribute (VertexElementUsage.Position, 0, "aPosition", 0),
				new Attribute (VertexElementUsage.TextureCoordinate, 0, "aTexCoord", 0),
				new Attribute (VertexElementUsage.Color, 0, "aColor", 0)
			};
			_attributes = attributes;

			var src = @"
				uniform mat4 transformMatrix;
				uniform vec4 posFixup;

				attribute vec4 aPosition;
				attribute vec4 aTexCoord;
				attribute vec4 aColor;

				varying vec4 vTexCoord0;
				varying vec4 vFrontColor;
				void main() {
					vTexCoord0.xy = aTexCoord.xy;
					vFrontColor = aColor;

					gl_Position = transformMatrix * aPosition;

					gl_Position.y = gl_Position.y * posFixup.y;
					gl_Position.xy += posFixup.zw * gl_Position.ww;
				}";
			SetGLSL (src);
		}

		public override void Apply (GraphicsDevice graphicsDevice,
                            int program, 
                            EffectParameterCollection parameters,
		                    ConstantBuffer[] cbuffers)
		{
			base.Apply (graphicsDevice, program, parameters, cbuffers);

			Viewport vp = graphicsDevice.Viewport;
			Matrix projection = Matrix.CreateOrthographicOffCenter (0, vp.Width, vp.Height, 0, 0, 1);
			Matrix halfPixelOffset = Matrix.CreateTranslation (-0.5f, -0.5f, 0);
			Matrix transform = halfPixelOffset * projection;

			int uniform = GL.GetUniformLocation (program, "transformMatrix");
			GL.UniformMatrix4 (uniform, 1, false, Matrix.ToFloatArray (transform));
		}
	}
}

