// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

#if WINDOWS
using FreeImageAPI;
#endif

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reading texture files for use in the Content Pipeline.
    /// </summary>
    [ContentImporter(".png", ".jpg", ".bmp", DisplayName = "Texture Importer - MonoGame", DefaultProcessor = "TextureProcessor")]
    public class TextureImporter : ContentImporter<TextureContent>
    {
        /// <summary>
        /// Initializes a new instance of TextureImporter.
        /// </summary>
        public TextureImporter()
        {
        }

        /// <summary>
        /// Called by the XNA Framework when importing a texture file to be used as a game asset. This is the method called by the XNA Framework when an asset is to be imported into an object that can be recognized by the Content Pipeline.
        /// </summary>
        /// <param name="filename">Name of a game asset file.</param>
        /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
        /// <returns>Resulting game asset.</returns>
        public override TextureContent Import (string filename, ContentImporterContext context)
		{
			var output = new Texture2DContent ();

#if WINDOWS

            // TODO: This is a pretty lame way to do this. It would be better
            // if we could completely get rid of the System.Drawing.Bitmap
            // class and replace it with FreeImage, but some other processors/importers
            // such as Font rely on Bitmap. Soon, we should completely remove any references
            // to System.Drawing.Bitmap and replace it with FreeImage. For now
            // this is the quickest way to add support for virtually every input Texture
            // format without breaking functionality in other places.
            var format = FREE_IMAGE_FORMAT.FIF_UNKNOWN;
            var fiBitmap = FreeImage.LoadEx(filename, ref format);

            output._bitmap = FreeImage.GetBitmap(fiBitmap);
            FreeImage.UnloadEx(ref fiBitmap);
#else
            output._bitmap = new Bitmap(filename);
#endif

            var height = output._bitmap.Height;
            var width = output._bitmap.Width;

            // Force the input's pixelformat to ARGB32, so we can have a common pixel format to deal with.
			if (output._bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb) {

				var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				using ( var graphics = System.Drawing.Graphics.FromImage(bitmap)) {
					graphics.DrawImage(output._bitmap, 0,0, width, height);
				}

				output._bitmap = bitmap;
			}

			var imageData = output._bitmap.GetData();

            var bitmapContent = new PixelBitmapContent<Color>(width, height);
            bitmapContent.SetPixelData(imageData);

            output.Faces.Add(new MipmapChain(bitmapContent));

            return output;
        }
    }
}
