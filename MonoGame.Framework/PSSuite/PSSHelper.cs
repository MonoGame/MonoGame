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
#warning Not Implemented
            throw new NotImplementedException();
        }
	}
}
