using System;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class TextureCubeReader : ContentTypeReader<TextureCube>
	{
		
		protected internal override TextureCube Read(ContentReader reader, TextureCube existingInstance)
		{
			SurfaceFormat surfaceFormat = (SurfaceFormat)reader.ReadInt32 ();
			int size = reader.ReadInt32 ();
			int levels = reader.ReadInt32 ();
			TextureCube textureCube = new TextureCube(reader.GraphicsDevice, size, levels > 1, surfaceFormat);
			for (int face = 0; face < 6; face++) {
				for (int i=0; i<levels; i++) {
					int faceSize = reader.ReadInt32();
					byte[] faceData = reader.ReadBytes(faceSize);
					textureCube.SetData<byte>((CubeMapFace)face, i, null, faceData, 0, faceSize);
				}
			}
			return textureCube;
		}
	}
}
