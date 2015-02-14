// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Utilities.ZLib;

namespace MonoGame.Utilities.Png
{
    public class PngReader
    {
        private byte[] pngSignature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        private int width;
        private int height;
        private int bitsPerSample;
        private int bytesPerSample;
        private int bytesPerPixel;
        private int bytesPerScanline;
        private IList<PngChunk> chunks;
        private IList<PngChunk> dataChunks;
        private ColorType colorType;
        private Palette palette;
        private Bitmap bitmap;
        
        public PngReader()
        {
            chunks = new List<PngChunk>();
            dataChunks = new List<PngChunk>();
        }

        public Bitmap Read(Stream inputStream)
        {
            byte[] signature = new byte[8];
            inputStream.Read(signature, 0, 8);

            if (signature.EqualsByElement(pngSignature) == false)
            {
                throw new Exception("File does not have PNG signature.");
            }

            while (inputStream.Position != inputStream.Length)
            {
                byte[] chunkDataLengthBytes = new byte[4];
                inputStream.Read(chunkDataLengthBytes, 0, 4);
                uint chunkDataLength = chunkDataLengthBytes.ToUInt();

                inputStream.Position -= 4;

                byte[] chunkBytes = new byte[12 + chunkDataLength];
                inputStream.Read(chunkBytes, 0, (int)(12 + chunkDataLength));

                ProcessChunk(chunkBytes);
            }

            UnpackDataChunks();

            return bitmap;
        }

        private void ProcessChunk(byte[] chunkBytes)
        {
            string chunkType = PngChunk.GetChunkTypeString(chunkBytes.Skip(4).Take(4).ToArray());

            switch (chunkType)
            {
                case "IHDR":

                    var headerChunk = new HeaderChunk();
                    headerChunk.Decode(chunkBytes);
                    width = (int)headerChunk.Width;
                    height = (int)headerChunk.Height;
                    bitsPerSample = (int)headerChunk.BitDepth;
                    colorType = headerChunk.ColorType;
                    chunks.Add(headerChunk);

                    break;

                case "PLTE":

                    var paletteChunk = new PaletteChunk();
                    paletteChunk.Decode(chunkBytes);
                    palette = paletteChunk.Palette;
                    chunks.Add(paletteChunk);

                    break;

                case "tRNS":

                    var transparencyChunk = new TransparencyChunk();
                    transparencyChunk.Decode(chunkBytes);
                    palette.AddAlphaToColors(transparencyChunk.PaletteTransparencies);
                    break;

                case "IDAT":

                    var dataChunk = new DataChunk();
                    dataChunk.Decode(chunkBytes);
                    dataChunks.Add(dataChunk);

                    break;

                default:
                    break;
            }
        }

        private void UnpackDataChunks()
        {
            var dataByteList = new List<byte>();

            foreach (var dataChunk in dataChunks)
            {
                if (dataChunk.Type == "IDAT")
                {
                    dataByteList.AddRange(dataChunk.Data);
                }
            }

            var compressedStream = new MemoryStream(dataByteList.ToArray());
            var decompressedStream = new MemoryStream();

            ZStreamUtilities.DecompressStream(compressedStream, decompressedStream);

            var decompressedBytes = decompressedStream.ToArray();
            var pixelData = DeserializePixelData(decompressedBytes);

            DecodePixelData(pixelData);
        }

        private byte[][] DeserializePixelData(byte[] pixelData)
        {
            bytesPerPixel = CalculateBytesPerPixel();
            bytesPerSample = bitsPerSample / 8;
            bytesPerScanline = (bytesPerPixel * width) + 1;
            int scanlineCount = pixelData.Length / bytesPerScanline;

            if (pixelData.Length % bytesPerScanline != 0)
            {
                throw new Exception("Malformed pixel data - not multiple of ((bytesPerPixel * width) + 1)");
            }

            var result = new byte[scanlineCount][];

            for (int y = 0; y < scanlineCount; y++)
            {
                result[y] = new byte[bytesPerScanline];
                
                for (int x = 0; x < bytesPerScanline; x++)
                {
                    result[y][x] = pixelData[y * bytesPerScanline + x];
                }
            }
            
            return result;
        }

        private void DecodePixelData(byte[][] pixelData)
        {
            bitmap = new Bitmap(width, height);

            byte[] previousScanline = new byte[bytesPerScanline];

            for (int y = 0; y < height; y++)
            {
                var scanline = pixelData[y];

                FilterType filterType = (FilterType)scanline[0];
                byte[] defilteredScanline;

                switch (filterType)
                {
                    case FilterType.None:

                        defilteredScanline = NoneFilter.Decode(scanline);

                        break;

                    case FilterType.Sub:

                        defilteredScanline = SubFilter.Decode(scanline, bytesPerPixel);

                        break;

                    case FilterType.Up:

                        defilteredScanline = UpFilter.Decode(scanline, previousScanline);

                        break;

                    case FilterType.Average:

                        defilteredScanline = AverageFilter.Decode(scanline, previousScanline, bytesPerPixel);

                        break;

                    case FilterType.Paeth:

                        defilteredScanline = PaethFilter.Decode(scanline, previousScanline, bytesPerPixel);

                        break;

                    default:
                        throw new Exception("Unknown filter type.");
                }

                previousScanline = defilteredScanline;
                ProcessDefilteredScanline(defilteredScanline, y);
            }
        }

        private void ProcessDefilteredScanline(byte[] defilteredScanline, int y)
        {
            switch (colorType)
            {
                case ColorType.Grayscale:

                    for (int x = 0; x < width; x++)
                    {
                        int offset = 1 + (x * bytesPerPixel);

                        byte intensity = defilteredScanline[offset];

                        bitmap.SetPixel(x, y, Color.FromArgb(intensity, intensity, intensity));
                    }

                    break;

                case ColorType.GrayscaleWithAlpha:

                    for (int x = 0; x < width; x++)
                    {
                        int offset = 1 + (x * bytesPerPixel);

                        byte intensity = defilteredScanline[offset];
                        byte alpha = defilteredScanline[offset + bytesPerSample];

                        bitmap.SetPixel(x, y, Color.FromArgb(alpha, intensity, intensity, intensity));
                    }

                    break;

                case ColorType.Palette:

                    for (int x = 0; x < width; x++)
                    {
                        var pixelColor = palette[defilteredScanline[x + 1]];

                        bitmap.SetPixel(x, y, Color.FromArgb(pixelColor.Item4, pixelColor.Item1, pixelColor.Item2, pixelColor.Item3));
                    }

                    break;

                case ColorType.Rgb:

                    for (int x = 0; x < width; x++)
                    {
                        int offset = 1 + (x * bytesPerPixel);
                        
                        int red = defilteredScanline[offset];
                        int green = defilteredScanline[offset + bytesPerSample];
                        int blue = defilteredScanline[offset + 2 * bytesPerSample];

                        bitmap.SetPixel(x, y, Color.FromArgb(red, green, blue));
                    }

                    break;

                case ColorType.RgbWithAlpha:

                    for (int x = 0; x < width; x++)
                    {
                        int offset = 1 + (x * bytesPerPixel);

                        int red = defilteredScanline[offset];
                        int green = defilteredScanline[offset + bytesPerSample];
                        int blue = defilteredScanline[offset + 2 * bytesPerSample];
                        int alpha = defilteredScanline[offset + 3 * bytesPerSample];

                        bitmap.SetPixel(x, y, Color.FromArgb(alpha, red, green, blue));
                    }

                    break;

                default:
                    break;
            }
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
                    throw new Exception("Unknown color type.");
            }
        }
    }
}