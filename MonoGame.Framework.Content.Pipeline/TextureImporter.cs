using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    [ContentImporter(".png", DisplayName="MGTextureImporter", DefaultProcessor="TextureProcessor")]
    public class TextureImporter : ContentImporter<TextureContent>
    {
        public override TextureContent Import(string filename, ContentImporterContext context)
        {
            var output = new Texture2DContent();
            var bmp = new Bitmap(filename);
            
            var bitmapContent = new PixelBitmapContent<Color>(bmp.Width, bmp.Height);


            var bitmapData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                                                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                System.Drawing.Imaging.PixelFormat.DontCare);

            var length = bitmapData.Stride * bitmapData.Height;
            var imageData = new byte[length];

            // Copy bitmap to byte[]
            Marshal.Copy(bitmapData.Scan0, imageData, 0, length);
            bmp.UnlockBits(bitmapData);

            bitmapContent.SetPixelData(imageData);

            output.Faces[0].Add(bitmapContent);
            return output;
        }

    }
}
