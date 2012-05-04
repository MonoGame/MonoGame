using System;

using PssPixelFormat = Sce.Pss.Core.Graphics.PixelFormat;
using PssVertexFormat = Sce.Pss.Core.Graphics.VertexFormat;

namespace Microsoft.Xna.Framework.Graphics
{
	internal static class PSSHelper
	{
		public static PssPixelFormat ToFormat(SurfaceFormat format)
		{
#warning This is not fully implemented. Do something like SharpDXHelper.ToFormat
			return PssPixelFormat.Rgba;
		}
        
        public static PssVertexFormat ToVertexFormat(VertexElementFormat format)
        {
            switch (format)
            {
            case VertexElementFormat.Vector3:
                return PssVertexFormat.Float3;
            case VertexElementFormat.Color:
                return PssVertexFormat.UByte4N; //FIXME: The samples all take Float4 but the actual data is uint/Byte4
            default:
                throw new NotImplementedException();
            }
        }
	}
}
