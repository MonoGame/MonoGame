using System;

using PssMatrix4 = Sce.PlayStation.Core.Matrix4;
using PssPixelFormat = Sce.PlayStation.Core.Graphics.PixelFormat;
using PssVertexFormat = Sce.PlayStation.Core.Graphics.VertexFormat;
using PssDrawMode = Sce.PlayStation.Core.Graphics.DrawMode;
using Sce.PlayStation.Core.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
	internal static class PSSHelper
	{
		public static PssPixelFormat ToFormat(SurfaceFormat format)
		{
#warning This is not fully implemented at all! TODO: Do something like SharpDXHelper.ToFormat
			return PssPixelFormat.Rgba;
		}
        
        public static PssDrawMode ToDrawMode(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return PssDrawMode.Lines;
                case PrimitiveType.LineStrip:
                    return PssDrawMode.LineStrip;
                case PrimitiveType.TriangleList:
                    return PssDrawMode.Triangles;
                case PrimitiveType.TriangleStrip:
                    return PssDrawMode.TriangleStrip;
            }

            throw new ArgumentException();
        }
        
        public static BlendFuncFactor ToBlendFuncFactor(Blend format)
        {
            switch(format)
            {
                case Blend.DestinationAlpha :
                    return BlendFuncFactor.DstAlpha;
                case Blend.DestinationColor :
                    return BlendFuncFactor.DstColor;
                case Blend.InverseBlendFactor :
                    throw new NotSupportedException();
                case Blend.InverseDestinationAlpha :
                    return BlendFuncFactor.OneMinusDstAlpha;
                case Blend.InverseDestinationColor :
                    return BlendFuncFactor.OneMinusDstColor;
                case Blend.InverseSourceAlpha :
                    return BlendFuncFactor.OneMinusSrcAlpha;
                case Blend.InverseSourceColor :
                    return BlendFuncFactor.OneMinusSrcColor;
                case Blend.One :
                    return BlendFuncFactor.One;
                case Blend.SourceAlpha :
                    return BlendFuncFactor.SrcAlpha;
                case Blend.SourceAlphaSaturation :
                    return BlendFuncFactor.SrcAlphaSaturate;
                case Blend.SourceColor :
                    return BlendFuncFactor.SrcColor;
                case Blend.Zero :
                    return BlendFuncFactor.Zero;
            }
            throw new ArgumentException();
        }
        
        public static BlendFuncMode ToBlendFuncMode(BlendFunction format)
        {
            switch(format)
            {
                case BlendFunction.Add  :
                    return BlendFuncMode.Add;
                case BlendFunction.ReverseSubtract  :
                    return BlendFuncMode.ReverseSubtract;
                case BlendFunction.Subtract  :
                    return BlendFuncMode.Subtract;    
            }
            throw new ArgumentException();
        }
        
        public static PssVertexFormat ToVertexFormat(VertexElementFormat format)
        {
            switch (format)
            {
            case VertexElementFormat.Single:
                return PssVertexFormat.Float;
            case VertexElementFormat.Vector2:
                return PssVertexFormat.Float2;
            case VertexElementFormat.Vector3:
                return PssVertexFormat.Float3;
            case VertexElementFormat.Vector4:
                return PssVertexFormat.Float4;
            case VertexElementFormat.Color:
                return PssVertexFormat.UByte4N;
            default:
#warning Plenty more formats still to implement
                throw new NotImplementedException();
            }
        }
        
        /// <summary>
        /// Transforms a float[] (As inside EffectParameters["WorldViewProj"].Data) into a PssMatrix4
        /// </summary>
        public static PssMatrix4 ToPssMatrix4(float[] flatMatrix)
        {
            return new PssMatrix4(flatMatrix[0], flatMatrix[1], flatMatrix[2], flatMatrix[3], flatMatrix[4], flatMatrix[5], flatMatrix[6], flatMatrix[7], flatMatrix[8], flatMatrix[9], flatMatrix[10], flatMatrix[11], flatMatrix[12], flatMatrix[13], flatMatrix[14], flatMatrix[15]);
        }
	}
}
