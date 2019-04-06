// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Utilities;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace MonoGame.Utilities.Png
{
    public class PngWriter
    {
        private const int bitsPerSample = 8;
        private ColorType colorType;
        private Color[] colorData;
        private int width;
        private int height;

        public PngWriter()
        {
            colorType = ColorType.RgbWithAlpha;
        }

        public void Write(Texture2D texture2D, Stream outputStream)
        {
            width = texture2D.Width;
            height = texture2D.Height;

            GetColorData(texture2D);

            // write PNG signature
            outputStream.Write(HeaderChunk.PngSignature, 0, HeaderChunk.PngSignature.Length);

            // write header chunk
            var headerChunk = new HeaderChunk();
            headerChunk.Width = (uint)texture2D.Width;
            headerChunk.Height = (uint)texture2D.Height;
            headerChunk.BitDepth = 8;
            headerChunk.ColorType = colorType;
            headerChunk.CompressionMethod = 0;
            headerChunk.FilterMethod = 0;
            headerChunk.InterlaceMethod = 0;

            var headerChunkBytes = headerChunk.Encode();
            outputStream.Write(headerChunkBytes, 0, headerChunkBytes.Length);

            // write data chunks
            var encodedPixelData = EncodePixelData(texture2D);
            var compressedPixelData = new MemoryStream();

            ZlibStream deflateStream = null;
            try
            {
                deflateStream = new ZlibStream(compressedPixelData, CompressionMode.Compress);
                deflateStream.Write(encodedPixelData, 0, encodedPixelData.Length);
                deflateStream.Finish();
            }
            catch (Exception exception)
            {
                throw new Exception("An error occurred during DEFLATE compression.", exception);
            }

            var dataChunk = new DataChunk();
            dataChunk.Data = compressedPixelData.ToArray();
            var dataChunkBytes = dataChunk.Encode();
            outputStream.Write(dataChunkBytes, 0, dataChunkBytes.Length);

            deflateStream.Dispose();
            compressedPixelData.Dispose();

            // write end chunk
            var endChunk = new EndChunk();
            var endChunkBytes = endChunk.Encode();
            outputStream.Write(endChunkBytes, 0, endChunkBytes.Length);
        }

        private byte[] EncodePixelData(Texture2D texture2D)
        {
            List<byte[]> filteredScanlines = new List<byte[]>();

            int bytesPerPixel = CalculateBytesPerPixel();
            byte[] previousScanline = new byte[width * bytesPerPixel];

            for (int y = 0; y < height; y++)
            {
                var rawScanline = GetRawScanline(y);

                var filteredScanline = GetOptimalFilteredScanline(rawScanline, previousScanline, bytesPerPixel);

                filteredScanlines.Add(filteredScanline);

                previousScanline = rawScanline;
            }

            List<byte> result = new List<byte>();

            foreach (var encodedScanline in filteredScanlines)
            {
                result.AddRange(encodedScanline);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Applies all PNG filters to the given scanline and returns the filtered scanline that is deemed
        /// to be most compressible, using lowest total variation as proxy for compressibility.
        /// </summary>
        /// <param name="rawScanline"></param>
        /// <param name="previousScanline"></param>
        /// <param name="bytesPerPixel"></param>
        /// <returns></returns>
        private byte[] GetOptimalFilteredScanline(byte[] rawScanline, byte[] previousScanline, int bytesPerPixel)
        {
            var candidates = new List<Tuple<byte[], int>>();
            
            var sub = SubFilter.Encode(rawScanline, bytesPerPixel);
            candidates.Add(new Tuple<byte[], int>(sub, CalculateTotalVariation(sub)));

            var up = UpFilter.Encode(rawScanline, previousScanline);
            candidates.Add(new Tuple<byte[], int>(up, CalculateTotalVariation(up)));

            var average = AverageFilter.Encode(rawScanline, previousScanline, bytesPerPixel);
            candidates.Add(new Tuple<byte[], int>(average, CalculateTotalVariation(average)));

            var paeth = PaethFilter.Encode(rawScanline, previousScanline, bytesPerPixel);
            candidates.Add(new Tuple<byte[], int>(paeth, CalculateTotalVariation(paeth)));

            int lowestTotalVariation = Int32.MaxValue;
            int lowestTotalVariationIndex = 0;

            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].Item2 < lowestTotalVariation)
                {
                    lowestTotalVariationIndex = i;
                    lowestTotalVariation = candidates[i].Item2;
                }
            }

            return candidates[lowestTotalVariationIndex].Item1;
        }

        /// <summary>
        /// Calculates the total variation of given byte array.  Total variation is the sum of the absolute values of
        /// neighbour differences.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int CalculateTotalVariation(byte[] input)
        {
            int totalVariation = 0;

            for (int i = 1; i < input.Length; i++)
            {
                totalVariation += Math.Abs(input[i] - input[i - 1]);
            }

            return totalVariation;
        }

        private byte[] GetRawScanline(int y)
        {
            var rawScanline = new byte[4 * width];
            
            for (int x = 0; x < width; x++)
            {
                var color = colorData[(y * width) + x];

                rawScanline[4 * x] = color.R;
                rawScanline[(4 * x) + 1] = color.G;
                rawScanline[(4 * x) + 2] = color.B;
                rawScanline[(4 * x) + 3] = color.A;
            }

            return rawScanline;
        }

        private int CalculateBytesPerPixel()
        {
            switch (colorType)
            {
                case ColorType.Grayscale:
                    return bitsPerSample / 8;

                case ColorType.GrayscaleWithAlpha:
                    return (2 * bitsPerSample) / 8;

                case ColorType.Palette:
                    return bitsPerSample / 8;

                case ColorType.Rgb:
                    return (3 * bitsPerSample) / 8;

                case ColorType.RgbWithAlpha:
                    return (4 * bitsPerSample) / 8;

                default:
                    return -1;
            }
        }

        private void GetColorData(Texture2D texture2D)
        {
            int colorDataLength = texture2D.Width * texture2D.Height;
            colorData = new Color[colorDataLength];

            switch (texture2D.Format)
            {
                case SurfaceFormat.Single:
                    var floatData = new float[colorDataLength];
                    texture2D.GetData<float>(floatData);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        float brightness = floatData[i];
                        // Export as a greyscale image.
                        colorData[i] = new Color(brightness, brightness, brightness);
                    }
                    break;

                case SurfaceFormat.Color:
                    texture2D.GetData<Color>(colorData);
                    break;

                case SurfaceFormat.Alpha8:
                    var alpha8Data = new Alpha8[colorDataLength];
                    texture2D.GetData<Alpha8>(alpha8Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(((IPackedVector)alpha8Data[i]).ToVector4());
                    }

                    break;
                
                case SurfaceFormat.Bgr565:
                    var bgr565Data = new Bgr565[colorDataLength];
                    texture2D.GetData<Bgr565>(bgr565Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(((IPackedVector)bgr565Data[i]).ToVector4());
                    }

                    break;

                case SurfaceFormat.Bgra4444:
                    var bgra4444Data = new Bgra4444[colorDataLength];
                    texture2D.GetData<Bgra4444>(bgra4444Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(((IPackedVector)bgra4444Data[i]).ToVector4());
                    }

                    break;

                case SurfaceFormat.Bgra5551:
                    var bgra5551Data = new Bgra5551[colorDataLength];
                    texture2D.GetData<Bgra5551>(bgra5551Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(((IPackedVector)bgra5551Data[i]).ToVector4());
                    }
                    break;

                case SurfaceFormat.HalfSingle:
                    var halfSingleData = new HalfSingle[colorDataLength];
                    texture2D.GetData<HalfSingle>(halfSingleData);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(((IPackedVector)halfSingleData[i]).ToVector4());
                    }

                    break;

                case SurfaceFormat.HalfVector2:
                    var halfVector2Data = new HalfVector2[colorDataLength];
                    texture2D.GetData<HalfVector2>(halfVector2Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(((IPackedVector)halfVector2Data[i]).ToVector4());
                    }

                    break;

                case SurfaceFormat.HalfVector4:
                    var halfVector4Data = new HalfVector4[colorDataLength];
                    texture2D.GetData<HalfVector4>(halfVector4Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(((IPackedVector)halfVector4Data[i]).ToVector4());
                    }

                    break;

                case SurfaceFormat.NormalizedByte2:
                    var normalizedByte2Data = new NormalizedByte2[colorDataLength];
                    texture2D.GetData<NormalizedByte2>(normalizedByte2Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(((IPackedVector)normalizedByte2Data[i]).ToVector4());
                    }

                    break;

                case SurfaceFormat.NormalizedByte4:
                    var normalizedByte4Data = new NormalizedByte4[colorDataLength];
                    texture2D.GetData<NormalizedByte4>(normalizedByte4Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(((IPackedVector)normalizedByte4Data[i]).ToVector4());
                    }

                    break;

                case SurfaceFormat.Rg32:
                    var rg32Data = new Rg32[colorDataLength];
                    texture2D.GetData<Rg32>(rg32Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(((IPackedVector)rg32Data[i]).ToVector4());
                    }

                    break;

                case SurfaceFormat.Rgba64:
                    var rgba64Data = new Rgba64[colorDataLength];
                    texture2D.GetData<Rgba64>(rgba64Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(((IPackedVector)rgba64Data[i]).ToVector4());
                    }

                    break;

                case SurfaceFormat.Rgba1010102:
                    var rgba1010102Data = new Rgba1010102[colorDataLength];
                    texture2D.GetData<Rgba1010102>(rgba1010102Data);

                    for (int i = 0; i < colorDataLength; i++)
                    {
                        colorData[i] = new Color(((IPackedVector)rgba1010102Data[i]).ToVector4());
                    }

                    break;

                default:
                    throw new Exception("Texture surface format not supported");
            }
        }
    }
}
