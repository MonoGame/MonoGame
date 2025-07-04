// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoGame.Framework.Content.Pipeline.Interop;

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
        public TextureImporter()
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
            }

            var output = new Texture2DContent { Identity = new ContentIdentity(filename) };

            MGCP_Bitmap bitmap = default;
            IntPtr err = MGCP.MP_ImportBitmap(filename, ref bitmap);
            if (err != IntPtr.Zero)
            {
                string errorMsg = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(err);
                throw new InvalidContentException($"TextureImporter failed to load '{filename}': {errorMsg}");
            }
            if (bitmap.data == IntPtr.Zero)
                throw new InvalidContentException($"TextureImporter failed to load '{filename}': native returned null data");

            int width = bitmap.width;
            int height = bitmap.height;
            int pixelCount = width * height;

            switch (bitmap.type)
            {
                case TextureType.Rgba8:
                    AddFace<Color>(output, bitmap.data, width, height, pixelCount, 4);
                    break;
                case TextureType.Rgba16:
                    AddFace<Rgba64>(output, bitmap.data, width, height, pixelCount, 8);
                    break;
                case TextureType.RgbaF:
                    AddFace<Vector4>(output, bitmap.data, width, height, pixelCount, 16);
                    break;
                default:
                    throw new InvalidContentException("TextureImporter does not support the specified texture format.");
            }

            MGCP.MP_FreeBitmap(ref bitmap);
            return output;
        }

        /// <summary>
        /// Adds a face to the texture content with the specified pixel format.
        /// </summary>
        private unsafe void AddFace<T>(Texture2DContent output, IntPtr data, int width, int height, int pixelCount, int bytesPerPixel)
            where T : struct, IEquatable<T>
        {
            var face = new PixelBitmapContent<T>(width, height);
            var bytes = new byte[pixelCount * bytesPerPixel];
            System.Runtime.InteropServices.Marshal.Copy(data, bytes, 0, bytes.Length);
            face.SetPixelData(bytes);
            output.Faces[0].Add(face);
        }
    }
}
