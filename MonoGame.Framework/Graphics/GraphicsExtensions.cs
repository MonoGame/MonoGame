using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif WINRT
#elif GLES
using OpenTK.Graphics.ES20;
using BlendEquationMode = OpenTK.Graphics.ES20.All;
using BlendingFactorSrc = OpenTK.Graphics.ES20.All;
using BlendingFactorDest = OpenTK.Graphics.ES20.All;
using VertexAttribPointerType = OpenTK.Graphics.ES20.All;
using PixelInternalFormat = OpenTK.Graphics.ES20.All;
using PixelType = OpenTK.Graphics.ES20.All;
using PixelFormat = OpenTK.Graphics.ES20.All;
using VertexPointerType = OpenTK.Graphics.ES20.All;
using ColorPointerType = OpenTK.Graphics.ES20.All;
using NormalPointerType = OpenTK.Graphics.ES20.All;
using TexCoordPointerType = OpenTK.Graphics.ES20.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public static class GraphicsExtensions
    {
#if !WINRT && !PSS
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

        public static int OpenGLNumberOfElements(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return 1;

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

        public static VertexPointerType OpenGLVertexPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return VertexPointerType.Float;

                case VertexElementFormat.Vector2:
                    return VertexPointerType.Float;

                case VertexElementFormat.Vector3:
                    return VertexPointerType.Float;

                case VertexElementFormat.Vector4:
                    return VertexPointerType.Float;

                case VertexElementFormat.Color:
                    return VertexPointerType.Short;

                case VertexElementFormat.Byte4:
                    return VertexPointerType.Short;

                case VertexElementFormat.Short2:
                    return VertexPointerType.Short;

                case VertexElementFormat.Short4:
                    return VertexPointerType.Short;

                case VertexElementFormat.NormalizedShort2:
                    return VertexPointerType.Short;

                case VertexElementFormat.NormalizedShort4:
                    return VertexPointerType.Short;

                case VertexElementFormat.HalfVector2:
                    return VertexPointerType.Float;

                case VertexElementFormat.HalfVector4:
                    return VertexPointerType.Float;
            }

            throw new NotImplementedException();
        }

		public static VertexAttribPointerType OpenGLVertexAttribPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return VertexAttribPointerType.Float;

                case VertexElementFormat.Vector2:
                    return VertexAttribPointerType.Float;

                case VertexElementFormat.Vector3:
                    return VertexAttribPointerType.Float;

                case VertexElementFormat.Vector4:
                    return VertexAttribPointerType.Float;

                case VertexElementFormat.Color:
					return VertexAttribPointerType.UnsignedByte;

                case VertexElementFormat.Byte4:
					return VertexAttribPointerType.UnsignedByte;

                case VertexElementFormat.Short2:
                    return VertexAttribPointerType.Short;

                case VertexElementFormat.Short4:
                    return VertexAttribPointerType.Short;

                case VertexElementFormat.NormalizedShort2:
                    return VertexAttribPointerType.Short;

                case VertexElementFormat.NormalizedShort4:
                    return VertexAttribPointerType.Short;
                
#if MONOMAC || WINDOWS || LINUX
               case VertexElementFormat.HalfVector2:
                    return VertexAttribPointerType.HalfFloat;

                case VertexElementFormat.HalfVector4:
                    return VertexAttribPointerType.HalfFloat;
#endif
            }

            throw new NotImplementedException();
        }

        public static bool OpenGLVertexAttribNormalized(this VertexElement element)
        {
            if (element.VertexElementUsage == VertexElementUsage.Color)
                return true;

            switch (element.VertexElementFormat)
            {
                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.NormalizedShort4:
                    return true;

                default:
                    return false;
            }
        }

        public static ColorPointerType OpenGLColorPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return ColorPointerType.Float;

                case VertexElementFormat.Vector2:
                    return ColorPointerType.Float;

                case VertexElementFormat.Vector3:
                    return ColorPointerType.Float;

                case VertexElementFormat.Vector4:
                    return ColorPointerType.Float;

                case VertexElementFormat.Color:
                    //return ColorPointerType.UnsignedByte;
                    return ColorPointerType.UnsignedByte;

                case VertexElementFormat.Byte4:
                    return ColorPointerType.UnsignedByte;

                case VertexElementFormat.Short2:
                    return ColorPointerType.Short;

                case VertexElementFormat.Short4:
                    return ColorPointerType.Short;

                case VertexElementFormat.NormalizedShort2:
                    return ColorPointerType.UnsignedShort;

                case VertexElementFormat.NormalizedShort4:
                    return ColorPointerType.UnsignedShort;
				
#if MONOMAC
                case VertexElementFormat.HalfVector2:
                    return ColorPointerType.HalfFloat;

                case VertexElementFormat.HalfVector4:
                    return ColorPointerType.HalfFloat;
#endif
			}

            throw new NotImplementedException();
        }

       public static NormalPointerType OpenGLNormalPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return NormalPointerType.Float;

                case VertexElementFormat.Vector2:
                    return NormalPointerType.Float;

                case VertexElementFormat.Vector3:
                    return NormalPointerType.Float;

                case VertexElementFormat.Vector4:
                    return NormalPointerType.Float;

                case VertexElementFormat.Color:
                    return NormalPointerType.Byte;

                case VertexElementFormat.Byte4:
                    return NormalPointerType.Byte;

                case VertexElementFormat.Short2:
                    return NormalPointerType.Short;

                case VertexElementFormat.Short4:
                    return NormalPointerType.Short;

                case VertexElementFormat.NormalizedShort2:
                    return NormalPointerType.Short;

                case VertexElementFormat.NormalizedShort4:
                    return NormalPointerType.Short;
				
#if MONOMAC
                case VertexElementFormat.HalfVector2:
                    return NormalPointerType.HalfFloat;

                case VertexElementFormat.HalfVector4:
                    return NormalPointerType.HalfFloat;
#endif
			}

            throw new NotImplementedException();
        }

       public static TexCoordPointerType OpenGLTexCoordPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return TexCoordPointerType.Float;

                case VertexElementFormat.Vector2:
                    return TexCoordPointerType.Float;

                case VertexElementFormat.Vector3:
                    return TexCoordPointerType.Float;

                case VertexElementFormat.Vector4:
                    return TexCoordPointerType.Float;

                case VertexElementFormat.Color:
                    return TexCoordPointerType.Float;

                case VertexElementFormat.Byte4:
                    return TexCoordPointerType.Float;

                case VertexElementFormat.Short2:
                    return TexCoordPointerType.Short;

                case VertexElementFormat.Short4:
                    return TexCoordPointerType.Short;

                case VertexElementFormat.NormalizedShort2:
                    return TexCoordPointerType.Short;

                case VertexElementFormat.NormalizedShort4:
                    return TexCoordPointerType.Short;
				
#if MONOMAC
                case VertexElementFormat.HalfVector2:
                    return TexCoordPointerType.HalfFloat;

                case VertexElementFormat.HalfVector4:
                    return TexCoordPointerType.HalfFloat;
#endif
			}

            throw new NotImplementedException();
        }

		
		public static BlendEquationMode GetBlendEquationMode (this BlendFunction function)
		{
			switch (function) {
			case BlendFunction.Add:
				return BlendEquationMode.FuncAdd;
#if IPHONE
			case BlendFunction.Max:
				return BlendEquationMode.MaxExt;
			case BlendFunction.Min:
				return BlendEquationMode.MinExt;
#elif MONOMAC
			case BlendFunction.Max:
				return BlendEquationMode.Max;
			case BlendFunction.Min:
				return BlendEquationMode.Min;
#endif
			case BlendFunction.ReverseSubtract:
				return BlendEquationMode.FuncReverseSubtract;
			case BlendFunction.Subtract:
				return BlendEquationMode.FuncSubtract;

			default:
                throw new NotImplementedException();
			}
		}

		public static BlendingFactorSrc GetBlendFactorSrc (this Blend blend)
		{
			switch (blend) {
			case Blend.DestinationAlpha:
				return BlendingFactorSrc.DstAlpha;
			case Blend.DestinationColor:
				return BlendingFactorSrc.DstColor;
			case Blend.InverseDestinationAlpha:
				return BlendingFactorSrc.OneMinusDstAlpha;
			case Blend.InverseDestinationColor:
				return BlendingFactorSrc.OneMinusDstColor;
			case Blend.InverseSourceAlpha:
				return BlendingFactorSrc.OneMinusSrcAlpha;
			case Blend.InverseSourceColor:
#if MONOMAC || WINDOWS || LINUX
				return (BlendingFactorSrc)All.OneMinusSrcColor;
#else
				return BlendingFactorSrc.OneMinusSrcColor;
#endif
			case Blend.One:
				return BlendingFactorSrc.One;
			case Blend.SourceAlpha:
				return BlendingFactorSrc.SrcAlpha;
			case Blend.SourceAlphaSaturation:
				return BlendingFactorSrc.SrcAlphaSaturate;
			case Blend.SourceColor:
#if MONOMAC || WINDOWS || LINUX
				return (BlendingFactorSrc)All.SrcColor;
#else
				return BlendingFactorSrc.SrcColor;
#endif
			case Blend.Zero:
				return BlendingFactorSrc.Zero;
			default:
				return BlendingFactorSrc.One;
			}

		}

		public static BlendingFactorDest GetBlendFactorDest (this Blend blend)
		{
			switch (blend) {
			case Blend.DestinationAlpha:
				return BlendingFactorDest.DstAlpha;
//			case Blend.DestinationColor:
//				return BlendingFactorDest.DstColor;
			case Blend.InverseDestinationAlpha:
				return BlendingFactorDest.OneMinusDstAlpha;
//			case Blend.InverseDestinationColor:
//				return BlendingFactorDest.OneMinusDstColor;
			case Blend.InverseSourceAlpha:
				return BlendingFactorDest.OneMinusSrcAlpha;
			case Blend.InverseSourceColor:
#if MONOMAC || WINDOWS
				return (BlendingFactorDest)All.OneMinusSrcColor;
#else
				return BlendingFactorDest.OneMinusSrcColor;
#endif
			case Blend.One:
				return BlendingFactorDest.One;
			case Blend.SourceAlpha:
				return BlendingFactorDest.SrcAlpha;
//			case Blend.SourceAlphaSaturation:
//				return BlendingFactorDest.SrcAlphaSaturate;
			case Blend.SourceColor:
#if MONOMAC || WINDOWS
				return (BlendingFactorDest)All.SrcColor;
#else
				return BlendingFactorDest.SrcColor;
#endif
			case Blend.Zero:
				return BlendingFactorDest.Zero;
			default:
				return BlendingFactorDest.One;
			}

		}
		
		
		internal static void GetGLFormat(this SurfaceFormat format,
		                                 out PixelInternalFormat glInternalFormat,
		                                 out PixelFormat glFormat,
		                                 out PixelType glType)
		{
			glInternalFormat = PixelInternalFormat.Rgba;
			glFormat = PixelFormat.Rgba;
			glType = PixelType.UnsignedByte;
			
			switch (format)
			{
			case SurfaceFormat.Color:
				glInternalFormat = PixelInternalFormat.Rgba;
				glFormat = PixelFormat.Rgba;
				glType = PixelType.UnsignedByte;
				break;
			case SurfaceFormat.Bgr565:
				glInternalFormat = PixelInternalFormat.Rgb;
				glFormat = PixelFormat.Rgb;
				glType = PixelType.UnsignedShort565;
				break;
			case SurfaceFormat.Bgra4444:
#if IPHONE
				glInternalFormat = PixelInternalFormat.Rgba;
#else
				glInternalFormat = PixelInternalFormat.Rgba4;
#endif
				glFormat = PixelFormat.Rgba;
				glType = PixelType.UnsignedShort4444;
				break;
			case SurfaceFormat.Bgra5551:
				glInternalFormat = PixelInternalFormat.Rgba;
				glFormat = PixelFormat.Rgba;
				glType = PixelType.UnsignedShort5551;
				break;
			case SurfaceFormat.Alpha8:
				glInternalFormat = PixelInternalFormat.Luminance;
				glFormat = PixelFormat.Luminance;
				glType = PixelType.UnsignedByte;
				break;
#if !IPHONE && !ANDROID
			case SurfaceFormat.Dxt1:
				glInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
				glFormat = (PixelFormat)All.CompressedTextureFormats;
				break;
			case SurfaceFormat.Dxt3:
				glInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
				glFormat = (PixelFormat)All.CompressedTextureFormats;
				break;
			case SurfaceFormat.Dxt5:
				glInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
				glFormat = (PixelFormat)All.CompressedTextureFormats;
				break;
#endif
				
#if IPHONE || ANDROID
			case SurfaceFormat.RgbPvrtc2Bpp:
				glInternalFormat = PixelInternalFormat.CompressedRgbPvrtc2Bppv1Img;
				glFormat = (PixelFormat)All.CompressedTextureFormats;
				break;
			case SurfaceFormat.RgbPvrtc4Bpp:
				glInternalFormat = PixelInternalFormat.CompressedRgbPvrtc4Bppv1Img;
				glFormat = (PixelFormat)All.CompressedTextureFormats;
				break;
			case SurfaceFormat.RgbaPvrtc2Bpp:
				glInternalFormat = PixelInternalFormat.CompressedRgbaPvrtc2Bppv1Img;
				glFormat = (PixelFormat)All.CompressedTextureFormats;
				break;
			case SurfaceFormat.RgbaPvrtc4Bpp:
				glInternalFormat = PixelInternalFormat.CompressedRgbaPvrtc4Bppv1Img;
				glFormat = (PixelFormat)All.CompressedTextureFormats;
				break;
#endif
			default:
				throw new NotSupportedException();
			}
		}

#endif

        public static int Size(this SurfaceFormat surfaceFormat)
        {
            switch (surfaceFormat)
            {
                case SurfaceFormat.Color:
                    return 4;
                case SurfaceFormat.Dxt3:
                    return 4;
                case SurfaceFormat.Bgra4444:
                    return 2;
                case SurfaceFormat.Bgra5551:
                    return 2;
                case SurfaceFormat.Alpha8:
                    return 1;
				case SurfaceFormat.NormalizedByte4:
					return 4;
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
    }
}
