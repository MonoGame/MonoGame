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

            FREE_IMAGE_FORMAT format = FREE_IMAGE_FORMAT.FIF_UNKNOWN;
            var fBitmap = FreeImage.LoadEx(filename, FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
            if (format == FREE_IMAGE_FORMAT.FIF_UNKNOWN)
                throw new ContentLoadException("TextureImporter failed to load '" + filename + "'");

            BitmapContent face = null;
            var height = (int)FreeImage.GetHeight(fBitmap);
            var width = (int)FreeImage.GetWidth(fBitmap);
            var bpp = FreeImage.GetBPP(fBitmap);
            var imageType = FreeImage.GetImageType(fBitmap);

            // Swizzle channels and expand to include an alpha channel
            switch (imageType)
            {
                case FREE_IMAGE_TYPE.FIT_BITMAP:
                    switch (bpp)
                    {
                        case 8:
                            {
                                // Expand to 32-bit
                                var bgra = FreeImage.ConvertTo32Bits(fBitmap);
                                FreeImage.UnloadEx(ref fBitmap);
                                fBitmap = bgra;
                                // Swap R and B channels to make it BGRA
                                var r = FreeImage.GetChannel(fBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
                                var b = FreeImage.GetChannel(fBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                                FreeImage.SetChannel(fBitmap, b, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
                                FreeImage.SetChannel(fBitmap, r, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                                FreeImage.UnloadEx(ref r);
                                FreeImage.UnloadEx(ref b);
                            }
                            break;

                        case 16:
                            // Channel swizzling for 16-bit formats is done after we get the bytes from the bitmap
                            break;

                        case 24:
                            {
                                // Swap R and B channels to make it BGR, then add an alpha channel
                                var r = FreeImage.GetChannel(fBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
                                var b = FreeImage.GetChannel(fBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                                FreeImage.SetChannel(fBitmap, b, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
                                FreeImage.SetChannel(fBitmap, r, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                                FreeImage.UnloadEx(ref r);
                                FreeImage.UnloadEx(ref b);
                                var bgra = FreeImage.ConvertTo32Bits(fBitmap);
                                FreeImage.UnloadEx(ref fBitmap);
                                fBitmap = bgra;
                            }
                            break;

                        case 32:
                            {
                                // Swap R and B channels to make it BGRA
                                var r = FreeImage.GetChannel(fBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
                                var b = FreeImage.GetChannel(fBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                                FreeImage.SetChannel(fBitmap, b, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
                                FreeImage.SetChannel(fBitmap, r, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                                FreeImage.UnloadEx(ref r);
                                FreeImage.UnloadEx(ref b);
                            }
                            break;
                    }
                    break;

                case FREE_IMAGE_TYPE.FIT_RGBF:
                    {
                        // Swap R and B channels to make it BGR, then add an alpha channel
                        var r = FreeImage.GetChannel(fBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
                        var b = FreeImage.GetChannel(fBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                        FreeImage.SetChannel(fBitmap, b, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
                        FreeImage.SetChannel(fBitmap, r, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                        FreeImage.UnloadEx(ref r);
                        FreeImage.UnloadEx(ref b);
                        var bgra = FreeImage.ConvertToType(fBitmap, FREE_IMAGE_TYPE.FIT_RGBAF, true);
                        FreeImage.UnloadEx(ref fBitmap);
                        fBitmap = bgra;
                    }
                    break;

                case FREE_IMAGE_TYPE.FIT_RGBAF:
                    {
                        // Swap R and B channels to make it BGRA
                        var r = FreeImage.GetChannel(fBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
                        var b = FreeImage.GetChannel(fBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                        FreeImage.SetChannel(fBitmap, b, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
                        FreeImage.SetChannel(fBitmap, r, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                        FreeImage.UnloadEx(ref r);
                        FreeImage.UnloadEx(ref b);
                    }
                    break;
            }

            // The bits per pixel and image type may have changed
            bpp = FreeImage.GetBPP(fBitmap);
            imageType = FreeImage.GetImageType(fBitmap);
            var pitch = (int)FreeImage.GetPitch(fBitmap);
            var redMask = FreeImage.GetRedMask(fBitmap);
            var greenMask = FreeImage.GetGreenMask(fBitmap);
            var blueMask = FreeImage.GetBlueMask(fBitmap);

            // Get the bytes from the FreeImage bitmap
            var bytes = new byte[width * height * (bpp / 8)];
            FreeImage.ConvertToRawBits(bytes, fBitmap, pitch, bpp, redMask, greenMask, blueMask, true);

            // Massage into the ordering and formats we want that wasn't possible earlier
            switch (imageType)
            {
                case FREE_IMAGE_TYPE.FIT_BITMAP:
                    switch (bpp)
                    {
                        case 16:
                            // Swap R and B channels to make it BGR(A)
                            if (FreeImage.IsRGB555(fBitmap))
                            {
                                var alphaMask = (uint)1;
                                for (var i = 0; i < bytes.Length; i += 2)
                                {
                                    ushort rgb = (ushort)(bytes[i] | (bytes[i] << 8));
                                    ushort bgr = (ushort)(((rgb & blueMask) << 10) | (rgb & greenMask) | ((rgb & redMask) >> 10) | (rgb & alphaMask));
                                    bytes[i] = (byte)bgr;
                                    bytes[i + 1] = (byte)(bgr >> 8);
                                }

                                face = new PixelBitmapContent<Bgra5551>(width, height);
                            }
                            else if (FreeImage.IsRGB565(fBitmap))
                            {
                                for (var i = 0; i < bytes.Length; i += 2)
                                {
                                    ushort rgb = (ushort)(bytes[i] | (bytes[i] << 8));
                                    ushort bgr = (ushort)(((rgb & blueMask) << 11) | (rgb & greenMask) | ((rgb & redMask) >> 11));
                                    bytes[i] = (byte)bgr;
                                    bytes[i + 1] = (byte)(bgr >> 8);
                                }

                                face = new PixelBitmapContent<Bgr565>(width, height);
                            }
                            else
                            {
                                var alphaMask = (uint)0x000F;
                                for (var i = 0; i < bytes.Length; i += 2)
                                {
                                    ushort rgba = (ushort)(bytes[i] | (bytes[i] << 8));
                                    ushort bgra = (ushort)(((rgba & blueMask) << 8) | (rgba & greenMask) | ((rgba & redMask) >> 8) | (rgba & alphaMask));
                                    bytes[i] = (byte)bgra;
                                    bytes[i + 1] = (byte)(bgra >> 8);
                                }

                                face = new PixelBitmapContent<Bgra4444>(width, height);
                            }
                            break;

                        case 32:
                            face = new PixelBitmapContent<Color>(width, height);
                            break;
                    }
                    break;

                case FREE_IMAGE_TYPE.FIT_RGBAF:
                    face = new PixelBitmapContent<Vector4>(width, height);
                    break;
            }
            FreeImage.UnloadEx(ref fBitmap);

            face.SetPixelData(bytes);
            output.Faces[0].Add(face);
            return output;
        }
    }
}
