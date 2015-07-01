// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using FreeImageAPI;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reading texture files for use in the Content Pipeline.
    /// </summary>
    [ContentImporter(   ".bmp", // Bitmap Image File
                        ".cut", // Dr Halo CUT
                        ".dds", // Direct Draw Surface
                        ".g3", // Raw Fax G3
                        ".hdr", // RGBE
                        ".gif", // Graphcis Interchange Format
                        ".ico", // Microsoft Windows Icon
                        ".iff", // Interchange File Format
                        ".jbg", ".jbig", // JBIG
                        ".jng", ".jpg", ".jpeg", ".jpe", ".jif", ".jfif", ".jfi", // JPEG
                        ".jp2", ".j2k", ".jpf", ".jpx", ".jpm", ".mj2", // JPEG 2000
                        ".jxr", ".hdp", ".wdp", // JPEG XR
                        ".koa", ".gg", // Koala
                        ".pcd", // Kodak PhotoCD
                        ".mng", // Multiple-Image Network Graphics
                        ".pcx", //Personal Computer Exchange
                        ".pbm", ".pgm", ".ppm", ".pnm", // Netpbm
                        ".pfm", // Printer Font Metrics
                        ".png", //Portable Network Graphics
                        ".pict", ".pct", ".pic", // PICT
                        ".psd", // Photoshop
                        ".3fr", ".ari", ".arw", ".bay", ".crw", ".cr2", ".cap", ".dcs", // RAW
                        ".dcr", ".dng", ".drf", ".eip", ".erf", ".fff", ".iiq", ".k25", // RAW
                        ".kdc", ".mdc", ".mef", ".mos", ".mrw", ".nef", ".nrw", ".obm", // RAW
                        ".orf", ".pef", ".ptx", ".pxn", ".r3d", ".raf", ".raw", ".rwl", // RAW
                        ".rw2", ".rwz", ".sr2", ".srf", ".srw", ".x3f", // RAW
                        ".ras", ".sun", // Sun RAS
                        ".sgi", ".rgba", ".bw", ".int", ".inta", // Silicon Graphics Image
                        ".tga", // Truevision TGA/TARGA
                        ".tiff", ".tif", // Tagged Image File Format
                        ".wbmp", // Wireless Application Protocol Bitmap Format
                        ".webp", // WebP
                        ".xbm", // X BitMap
                        ".xpm", // X PixMap
                    DisplayName = "Texture Importer - MonoGame", DefaultProcessor = "TextureProcessor")]
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
            var output = new Texture2DContent { Identity = new ContentIdentity(filename) };

            var fBitmap = FreeImage.LoadEx(filename);

            BitmapContent face = null;
            var height = (int)FreeImage.GetHeight(fBitmap);
            var width = (int)FreeImage.GetWidth(fBitmap);
            var bpp = FreeImage.GetBPP(fBitmap);
            if (FreeImage.GetImageType(fBitmap) == FREE_IMAGE_TYPE.FIT_BITMAP)
            {
                switch (bpp)
                {
                    case 16:
                        if (FreeImage.IsRGB555(fBitmap))
                            face = new PixelBitmapContent<Bgra5551>(width, height);
                        else if (FreeImage.IsRGB565(fBitmap))
                            face = new PixelBitmapContent<Bgr565>(width, height);
                        else
                            face = new PixelBitmapContent<Bgra4444>(width, height);
                        break;

                    case 24:
                        {
                            // MonoGame has no SurfaceFormat for BGR888, so convert to BGRA8888 through FreeImage first
                            var copy = FreeImage.ConvertTo32Bits(fBitmap);
                            FreeImage.UnloadEx(ref fBitmap);
                            fBitmap = copy;
                            face = new PixelBitmapContent<Color>(width, height);
                        }
                        break;

                    case 32:
                        face = new PixelBitmapContent<Color>(width, height);
                        break;

                    case 64:
                        face = new PixelBitmapContent<HalfVector4>(width, height);
                        break;

                    case 128:
                        face = new PixelBitmapContent<Vector4>(width, height);
                        break;
                }
            }

            var pitch = (int)FreeImage.GetPitch(fBitmap);
            var redMask = FreeImage.GetRedMask(fBitmap);
            var greenMask = FreeImage.GetGreenMask(fBitmap);
            var blueMask = FreeImage.GetBlueMask(fBitmap);

            SurfaceFormat format;
            face.TryGetFormat(out format);
            var bitmapSize = format.GetSize() * width * height;
            var bytes = new byte[bitmapSize];
            FreeImage.ConvertToRawBits(bytes, fBitmap, pitch, bpp, redMask, greenMask, blueMask, true);
            FreeImage.UnloadEx(ref fBitmap);

            face.SetPixelData(bytes);

            output.Faces[0].Add(face);

            return output;
        }
    }
}
