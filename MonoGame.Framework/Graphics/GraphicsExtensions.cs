// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

#if OPENGL
#if DESKTOPGL || GLES
using MonoGame.OpenGL;
using GLPixelFormat = MonoGame.OpenGL.PixelFormat;
using PixelFormat = MonoGame.OpenGL.PixelFormat;
#elif ANGLE
using OpenTK.Graphics;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    static class GraphicsExtensions
    {
#if OPENGL
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
                    return 4;

                case VertexElementFormat.NormalizedShort2:
                    return 2;

                case VertexElementFormat.NormalizedShort4:
                    return 4;

                case VertexElementFormat.HalfVector2:
                    return 2;

                case VertexElementFormat.HalfVector4:
                    return 4;
            }

            throw new ArgumentException();
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

            throw new ArgumentException();
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
                
#if WINDOWS || DESKTOPGL
               case VertexElementFormat.HalfVector2:
                    return VertexAttribPointerType.HalfFloat;

                case VertexElementFormat.HalfVector4:
                    return VertexAttribPointerType.HalfFloat;
#endif
            }

            throw new ArgumentException();
        }

        public static bool OpenGLVertexAttribNormalized(this VertexElement element)
        {
            // TODO: This may or may not be the right behavor.  
            //
            // For instance the VertexElementFormat.Byte4 format is not supposed
            // to be normalized, but this line makes it so.
            //
            // The question is in MS XNA are types normalized based on usage or
            // normalized based to their format?
            //
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

            throw new ArgumentException();
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

            throw new ArgumentException();
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

            throw new ArgumentException();
        }

		
		public static BlendEquationMode GetBlendEquationMode (this BlendFunction function)
		{
			switch (function) {
			case BlendFunction.Add:
				return BlendEquationMode.FuncAdd;
#if WINDOWS || DESKTOPGL || IOS
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
                throw new ArgumentException();
			}
		}

		public static BlendingFactorSrc GetBlendFactorSrc (this Blend blend)
		{
			switch (blend) {
            case Blend.BlendFactor:
                return BlendingFactorSrc.ConstantColor;
			case Blend.DestinationAlpha:
				return BlendingFactorSrc.DstAlpha;
			case Blend.DestinationColor:
				return BlendingFactorSrc.DstColor;
            case Blend.InverseBlendFactor:
                return BlendingFactorSrc.OneMinusConstantColor;
			case Blend.InverseDestinationAlpha:
				return BlendingFactorSrc.OneMinusDstAlpha;
			case Blend.InverseDestinationColor:
				return BlendingFactorSrc.OneMinusDstColor;
			case Blend.InverseSourceAlpha:
				return BlendingFactorSrc.OneMinusSrcAlpha;
			case Blend.InverseSourceColor:
                return BlendingFactorSrc.OneMinusSrcColor;
			case Blend.One:
				return BlendingFactorSrc.One;
			case Blend.SourceAlpha:
				return BlendingFactorSrc.SrcAlpha;
			case Blend.SourceAlphaSaturation:
				return BlendingFactorSrc.SrcAlphaSaturate;
			case Blend.SourceColor:
				return BlendingFactorSrc.SrcColor;
			case Blend.Zero:
				return BlendingFactorSrc.Zero;
            default:
                throw new ArgumentOutOfRangeException("blend", "The specified blend function is not implemented.");
            }

		}

		public static BlendingFactorDest GetBlendFactorDest (this Blend blend)
		{
			switch (blend) {
            case Blend.BlendFactor:
                return BlendingFactorDest.ConstantColor;
            case Blend.DestinationAlpha:
                return BlendingFactorDest.DstAlpha;
            case Blend.DestinationColor:
                return BlendingFactorDest.DstColor;
            case Blend.InverseBlendFactor:
                return BlendingFactorDest.OneMinusConstantColor;
            case Blend.InverseDestinationAlpha:
				return BlendingFactorDest.OneMinusDstAlpha;
            case Blend.InverseDestinationColor:
                return BlendingFactorDest.OneMinusDstColor;
            case Blend.InverseSourceAlpha:
                return BlendingFactorDest.OneMinusSrcAlpha;
			case Blend.InverseSourceColor:
				return BlendingFactorDest.OneMinusSrcColor;
			case Blend.One:
				return BlendingFactorDest.One;
			case Blend.SourceAlpha:
				return BlendingFactorDest.SrcAlpha;
            case Blend.SourceAlphaSaturation:
                return BlendingFactorDest.SrcAlphaSaturate;
            case Blend.SourceColor:
			    return BlendingFactorDest.SrcColor;
			case Blend.Zero:
				return BlendingFactorDest.Zero;
			default:
				throw new ArgumentOutOfRangeException("blend", "The specified blend function is not implemented.");
			}

		}

        public static DepthFunction GetDepthFunction(this CompareFunction compare)
        {
            switch (compare)
            {
                default:
                case CompareFunction.Always:
                    return DepthFunction.Always;
                case CompareFunction.Equal:
                    return DepthFunction.Equal;
                case CompareFunction.Greater:
                    return DepthFunction.Greater;
                case CompareFunction.GreaterEqual:
                    return DepthFunction.Gequal;
                case CompareFunction.Less:
                    return DepthFunction.Less;
                case CompareFunction.LessEqual:
                    return DepthFunction.Lequal;
                case CompareFunction.Never:
                    return DepthFunction.Never;
                case CompareFunction.NotEqual:
                    return DepthFunction.Notequal;
            }
        }

#if WINDOWS || DESKTOPGL || ANGLE
        /// <summary>
        /// Convert a <see cref="SurfaceFormat"/> to an OpenTK.Graphics.ColorFormat.
        /// This is used for setting up the backbuffer format of the OpenGL context.
        /// </summary>
        /// <returns>An OpenTK.Graphics.ColorFormat instance.</returns>
        /// <param name="format">The <see cref="SurfaceFormat"/> to convert.</param>
        internal static ColorFormat GetColorFormat(this SurfaceFormat format)
        {
            switch (format)
            {
                case SurfaceFormat.Alpha8:
                    return new ColorFormat(0, 0, 0, 8);
                case SurfaceFormat.Bgr565:
                    return new ColorFormat(5, 6, 5, 0);
                case SurfaceFormat.Bgra4444:
                    return new ColorFormat(4, 4, 4, 4);
                case SurfaceFormat.Bgra5551:
                    return new ColorFormat(5, 5, 5, 1);
                case SurfaceFormat.Bgr32:
                    return new ColorFormat(8, 8, 8, 0);
                case SurfaceFormat.Bgra32:
                case SurfaceFormat.Color:
                case SurfaceFormat.ColorSRgb:
                    return new ColorFormat(8, 8, 8, 8);
                case SurfaceFormat.Rgba1010102:
                    return new ColorFormat(10, 10, 10, 2);
                default:
                    // Floating point backbuffers formats could be implemented
                    // but they are not typically used on the backbuffer. In
                    // those cases it is better to create a render target instead.
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Converts <see cref="PresentInterval"/> to OpenGL swap interval.
        /// </summary>
        /// <returns>A value according to EXT_swap_control</returns>
        /// <param name="interval">The <see cref="PresentInterval"/> to convert.</param>
        internal static int GetSwapInterval(this PresentInterval interval)
        {
            // See http://www.opengl.org/registry/specs/EXT/swap_control.txt
            // and https://www.opengl.org/registry/specs/EXT/glx_swap_control_tear.txt
            // OpenTK checks for EXT_swap_control_tear:
            // if supported, a swap interval of -1 enables adaptive vsync;
            // otherwise -1 is converted to 1 (vsync enabled.)

            switch (interval)
            {

                case PresentInterval.Immediate:
                    return 0;
                case PresentInterval.One:
                    return 1;
                case PresentInterval.Two:
                    return 2;
                case PresentInterval.Default:
                default:
                    return -1;
            }
        }
#endif

        const SurfaceFormat InvalidFormat = (SurfaceFormat)int.MaxValue;
		internal static void GetGLFormat (this SurfaceFormat format,
            GraphicsDevice graphicsDevice,
            out PixelInternalFormat glInternalFormat,
            out PixelFormat glFormat,
            out PixelType glType)
		{
			glInternalFormat = PixelInternalFormat.Rgba;
			glFormat = PixelFormat.Rgba;
			glType = PixelType.UnsignedByte;

		    var supportsSRgb = graphicsDevice.GraphicsCapabilities.SupportsSRgb;
            var supportsS3tc = graphicsDevice.GraphicsCapabilities.SupportsS3tc;
            var supportsPvrtc = graphicsDevice.GraphicsCapabilities.SupportsPvrtc;
            var supportsEtc1 = graphicsDevice.GraphicsCapabilities.SupportsEtc1;
            var supportsAtitc = graphicsDevice.GraphicsCapabilities.SupportsAtitc;
            var supportsFloat = graphicsDevice.GraphicsCapabilities.SupportsFloatTextures;
            var supportsHalfFloat = graphicsDevice.GraphicsCapabilities.SupportsHalfFloatTextures;
            var supportsNormalized = graphicsDevice.GraphicsCapabilities.SupportsNormalized;
            var isGLES2 = GL.BoundApi == GL.RenderApi.ES && graphicsDevice.glMajorVersion == 2;

			switch (format) {
			case SurfaceFormat.Color:
				glInternalFormat = PixelInternalFormat.Rgba;
				glFormat = PixelFormat.Rgba;
				glType = PixelType.UnsignedByte;
				break;
            case SurfaceFormat.ColorSRgb:
                if (!supportsSRgb)
                    goto case SurfaceFormat.Color;
                glInternalFormat = PixelInternalFormat.Srgb;
                glFormat = PixelFormat.Rgba;
                glType = PixelType.UnsignedByte;
                break;
			case SurfaceFormat.Bgr565:
				glInternalFormat = PixelInternalFormat.Rgb;
				glFormat = PixelFormat.Rgb;
				glType = PixelType.UnsignedShort565;
				break;
			case SurfaceFormat.Bgra4444:
#if IOS || ANDROID
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
			case SurfaceFormat.Dxt1:
                if (!supportsS3tc)
                    goto case InvalidFormat;
				glInternalFormat = PixelInternalFormat.CompressedRgbS3tcDxt1Ext;
                glFormat = (PixelFormat)GLPixelFormat.CompressedTextureFormats;
				break;
            case SurfaceFormat.Dxt1SRgb:
                if (!supportsSRgb)
                    goto case SurfaceFormat.Dxt1;
                glInternalFormat = PixelInternalFormat.CompressedSrgbS3tcDxt1Ext;
                glFormat = (PixelFormat)GLPixelFormat.CompressedTextureFormats;
                break;
            case SurfaceFormat.Dxt1a:
                if (!supportsS3tc)
                    goto case InvalidFormat;
                glInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                glFormat = (PixelFormat)GLPixelFormat.CompressedTextureFormats;
                break;
            case SurfaceFormat.Dxt3:
                if (!supportsS3tc)
                    goto case InvalidFormat;
				glInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                glFormat = (PixelFormat)GLPixelFormat.CompressedTextureFormats;
				break;
            case SurfaceFormat.Dxt3SRgb:
                if (!supportsSRgb)
                    goto case SurfaceFormat.Dxt3;
                glInternalFormat = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
                glFormat = (PixelFormat)GLPixelFormat.CompressedTextureFormats;
                break;
			case SurfaceFormat.Dxt5:
                if (!supportsS3tc)
                    goto case InvalidFormat;
				glInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                glFormat = (PixelFormat)GLPixelFormat.CompressedTextureFormats;
				break;
            case SurfaceFormat.Dxt5SRgb:
                if (!supportsSRgb)
                    goto case SurfaceFormat.Dxt5;
                glInternalFormat = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
                glFormat = (PixelFormat)GLPixelFormat.CompressedTextureFormats;
                break;
#if !IOS && !ANDROID && !ANGLE
            case SurfaceFormat.Rgba1010102:
                glInternalFormat = PixelInternalFormat.Rgb10A2ui;
                glFormat = PixelFormat.Rgba;
                glType = PixelType.UnsignedInt1010102;
                break;
#endif
            case SurfaceFormat.Single:
                if (!supportsFloat)
                    goto case InvalidFormat;
                glInternalFormat = PixelInternalFormat.R32f;
                glFormat = PixelFormat.Red;
                glType = PixelType.Float;
                break;

            case SurfaceFormat.HalfVector2:
                if (!supportsHalfFloat)
                    goto case InvalidFormat;
                glInternalFormat = PixelInternalFormat.Rg16f;
				glFormat = PixelFormat.Rg;
				glType = PixelType.HalfFloat;
                break;

            // HdrBlendable implemented as HalfVector4 (see http://blogs.msdn.com/b/shawnhar/archive/2010/07/09/surfaceformat-hdrblendable.aspx)
            case SurfaceFormat.HdrBlendable:
            case SurfaceFormat.HalfVector4:
                if (!supportsHalfFloat)
                    goto case InvalidFormat;
                glInternalFormat = PixelInternalFormat.Rgba16f;
                glFormat = PixelFormat.Rgba;
                glType = PixelType.HalfFloat;
                break;

            case SurfaceFormat.HalfSingle:
                if (!supportsHalfFloat)
                    goto case InvalidFormat;
                glInternalFormat = PixelInternalFormat.R16f;
                glFormat = PixelFormat.Red;
                glType = isGLES2 ? PixelType.HalfFloatOES : PixelType.HalfFloat;
                break;

            case SurfaceFormat.Vector2:
                if (!supportsFloat)
                    goto case InvalidFormat;
                glInternalFormat = PixelInternalFormat.Rg32f;
                glFormat = PixelFormat.Rg;
                glType = PixelType.Float;
                break;

            case SurfaceFormat.Vector4:
                if (!supportsFloat)
                    goto case InvalidFormat;
                glInternalFormat = PixelInternalFormat.Rgba32f;
                glFormat = PixelFormat.Rgba;
                glType = PixelType.Float;
                break;

            case SurfaceFormat.NormalizedByte2:
                glInternalFormat = PixelInternalFormat.Rg8i;
                glFormat = PixelFormat.Rg;
                glType = PixelType.Byte;
                break;

            case SurfaceFormat.NormalizedByte4:
                glInternalFormat = PixelInternalFormat.Rgba8i;
                glFormat = PixelFormat.Rgba;
                glType = PixelType.Byte;
                break;

            case SurfaceFormat.Rg32:
                if (!supportsNormalized)
                    goto case InvalidFormat;
                glInternalFormat = PixelInternalFormat.Rg16ui;
                glFormat = PixelFormat.Rg;
                glType = PixelType.UnsignedShort;
                break;

            case SurfaceFormat.Rgba64:
                if (!supportsNormalized)
                    goto case InvalidFormat;
                glInternalFormat = PixelInternalFormat.Rgba16;
                glFormat = PixelFormat.Rgba;
                glType = PixelType.UnsignedShort;
                break;
            case SurfaceFormat.RgbaAtcExplicitAlpha:
                if (!supportsAtitc)
                    goto case InvalidFormat;
				glInternalFormat = PixelInternalFormat.AtcRgbaExplicitAlphaAmd;
				glFormat = PixelFormat.CompressedTextureFormats;
				break;
            case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                if (!supportsAtitc)
                    goto case InvalidFormat;
				glInternalFormat = PixelInternalFormat.AtcRgbaInterpolatedAlphaAmd;
				glFormat = PixelFormat.CompressedTextureFormats;
				break;
            case SurfaceFormat.RgbEtc1:
                if (!supportsEtc1)
                    goto case InvalidFormat;
                glInternalFormat = PixelInternalFormat.Etc1; // GL_ETC1_RGB8_OES
                glFormat = PixelFormat.CompressedTextureFormats;
                break;
			case SurfaceFormat.RgbPvrtc2Bpp:
                if (!supportsPvrtc)
                    goto case InvalidFormat;
				glInternalFormat = PixelInternalFormat.CompressedRgbPvrtc2Bppv1Img;
				glFormat = PixelFormat.CompressedTextureFormats;
				break;
			case SurfaceFormat.RgbPvrtc4Bpp:
                if (!supportsPvrtc)
                    goto case InvalidFormat;
				glInternalFormat = PixelInternalFormat.CompressedRgbPvrtc4Bppv1Img;
				glFormat = PixelFormat.CompressedTextureFormats;
				break;
			case SurfaceFormat.RgbaPvrtc2Bpp:
                if (!supportsPvrtc)
                    goto case InvalidFormat;
				glInternalFormat = PixelInternalFormat.CompressedRgbaPvrtc2Bppv1Img;
				glFormat = PixelFormat.CompressedTextureFormats;
				break;
			case SurfaceFormat.RgbaPvrtc4Bpp:
                if (!supportsPvrtc)
                    goto case InvalidFormat;
				glInternalFormat = PixelInternalFormat.CompressedRgbaPvrtc4Bppv1Img;
				glFormat = PixelFormat.CompressedTextureFormats;
				break;
            case InvalidFormat: 
            default:
                    throw new NotSupportedException(string.Format("The requested SurfaceFormat `{0}` is not supported.", format));
			}
		}

#endif // OPENGL

                    public static int GetSyncInterval(this PresentInterval interval)
        {
            switch (interval)
            {
                case PresentInterval.Immediate:
                    return 0;

                case PresentInterval.Two:
                    return 2;

                default:
                    return 1;
            }
        }

        public static bool IsCompressedFormat(this SurfaceFormat format)
        {
            switch (format)
            {
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1a:
                case SurfaceFormat.Dxt1SRgb:
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt3SRgb:
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.Dxt5SRgb:
                case SurfaceFormat.RgbaAtcExplicitAlpha:
                case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                case SurfaceFormat.RgbaPvrtc2Bpp:
                case SurfaceFormat.RgbaPvrtc4Bpp:
                case SurfaceFormat.RgbEtc1:
                case SurfaceFormat.RgbPvrtc2Bpp:
                case SurfaceFormat.RgbPvrtc4Bpp:
                    return true;
            }
            return false;
        }

        public static int GetSize(this SurfaceFormat surfaceFormat)
        {
            switch (surfaceFormat)
            {
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1SRgb:
                case SurfaceFormat.Dxt1a:
                case SurfaceFormat.RgbPvrtc2Bpp:
                case SurfaceFormat.RgbaPvrtc2Bpp:
                case SurfaceFormat.RgbPvrtc4Bpp:
                case SurfaceFormat.RgbaPvrtc4Bpp:
                case SurfaceFormat.RgbEtc1:
                    // One texel in DXT1, PVRTC (2bpp and 4bpp) and ETC1 is a minimum 4x4 block (8x4 for PVRTC 2bpp), which is 8 bytes
                    return 8;
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt3SRgb:
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.Dxt5SRgb:
                case SurfaceFormat.RgbaAtcExplicitAlpha:
                case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                    // One texel in DXT3 and DXT5 is a minimum 4x4 block, which is 16 bytes
                    return 16;
                case SurfaceFormat.Alpha8:
                    return 1;
                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgra4444:
                case SurfaceFormat.Bgra5551:
                case SurfaceFormat.HalfSingle:
                case SurfaceFormat.NormalizedByte2:
                    return 2;
                case SurfaceFormat.Color:
                case SurfaceFormat.ColorSRgb:
                case SurfaceFormat.Single:
                case SurfaceFormat.Rg32:
                case SurfaceFormat.HalfVector2:
                case SurfaceFormat.NormalizedByte4:
                case SurfaceFormat.Rgba1010102:
                case SurfaceFormat.Bgra32:
                case SurfaceFormat.Bgra32SRgb:
                case SurfaceFormat.Bgr32:
                case SurfaceFormat.Bgr32SRgb:
                    return 4;
                case SurfaceFormat.HalfVector4:
                case SurfaceFormat.Rgba64:
                case SurfaceFormat.Vector2:
                    return 8;
                case SurfaceFormat.Vector4:
                    return 16;
                default:
                    throw new ArgumentException();
            }
        }

        public static int GetSize(this VertexElementFormat elementFormat)
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
                    return 16;

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

        public static void GetBlockSize(this SurfaceFormat surfaceFormat, out int width, out int height)
        {
            switch (surfaceFormat)
            {
                case SurfaceFormat.RgbPvrtc2Bpp:
                case SurfaceFormat.RgbaPvrtc2Bpp:
                    width = 8;
                    height = 4;
                    break;
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1SRgb:
                case SurfaceFormat.Dxt1a:
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt3SRgb:
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.Dxt5SRgb:
                case SurfaceFormat.RgbPvrtc4Bpp:
                case SurfaceFormat.RgbaPvrtc4Bpp:
                case SurfaceFormat.RgbEtc1:
                case SurfaceFormat.RgbaAtcExplicitAlpha:
                case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                    width = 4;
                    height = 4;
                    break;
                default:
                    width = 1;
                    height = 1;
                    break;
            }
        }

#if OPENGL

        public static int GetBoundTexture2D()
        {
            var prevTexture = 0;
            GL.GetInteger(GetPName.TextureBinding2D, out prevTexture);
            GraphicsExtensions.LogGLError("GraphicsExtensions.GetBoundTexture2D() GL.GetInteger");
            return prevTexture;
        }

        [Conditional("DEBUG")]
		[DebuggerHidden]
        public static void CheckGLError()
        {
           var error = GL.GetError();
            //Console.WriteLine(error);
            if (error != ErrorCode.NoError)
                throw new MonoGameGLException("GL.GetError() returned " + error.ToString());
        }
#endif

#if OPENGL
        [Conditional("DEBUG")]
        public static void LogGLError(string location)
        {
            try
            {
                GraphicsExtensions.CheckGLError();
            }
            catch (MonoGameGLException ex)
            {
#if ANDROID
                // Todo: Add generic MonoGame logging interface
                Android.Util.Log.Debug("MonoGame", "MonoGameGLException at " + location + " - " + ex.Message);
#else
                Debug.WriteLine("MonoGameGLException at " + location + " - " + ex.Message);
#endif
            }
        }
#endif
            }

    internal class MonoGameGLException : Exception
    {
        public MonoGameGLException(string message)
            : base(message)
        {
        }
    }
}
