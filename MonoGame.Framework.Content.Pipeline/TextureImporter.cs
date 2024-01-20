// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using FreeImageAPI;
using MonoGame.Framework.Utilities;
using StbImageSharp;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reading texture files for use in the Content Pipeline.
    /// </summary>
    [ContentImporter(".bmp", // Bitmap Image File
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
        public TextureImporter( )
        {
        }

        /// <summary>
        /// Called by the XNA Framework when importing a texture file to be used as a game asset. This is the method called by the XNA Framework when an asset is to be imported into an object that can be recognized by the Content Pipeline.
        /// </summary>
        /// <param name="filename">Name of a game asset file.</param>
        /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
        /// <returns>Resulting game asset.</returns>
        public override TextureContent Import(string filename, ContentImporterContext context)
        {
            var ext = Path.GetExtension(filename).ToLower();

            // Special case for loading some formats
            switch (ext)
            {
                case ".dds":
                    return DdsLoader.Import(filename, context);
                case ".bmp":
                    return LoadImage(filename);
            }

            var output = new Texture2DContent { Identity = new ContentIdentity(filename) };

            var format = FreeImage.GetFileType(filename, 0);
            var fBitmap = FreeImage.Load(format, filename, 0);
            //if freeimage can not recognize the image type
            if(format == FREE_IMAGE_FORMAT.FIF_UNKNOWN)
                throw new ContentLoadException("TextureImporter failed to load '" + filename + "'");
            //if freeimage can recognize the file headers but can't read its contents
            else if(fBitmap == IntPtr.Zero)
                throw new InvalidContentException("TextureImporter couldn't understand the contents of '" + filename + "'", output.Identity);
            BitmapContent face = null;
            var height = (int) FreeImage.GetHeight(fBitmap);
            var width = (int) FreeImage.GetWidth(fBitmap);
            //uint bpp = FreeImage.GetBPP(fBitmap);
            var imageType = FreeImage.GetImageType(fBitmap);

            // Swizzle channels and expand to include an alpha channel
            fBitmap = ConvertAndSwapChannels(fBitmap, imageType);

            // The bits per pixel and image type may have changed
            uint bpp = FreeImage.GetBPP(fBitmap);
            imageType = FreeImage.GetImageType(fBitmap);
            var pitch = (int) FreeImage.GetPitch(fBitmap);
            var redMask = FreeImage.GetRedMask(fBitmap);
            var greenMask = FreeImage.GetGreenMask(fBitmap);
            var blueMask = FreeImage.GetBlueMask(fBitmap);

            // Create the byte array for the data
            byte[] bytes = new byte[((width * height * bpp - 1) / 8) + 1];

            //Converts the pixel data to bytes, do not try to use this call to switch the color channels because that only works for 16bpp bitmaps
            FreeImage.ConvertToRawBits(bytes, fBitmap, pitch, bpp, redMask, greenMask, blueMask, true);
            // Create the Pixel bitmap content depending on the image type
            switch(imageType)
            {
                //case FREE_IMAGE_TYPE.FIT_BITMAP:
                default:
                    face = new PixelBitmapContent<Color>(width, height);
                    break;
                case FREE_IMAGE_TYPE.FIT_RGBA16:
                    face = new PixelBitmapContent<Rgba64>(width, height);
                    break;
                case FREE_IMAGE_TYPE.FIT_RGBAF:
                    face = new PixelBitmapContent<Vector4>(width, height);
                    break;
            }
            FreeImage.Unload(fBitmap);

            face.SetPixelData(bytes);
            output.Faces[0].Add(face);
            return output;
        }
        /// <summary>
        /// Expands images to have an alpha channel and swaps red and blue channels
        /// </summary>
        /// <param name="fBitmap">Image to process</param>
        /// <param name="imageType">Type of the image for the procedure</param>
        /// <returns></returns>
        private static IntPtr ConvertAndSwapChannels(IntPtr fBitmap, FREE_IMAGE_TYPE imageType)
        {
            IntPtr bgra;
            switch(imageType)
            {
                // Return BGRA images as is

                case FREE_IMAGE_TYPE.FIT_RGBAF:
                case FREE_IMAGE_TYPE.FIT_RGBA16:
                    break;

                // Add an alpha channel to BGRA images without one

                case FREE_IMAGE_TYPE.FIT_RGBF:
                    bgra = FreeImage.ConvertToType(fBitmap, FREE_IMAGE_TYPE.FIT_RGBAF, true);
                    FreeImage.Unload(fBitmap);
                    fBitmap = bgra;
                    break;

                case FREE_IMAGE_TYPE.FIT_RGB16:
                    bgra = FreeImage.ConvertToType(fBitmap, FREE_IMAGE_TYPE.FIT_RGBA16, true);
                    FreeImage.Unload(fBitmap);
                    fBitmap = bgra;
                    break;


                // Add an alpha channel to RGB images
                // Swap the red and blue channels of RGBA images

                default:
                    // Bitmap and other formats are converted to 32-bit by default
                    bgra = FreeImage.ConvertTo32Bits(fBitmap);
                    SwitchRedAndBlueChannels(bgra);
                    FreeImage.Unload(fBitmap);
                    fBitmap = bgra;
                    break;
            }

            return fBitmap;
        }
        /// <summary>
        /// Switches the red and blue channels
        /// </summary>
        /// <param name="fBitmap">image</param>
        private static void SwitchRedAndBlueChannels(IntPtr fBitmap)
        {
            var r = FreeImage.GetChannel(fBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
            var b = FreeImage.GetChannel(fBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
            FreeImage.SetChannel(fBitmap, b, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
            FreeImage.SetChannel(fBitmap, r, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
            FreeImage.Unload(r);
            FreeImage.Unload(b);
        }

        // Loads BMP using StbSharp. This allows us to load BMP files containing BITMAPV4HEADER and BITMAPV5HEADER
        // structures, which FreeImage does not support.
        TextureContent LoadImage(string filename)
        {
            var output = new Texture2DContent { Identity = new ContentIdentity(filename) };

            ImageResult result;
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            }

            var face = new PixelBitmapContent<Color>(result.Width, result.Height);
            face.SetPixelData(result.Data);
            output.Faces[0].Add(face);

            return output;
        }
    }
}
