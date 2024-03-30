// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using FreeImageAPI;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public static class GraphicsUtil
    {
        internal static BitmapContent Resize(this BitmapContent bitmap, int newWidth, int newHeight)
        {
            BitmapContent src = bitmap;
            SurfaceFormat format;
            src.TryGetFormat(out format);
            if (format != SurfaceFormat.Vector4)
            {
                var v4 = new PixelBitmapContent<Vector4>(src.Width, src.Height);
                BitmapContent.Copy(src, v4);
                src = v4;
            }

            // Convert to FreeImage bitmap
            var bytes = src.GetPixelData();
            var fi = FreeImage.ConvertFromRawBits(bytes, FREE_IMAGE_TYPE.FIT_RGBAF, src.Width, src.Height, SurfaceFormat.Vector4.GetSize() * src.Width, 128, 0, 0, 0, true);

            // Resize
            var newfi = FreeImage.Rescale(fi, newWidth, newHeight, FREE_IMAGE_FILTER.FILTER_BICUBIC);
            FreeImage.Unload(fi);

            // Convert back to PixelBitmapContent<Vector4>
            src = new PixelBitmapContent<Vector4>(newWidth, newHeight);
            bytes = new byte[SurfaceFormat.Vector4.GetSize() * newWidth * newHeight];
            FreeImage.ConvertToRawBits(bytes, newfi, SurfaceFormat.Vector4.GetSize() * newWidth, 128, 0, 0, 0, true);
            src.SetPixelData(bytes);
            FreeImage.Unload(newfi);
            // Convert back to source type if required
            if (format != SurfaceFormat.Vector4)
            {
                var s = (BitmapContent)Activator.CreateInstance(bitmap.GetType(), new object[] { newWidth, newHeight });
                BitmapContent.Copy(src, s);
                src = s;
            }

            return src;
        }

        public static void BGRAtoRGBA(byte[] data)
        {
            for (var x = 0; x < data.Length; x += 4)
            {
                data[x] ^= data[x + 2];
                data[x + 2] ^= data[x];
                data[x] ^= data[x + 2];
            }
        }

        public static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        /// <summary>
        /// Returns the next power of two. Returns same value if already is PoT.
        /// </summary>
        public static int GetNextPowerOfTwo(int value)
        {
            if (IsPowerOfTwo(value))
                return value;

            var nearestPower = 1;
            while (nearestPower < value)
                nearestPower = nearestPower << 1;

            return nearestPower;
        }

        enum AlphaRange
        {
            /// <summary>
            /// Pixel data has no alpha values below 1.0.
            /// </summary>
            Opaque,

            /// <summary>
            /// Pixel data contains alpha values that are either 0.0 or 1.0.
            /// </summary>
            Cutout,

            /// <summary>
            /// Pixel data contains alpha values that cover the full range of 0.0 to 1.0.
            /// </summary>
            Full,
        }

        /// <summary>
        /// Gets the alpha range in a set of pixels.
        /// </summary>
        /// <param name="bitmap">A bitmap of full-colour floating point pixel data in RGBA or BGRA order.</param>
        /// <returns>A member of the AlphaRange enum to describe the range of alpha in the pixel data.</returns>
		static AlphaRange CalculateAlphaRange(BitmapContent bitmap)
        {
			AlphaRange result = AlphaRange.Opaque;
			var pixelBitmap = bitmap as PixelBitmapContent<Vector4>;
			if (pixelBitmap != null)
			{
				for (int y = 0; y < pixelBitmap.Height; ++y)
                {
                    var row = pixelBitmap.GetRow(y);
                    foreach (var pixel in row)
                    {
                        if (pixel.W == 0.0)
                            result = AlphaRange.Cutout;
                        else if (pixel.W < 1.0)
                            return AlphaRange.Full;
                    }
				}
			}
            return result;
        }

        public static void CompressPvrtc(ContentProcessorContext context, TextureContent content, bool isSpriteFont)
        {
            // If sharp alpha is required (for a font texture page), use 16-bit color instead of PVR
            if (isSpriteFont)
            {
                CompressColor16Bit(context, content);
                return;
            }

            // Calculate number of mip levels
            var width = content.Faces[0][0].Height;
            var height = content.Faces[0][0].Width;

			if (!IsPowerOfTwo(width) || !IsPowerOfTwo(height) || (width != height))
            {
                context.Logger.LogWarning(null, content.Identity, "PVR compression requires width and height to be powers of two and equal. Falling back to 16-bit color.");
                CompressColor16Bit(context, content);
                return;
            }

            var face = content.Faces[0][0];

            var alphaRange = CalculateAlphaRange(face);

            if (alphaRange == AlphaRange.Opaque)
                content.ConvertBitmapType(typeof(PvrtcRgb4BitmapContent));
            else
                content.ConvertBitmapType(typeof(PvrtcRgba4BitmapContent));
        }

        public static void CompressDxt(ContentProcessorContext context, TextureContent content, bool isSpriteFont)
        {
            var face = content.Faces[0][0];

            if (context.TargetProfile == GraphicsProfile.Reach)
            {
                if (!IsPowerOfTwo(face.Width) || !IsPowerOfTwo(face.Height))
                    throw new PipelineException("DXT compression requires width and height must be powers of two in Reach graphics profile.");                
            }

            // Test the alpha channel to figure out if we have alpha.
            var alphaRange = CalculateAlphaRange(face);

            // TODO: This isn't quite right.
            //
            // We should be generating DXT1 textures for cutout alpha
            // as DXT1 supports 1bit alpha and it uses less memory.
            //
            // XNA never generated DXT3 for textures... it always picked
            // between DXT1 for cutouts and DXT5 for fractional alpha.
            //
            // DXT3 however can produce better results for high frequency
            // alpha like a chain link fence where is DXT5 is better for 
            // low frequency alpha like clouds.  I don't know how we can 
            // pick the right thing in this case without a hint.
            //
            if (isSpriteFont)
                CompressFontDXT3(content);
            else if (alphaRange == AlphaRange.Opaque)
                content.ConvertBitmapType(typeof(Dxt1BitmapContent));
            else if (alphaRange == AlphaRange.Cutout)
                content.ConvertBitmapType(typeof(Dxt3BitmapContent));
            else
                content.ConvertBitmapType(typeof(Dxt5BitmapContent));
        }

        static public void CompressAti(ContentProcessorContext context, TextureContent content, bool isSpriteFont)
        {
            // If sharp alpha is required (for a font texture page), use 16-bit color instead of PVR
            if (isSpriteFont)
            {
                CompressColor16Bit(context, content);
                return;
            }

            var face = content.Faces[0][0];
			var alphaRange = CalculateAlphaRange(face);

            if (alphaRange == AlphaRange.Full)
                content.ConvertBitmapType(typeof(AtcExplicitBitmapContent));
            else
                content.ConvertBitmapType(typeof(AtcInterpolatedBitmapContent));
        }

        static public void CompressEtc1(ContentProcessorContext context, TextureContent content, bool isSpriteFont)
        {
            // If sharp alpha is required (for a font texture page), use 16-bit color instead of PVR
            if (isSpriteFont)
            {
                CompressColor16Bit(context, content);
                return;
            }

            var face = content.Faces[0][0];
            var alphaRange = CalculateAlphaRange(face);

            // Use BGRA4444 for textures with non-opaque alpha values
            if (alphaRange != AlphaRange.Opaque)
                content.ConvertBitmapType(typeof(PixelBitmapContent<Bgra4444>));
            else
            {
                // PVR SGX does not handle non-POT ETC1 textures.
                // https://code.google.com/p/libgdx/issues/detail?id=1310
                // Since we already enforce POT for PVR and DXT in Reach, we will also enforce POT for ETC1
                if (!IsPowerOfTwo(face.Width) || !IsPowerOfTwo(face.Height))
                {
                    context.Logger.LogWarning(null, content.Identity, "ETC1 compression requires width and height to be powers of two due to hardware restrictions on some devices. Falling back to BGR565.");
                    content.ConvertBitmapType(typeof(PixelBitmapContent<Bgr565>));
                }
                else
                    content.ConvertBitmapType(typeof(Etc1BitmapContent));
            }
        }

        static public void CompressColor16Bit(ContentProcessorContext context, TextureContent content)
        {
            var face = content.Faces[0][0];
            var alphaRange = CalculateAlphaRange(face);

            if (alphaRange == AlphaRange.Opaque)
                content.ConvertBitmapType(typeof(PixelBitmapContent<Bgr565>));
            else if (alphaRange == AlphaRange.Cutout)
                content.ConvertBitmapType(typeof(PixelBitmapContent<Bgra5551>));
            else
                content.ConvertBitmapType(typeof(PixelBitmapContent<Bgra4444>));
        }

        // Compress the greyscale font texture page using a specially-formulated DXT3 mode
        static public unsafe void CompressFontDXT3(TextureContent content)
        {
            if (content.Faces.Count > 1)
                throw new PipelineException("Font textures should only have one face");

            var block = new Vector4[16];
            for (int i = 0; i < content.Faces[0].Count; ++i)
            {
                var face = content.Faces[0][i];
                var xBlocks = (face.Width + 3) / 4;
                var yBlocks = (face.Height + 3) / 4;
                var dxt3Size = xBlocks * yBlocks * 16;
                var buffer = new byte[dxt3Size];

                var bytes = face.GetPixelData();
                fixed (byte* b = bytes)
                {
                    Vector4* colors = (Vector4*)b;

                    int w = 0;
                    int h = 0;
                    int x = 0;
                    int y = 0;
                    while (h < (face.Height & ~3))
                    {
                        w = 0;
                        x = 0;

                        var h0 = h * face.Width;
                        var h1 = h0 + face.Width;
                        var h2 = h1 + face.Width;
                        var h3 = h2 + face.Width;

                        while (w < (face.Width & ~3))
                        {
                            block[0] = colors[w + h0];
                            block[1] = colors[w + h0 + 1];
                            block[2] = colors[w + h0 + 2];
                            block[3] = colors[w + h0 + 3];
                            block[4] = colors[w + h1];
                            block[5] = colors[w + h1 + 1];
                            block[6] = colors[w + h1 + 2];
                            block[7] = colors[w + h1 + 3];
                            block[8] = colors[w + h2];
                            block[9] = colors[w + h2 + 1];
                            block[10] = colors[w + h2 + 2];
                            block[11] = colors[w + h2 + 3];
                            block[12] = colors[w + h3];
                            block[13] = colors[w + h3 + 1];
                            block[14] = colors[w + h3 + 2];
                            block[15] = colors[w + h3 + 3];

                            int offset = (x + y * xBlocks) * 16;
                            CompressFontDXT3Block(block, buffer, offset);

                            w += 4;
                            ++x;
                        }

                        // Do partial block at end of row
                        if (w < face.Width)
                        {
                            var cols = face.Width - w;
                            Array.Clear(block, 0, 16);
                            for (int r = 0; r < 4; ++r)
                            {
                                h0 = (h + r) * face.Width;
                                for (int c = 0; c < cols; ++c)
                                    block[(r * 4) + c] = colors[w + h0 + c];
                            }

                            int offset = (x + y * xBlocks) * 16;
                            CompressFontDXT3Block(block, buffer, offset);
                        }

                        h += 4;
                        ++y;
                    }

                    // Do last partial row
                    if (h < face.Height)
                    {
                        var rows = face.Height - h;
                        w = 0;
                        x = 0;
                        while (w < (face.Width & ~3))
                        {
                            Array.Clear(block, 0, 16);
                            for (int r = 0; r < rows; ++r)
                            {
                                var h0 = (h + r) * face.Width;
                                block[(r * 4) + 0] = colors[w + h0 + 0];
                                block[(r * 4) + 1] = colors[w + h0 + 1];
                                block[(r * 4) + 2] = colors[w + h0 + 2];
                                block[(r * 4) + 3] = colors[w + h0 + 3];
                            }

                            int offset = (x + y * xBlocks) * 16;
                            CompressFontDXT3Block(block, buffer, offset);

                            w += 4;
                            ++x;
                        }

                        // Do last partial block
                        if (w < face.Width)
                        {
                            var cols = face.Width - w;
                            Array.Clear(block, 0, 16);
                            for (int r = 0; r < rows; ++r)
                            {
                                var h0 = (h + r) * face.Width;
                                for (int c = 0; c < cols; ++c)
                                    block[(r * 4) + c] = colors[w + h0 + c];
                            }

                            int offset = (x + y * xBlocks) * 16;
                            CompressFontDXT3Block(block, buffer, offset);
                        }
                    }
                }

                var dxt3 = new Dxt3BitmapContent(face.Width, face.Height);
                dxt3.SetPixelData(buffer);
                content.Faces[0][i] = dxt3;
            }
        }

        // Maps a 2-bit greyscale to the index required for DXT3
        // 00 = color0
        // 01 = color1
        // 10 = 2/3 * color0 + 1/3 * color1
        // 11 = 1/3 * color0 + 2/3 * color1
        static byte[] dxt3Map = new byte[] { 0, 2, 3, 1 };

        // Compress a single 4x4 block from colors into buffer at the given offset
        static void CompressFontDXT3Block(Vector4[] colors, byte[] buffer, int offset)
        {
            // Get the alpha into a 0-15 range
            int a0 = (int)(colors[0].W * 15.0);
            int a1 = (int)(colors[1].W * 15.0);
            int a2 = (int)(colors[2].W * 15.0);
            int a3 = (int)(colors[3].W * 15.0);
            int a4 = (int)(colors[4].W * 15.0);
            int a5 = (int)(colors[5].W * 15.0);
            int a6 = (int)(colors[6].W * 15.0);
            int a7 = (int)(colors[7].W * 15.0);
            int a8 = (int)(colors[8].W * 15.0);
            int a9 = (int)(colors[9].W * 15.0);
            int a10 = (int)(colors[10].W * 15.0);
            int a11 = (int)(colors[11].W * 15.0);
            int a12 = (int)(colors[12].W * 15.0);
            int a13 = (int)(colors[13].W * 15.0);
            int a14 = (int)(colors[14].W * 15.0);
            int a15 = (int)(colors[15].W * 15.0);

            // Duplicate the top two bits into the bottom two bits so we get one of four values: b0000, b0101, b1010, b1111
            a0 = (a0 & 0xC) | (a0 >> 2);
            a1 = (a1 & 0xC) | (a1 >> 2);
            a2 = (a2 & 0xC) | (a2 >> 2);
            a3 = (a3 & 0xC) | (a3 >> 2);
            a4 = (a4 & 0xC) | (a4 >> 2);
            a5 = (a5 & 0xC) | (a5 >> 2);
            a6 = (a6 & 0xC) | (a6 >> 2);
            a7 = (a7 & 0xC) | (a7 >> 2);
            a8 = (a8 & 0xC) | (a8 >> 2);
            a9 = (a9 & 0xC) | (a9 >> 2);
            a10 = (a10 & 0xC) | (a10 >> 2);
            a11 = (a11 & 0xC) | (a11 >> 2);
            a12 = (a12 & 0xC) | (a12 >> 2);
            a13 = (a13 & 0xC) | (a13 >> 2);
            a14 = (a14 & 0xC) | (a14 >> 2);
            a15 = (a15 & 0xC) | (a15 >> 2);

            // 4-bit alpha
            buffer[offset + 0] = (byte)((a1 << 4) | a0);
            buffer[offset + 1] = (byte)((a3 << 4) | a2);
            buffer[offset + 2] = (byte)((a5 << 4) | a4);
            buffer[offset + 3] = (byte)((a7 << 4) | a6);
            buffer[offset + 4] = (byte)((a9 << 4) | a8);
            buffer[offset + 5] = (byte)((a11 << 4) | a10);
            buffer[offset + 6] = (byte)((a13 << 4) | a12);
            buffer[offset + 7] = (byte)((a15 << 4) | a14);

            // color0 (transparent)
            buffer[offset + 8] = 0;
            buffer[offset + 9] = 0;

            // color1 (white)
            buffer[offset + 10] = 255;
            buffer[offset + 11] = 255;

            // Get the red (to be used for green and blue channels as well) into a 0-15 range
            a0 = (int)(colors[0].X * 15.0);
            a1 = (int)(colors[1].X * 15.0);
            a2 = (int)(colors[2].X * 15.0);
            a3 = (int)(colors[3].X * 15.0);
            a4 = (int)(colors[4].X * 15.0);
            a5 = (int)(colors[5].X * 15.0);
            a6 = (int)(colors[6].X * 15.0);
            a7 = (int)(colors[7].X * 15.0);
            a8 = (int)(colors[8].X * 15.0);
            a9 = (int)(colors[9].X * 15.0);
            a10 = (int)(colors[10].X * 15.0);
            a11 = (int)(colors[11].X * 15.0);
            a12 = (int)(colors[12].X * 15.0);
            a13 = (int)(colors[13].X * 15.0);
            a14 = (int)(colors[14].X * 15.0);
            a15 = (int)(colors[15].X * 15.0);

            // Duplicate the top two bits into the bottom two bits so we get one of four values: b0000, b0101, b1010, b1111
            a0 = (a0 & 0xC) | (a0 >> 2);
            a1 = (a1 & 0xC) | (a1 >> 2);
            a2 = (a2 & 0xC) | (a2 >> 2);
            a3 = (a3 & 0xC) | (a3 >> 2);
            a4 = (a4 & 0xC) | (a4 >> 2);
            a5 = (a5 & 0xC) | (a5 >> 2);
            a6 = (a6 & 0xC) | (a6 >> 2);
            a7 = (a7 & 0xC) | (a7 >> 2);
            a8 = (a8 & 0xC) | (a8 >> 2);
            a9 = (a9 & 0xC) | (a9 >> 2);
            a10 = (a10 & 0xC) | (a10 >> 2);
            a11 = (a11 & 0xC) | (a11 >> 2);
            a12 = (a12 & 0xC) | (a12 >> 2);
            a13 = (a13 & 0xC) | (a13 >> 2);
            a14 = (a14 & 0xC) | (a14 >> 2);
            a15 = (a15 & 0xC) | (a15 >> 2);

            // Color indices (00 = color0, 01 = color1, 10 = 2/3 * color0 + 1/3 * color1, 11 = 1/3 * color0 + 2/3 * color1)
            buffer[offset + 12] = (byte)((dxt3Map[a3 >> 2] << 6) | (dxt3Map[a2 >> 2] << 4) | (dxt3Map[a1 >> 2] << 2) | dxt3Map[a0 >> 2]);
            buffer[offset + 13] = (byte)((dxt3Map[a7 >> 2] << 6) | (dxt3Map[a6 >> 2] << 4) | (dxt3Map[a5 >> 2] << 2) | dxt3Map[a4 >> 2]);
            buffer[offset + 14] = (byte)((dxt3Map[a11 >> 2] << 6) | (dxt3Map[a10 >> 2] << 4) | (dxt3Map[a9 >> 2] << 2) | dxt3Map[a8 >> 2]);
            buffer[offset + 15] = (byte)((dxt3Map[a15 >> 2] << 6) | (dxt3Map[a14 >> 2] << 4) | (dxt3Map[a13 >> 2] << 2) | dxt3Map[a12 >> 2]);
        }
    }
}
