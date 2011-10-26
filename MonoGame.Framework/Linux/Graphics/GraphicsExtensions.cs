using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
 

namespace Microsoft.Xna.Framework.Graphics
{
    public static class GraphicsExtensions
    {
        public static All OpenGL11(CullMode cull)
        {
            switch (cull)
            {
                case CullMode.CullClockwiseFace:
                    return All.Cw;
                case CullMode.CullCounterClockwiseFace:
                    return All.Ccw;
                default:
                    throw new NotImplementedException();
            }
        }

		internal static int WrapMode(this TextureAddressMode textureAddressMode)
		{
			switch(textureAddressMode)
			{
			case TextureAddressMode.Clamp:
				return (int)TextureWrapMode.Clamp;
			case TextureAddressMode.Wrap:
				return (int)TextureWrapMode.Repeat;
			case TextureAddressMode.Mirror:
				return (int)TextureWrapMode.MirroredRepeat;
			default:
				throw new NotImplementedException("No support for " + textureAddressMode);
			}
		}
				
        public static int OpenGLNumberOfElements(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    throw new NotImplementedException();

                case VertexElementFormat.Vector2:
                    return 2;

                case VertexElementFormat.Vector3:
                    return 3;

                case VertexElementFormat.Vector4:
                    return 4;

                case VertexElementFormat.Color:
                    return 4;

                case VertexElementFormat.Byte4:
                    return 4;

                case VertexElementFormat.Short2:
                    return 2;

                case VertexElementFormat.Short4:
                    return 2;

                case VertexElementFormat.NormalizedShort2:
                    return 2;

                case VertexElementFormat.NormalizedShort4:
                    return 4;

                case VertexElementFormat.HalfVector2:
                    return 2;

                case VertexElementFormat.HalfVector4:
                    return 4;
            }

            throw new NotImplementedException();
        }

        public static All OpenGLValueType(VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    throw new NotImplementedException();

                case VertexElementFormat.Vector2:
                    return All.Float;

                case VertexElementFormat.Vector3:
                    return All.Float;

                case VertexElementFormat.Vector4:
                    return All.Float;

                case VertexElementFormat.Color:
                    return All.UnsignedByte;

                case VertexElementFormat.Byte4:
                    return All.UnsignedByte;

                case VertexElementFormat.Short2:
                    return All.UnsignedShort;

                case VertexElementFormat.Short4:
                    return All.UnsignedShort;

                case VertexElementFormat.NormalizedShort2:
                    return All.UnsignedShort;

                case VertexElementFormat.NormalizedShort4:
                    return All.UnsignedShort;

                case VertexElementFormat.HalfVector2:
                    return All.Float;

                case VertexElementFormat.HalfVector4:
                    return All.Float;
            }

            throw new NotImplementedException();
        }

        public static int Size(this SurfaceFormat surfaceFormat)
        {
            switch (surfaceFormat)
            {
                case SurfaceFormat.Color:
                    return 0;
                case SurfaceFormat.Dxt3:
                    return 4;
                case SurfaceFormat.Bgra4444:
                    return 2;
                case SurfaceFormat.Bgra5551:
                    return 2;
                case SurfaceFormat.Alpha8:
                    return 1;
                default:
                    throw new NotImplementedException();
            }
        }
		
        public static int GetTypeSize(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return 4;

                case VertexElementFormat.Vector2:
                    return 8;

                case VertexElementFormat.Vector3:
                    return 12;

                case VertexElementFormat.Vector4:
                    return 0x10;

                case VertexElementFormat.Color:
                    return 4;

                case VertexElementFormat.Byte4:
                    return 4;

                case VertexElementFormat.Short2:
                    return 4;

                case VertexElementFormat.Short4:
                    return 8;

                case VertexElementFormat.NormalizedShort2:
                    return 4;

                case VertexElementFormat.NormalizedShort4:
                    return 8;

                case VertexElementFormat.HalfVector2:
                    return 4;

                case VertexElementFormat.HalfVector4:
                    return 8;
            }
            return 0;
        }

		internal static void ApplyTo(this SamplerState samplerState, TextureTarget textureTarget)
		{
			// Set up texture sample filtering.
			bool useMipmaps = samplerState.MaxMipLevel > 0;
			switch(samplerState.Filter)
			{
			case TextureFilter.Point:
				GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest));
				GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				break;
			case TextureFilter.Linear:
				GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
				GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				break;
			case TextureFilter.Anisotropic:
				// TODO: Requires EXT_texture_filter_anisotropic. Use linear filtering for now.
				GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
				GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				break;
			}

			// Set up texture addressing.
			GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, samplerState.AddressU.WrapMode());
			GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, samplerState.AddressV.WrapMode());
		}
	}
}
