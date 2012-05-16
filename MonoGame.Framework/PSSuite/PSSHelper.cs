using System;

using PssMatrix4 = Sce.Pss.Core.Matrix4;
using PssPixelFormat = Sce.Pss.Core.Graphics.PixelFormat;
using PssVertexFormat = Sce.Pss.Core.Graphics.VertexFormat;
using PssDrawMode = Sce.Pss.Core.Graphics.DrawMode;

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

            throw new NotImplementedException();
        }
        
        public static PssVertexFormat ToVertexFormat(VertexElementFormat format)
        {
            switch (format)
            {
            case VertexElementFormat.Vector3:
                return PssVertexFormat.Float3;
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
