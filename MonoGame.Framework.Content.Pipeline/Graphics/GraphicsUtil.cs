// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
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
            FreeImage.UnloadEx(ref fi);

            // Convert back to PixelBitmapContent<Vector4>
            src = new PixelBitmapContent<Vector4>(newWidth, newHeight);
            bytes = new byte[SurfaceFormat.Vector4.GetSize() * newWidth * newHeight];
            FreeImage.ConvertToRawBits(bytes, newfi, SurfaceFormat.Vector4.GetSize() * newWidth, 128, 0, 0, 0, true);
            src.SetPixelData(bytes);
            FreeImage.UnloadEx(ref newfi);
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

        public static void CompressPvrtc(TextureContent content, bool generateMipMaps)
        {
            // Calculate number of mip levels
            var width = content.Faces[0][0].Height;
            var height = content.Faces[0][0].Width;

			if (!IsPowerOfTwo(width) || !IsPowerOfTwo(height))
				throw new PipelineException("PVR compression requires width and height must be powers of two.");

			if (width != height)
				throw new PipelineException("PVR compression requires square textures.");

            var face = content.Faces[0][0];

            var alphaRange = CalculateAlphaRange(face);

            if (alphaRange == AlphaRange.Opaque)
                Compress(typeof(PvrtcRgb4BitmapContent), content, generateMipMaps);
            else
                Compress(typeof(PvrtcRgba4BitmapContent), content, generateMipMaps);
        }

        public static void CompressDxt(GraphicsProfile profile, TextureContent content, bool generateMipMaps, bool sharpAlpha)
        {
            var face = content.Faces[0][0];

            if (profile == GraphicsProfile.Reach)
            {
                if (!IsPowerOfTwo(face.Width) || !IsPowerOfTwo(face.Height))
                    throw new PipelineException("DXT compression requires width and height must be powers of two in Reach graphics profile.");                
            }

            // Test the alpha channel to figure out if we have alpha.
            var alphaRange = CalculateAlphaRange(face);

            if (alphaRange == AlphaRange.Opaque)
                Compress(typeof(Dxt1BitmapContent), content, generateMipMaps);
            else if (alphaRange == AlphaRange.Cutout || sharpAlpha)
                Compress(typeof(Dxt3BitmapContent), content, generateMipMaps);
            else
                Compress(typeof(Dxt5BitmapContent), content, generateMipMaps);
        }

        static public void CompressAti(TextureContent content, bool generateMipMaps)
        {
			var face = content.Faces[0][0];
			var alphaRange = CalculateAlphaRange(face);

            if (alphaRange == AlphaRange.Full)
                Compress(typeof(AtcExplicitBitmapContent), content, generateMipMaps);
            else
                Compress(typeof(AtcInterpolatedBitmapContent), content, generateMipMaps);
        }

        static public void CompressEtc1(TextureContent content, bool generateMipMaps)
        {
            var face = content.Faces[0][0];
            var alphaRange = CalculateAlphaRange(face);

            // Use BGRA4444 for textures with non-opaque alpha values
            if (alphaRange != AlphaRange.Opaque)
                Compress(typeof(PixelBitmapContent<Bgra4444>), content, generateMipMaps);
            else
            {
                // PVR SGX does not handle non-POT ETC1 textures.
                // https://code.google.com/p/libgdx/issues/detail?id=1310
                // Since we already enforce POT for PVR and DXT in Reach, we will also enforce POT for ETC1
                if (!IsPowerOfTwo(face.Width) || !IsPowerOfTwo(face.Height))
                    throw new PipelineException("ETC1 compression require width and height must be powers of two due to hardware restrictions on some devices.");
                Compress(typeof(Etc1BitmapContent), content, generateMipMaps);
            }
        }

        static public void CompressColor16Bit(TextureContent content, bool generateMipMaps)
        {
            var face = content.Faces[0][0];
            var alphaRange = CalculateAlphaRange(face);

            if (alphaRange == AlphaRange.Opaque)
                Compress(typeof(PixelBitmapContent<Bgr565>), content, generateMipMaps);
            else if (alphaRange == AlphaRange.Cutout)
                Compress(typeof(PixelBitmapContent<Bgra5551>), content, generateMipMaps);
            else
                Compress(typeof(PixelBitmapContent<Bgra4444>), content, generateMipMaps);
        }

        static void Compress(Type targetType, TextureContent content, bool generateMipMaps)
        {
            var wh = new object[2];
            if (generateMipMaps)
            {
                for (int i = 0; i < content.Faces.Count; ++i)
                {
                    // Only generate mipmaps if there are none already
                    if (content.Faces[i].Count == 1)
                    {
                        var src = content.Faces[i][0];
                        var w = src.Width;
                        var h = src.Height;

                        content.Faces[i].Clear();
                        wh[0] = w;
                        wh[1] = h;
                        var dest = (BitmapContent)Activator.CreateInstance(targetType, wh);
                        BitmapContent.Copy(src, dest);
                        content.Faces[i].Add(dest);
                        while (w > 1 && h > 1)
                        {
                            if (w > 1)
                                w = w >> 1;
                            if (h > 1)
                                h = h >> 1;
                            wh[0] = w;
                            wh[1] = h;
                            dest = (BitmapContent)Activator.CreateInstance(targetType, wh);
                            BitmapContent.Copy(src, dest);
                            content.Faces[i].Add(dest);
                        }
                    }
                    else
                    {
                        // Convert the existing mipmaps
                        var chain = content.Faces[i];
                        for (int j = 0; j < chain.Count; ++j)
                        {
                            var src = chain[j];
                            wh[0] = src.Width;
                            wh[1] = src.Height;
                            var dest = (BitmapContent)Activator.CreateInstance(targetType, wh);
                            BitmapContent.Copy(src, dest);
                            chain[j] = dest;
                        }
                    }
                }
            }
            else
            {
                // Converts all existing faces and mipmaps
                content.ConvertBitmapType(targetType);
            }
        }
    }
}
