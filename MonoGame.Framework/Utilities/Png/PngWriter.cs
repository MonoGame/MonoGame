// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Utilities.ZLib;

namespace MonoGame.Utilities.Png
{
    public class PngWriter
    {
        private byte[] pngSignature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        private const int bitsPerSample = 8;
        private ColorType colorType;

        public PngWriter()
        {
            colorType = ColorType.Rgb;
        }

        public Stream Write(Bitmap bitmap)
        {
            var outputStream = new MemoryStream();

            // write PNG signature
            outputStream.Write(pngSignature, 0, pngSignature.Length);

            // write header chunk
            var headerChunk = new HeaderChunk();
            headerChunk.Width = (uint)bitmap.Width;
            headerChunk.Height = (uint)bitmap.Height;
            headerChunk.BitDepth = 8;
            headerChunk.ColorType = colorType;
            headerChunk.CompressionMethod = 0;
            headerChunk.FilterMethod = 0;
            headerChunk.InterlaceMethod = 0;

            var headerChunkBytes = headerChunk.Encode();
            outputStream.Write(headerChunkBytes, 0, headerChunkBytes.Length);

            // write data chunks
            var encodedPixelData = EncodePixelData(bitmap);

            var compressedPixelData = new MemoryStream();
            ZStreamUtilities.CompressStream(new MemoryStream(encodedPixelData), compressedPixelData);
            
            var dataChunk = new DataChunk();
            dataChunk.Data = compressedPixelData.ToArray();
            var dataChunkBytes = dataChunk.Encode();
            outputStream.Write(dataChunkBytes, 0, dataChunkBytes.Length);

            // write end chunk

            var endChunk = new EndChunk();
            var endChunkBytes = endChunk.Encode();
            outputStream.Write(endChunkBytes, 0, endChunkBytes.Length);
            
            return outputStream;
        }

        private byte[] EncodePixelData(Bitmap bitmap)
        {
            List<byte[]> filteredScanlines = new List<byte[]>();

            int width = bitmap.Width;
            int height = bitmap.Height;
            int bytesPerPixel = CalculateBytesPerPixel();
            byte[] previousScanline = new byte[width * bytesPerPixel];

            for (int y = 0; y < height; y++)
            {
                var rawScanline = GetRawScanline(bitmap, y);

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

        private int CalculateTotalVariation(byte[] input)
        {
            int totalVariation = 0;

            for (int i = 1; i < input.Length; i++)
            {
                totalVariation += Math.Abs(input[i] - input[i - 1]);
            }

            return totalVariation;
        }

        private byte[] GetRawScanline(Bitmap bitmap, int y)
        {
            int width = bitmap.Width;

            var rawScanline = new byte[3 * width];
            
            for (int x = 0; x < width; x++)
            {
                var color = bitmap.GetPixel(x, y);

                rawScanline[3 * x] = color.R;
                rawScanline[(3 * x) + 1] = color.G;
                rawScanline[(3 * x) + 2] = color.B;
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
    }
}
