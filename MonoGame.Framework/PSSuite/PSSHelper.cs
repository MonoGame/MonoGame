using System;

using PssPixelFormat = Sce.Pss.Core.Graphics.PixelFormat;
using PssVertexFormat = Sce.Pss.Core.Graphics.VertexFormat;

namespace Microsoft.Xna.Framework.Graphics
{
	internal static class PSSHelper
	{
		public static PssPixelFormat ToFormat(SurfaceFormat format)
		{
#warning This is not fully implemented at all! TODO: Do something like SharpDXHelper.ToFormat
			return PssPixelFormat.Rgba;
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
	}
}
