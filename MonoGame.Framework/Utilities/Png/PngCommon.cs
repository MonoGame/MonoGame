// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGame.Utilities.Png
{
    internal class Palette
    {
        private IList<Color> colors;

        internal Palette()
        {
            colors = new List<Color>();
        }

        internal Color this[int index]
        {
            get
            {
                return colors[index];
            }
        }

        internal void AddColor(Color color)
        {
            colors.Add(color);
        }

        internal void AddAlphaToColorAtIndex(int colorIndex, byte alpha)
        {
            var oldColor = colors[colorIndex];

            colors[colorIndex] = new Color(oldColor.R, oldColor.G, oldColor.B, alpha);
        }

        internal void AddAlphaToColors(IList<byte> alphas)
        {
            for (int i = 0; i < alphas.Count; i++)
            {
                AddAlphaToColorAtIndex(i, alphas[i]);
            }
        }
    }

    #region Chunks

    internal class PngChunk
    {
        internal PngChunk()
        {
            this.Data = new byte[0];
        }

        /// <summary>
        /// Length of Data field
        /// </summary>
        internal uint Length
        {
            get;
            set;
        }

        internal string Type
        {
            get;
            set;
        }

        private bool Ancillary
        {
            get;
            set;
        }

        private bool Private
        {
            get;
            set;
        }

        private bool Reserved
        {
            get;
            set;
        }

        private bool SafeToCopy
        {
            get;
            set;
        }

        internal byte[] Data
        {
            get;
            set;
        }

        /// <summary>
        /// CRC of both Type and Data fields, but not Length field
        /// </summary>
        internal uint Crc
        {
            get;
            set;
        }

        internal virtual void Decode(byte[] chunkBytes)
        {
            var chunkBytesList = chunkBytes.ToList();

            this.Length = chunkBytesList.GetRange(0, 4).ToArray().ToUInt();
            DecodeType(chunkBytesList.GetRange(4, 4).ToArray());
            this.Data = chunkBytesList.GetRange(8, (int)this.Length).ToArray();
            this.Crc = chunkBytesList.GetRange((int)(8 + this.Length), 4).ToArray().ToUInt();

            if (CrcCheck() == false)
            {
                throw new Exception("CRC check failed.");
            }
        }

        internal virtual byte[] Encode()
        {
            var result = new List<byte>();

            uint dataLength = (uint)this.Data.Length;
            uint dataCrc = PngCrc.Calculate(InputToCrcCheck());

            result.AddRange(dataLength.ToByteArray());
            result.AddRange(GetChunkTypeBytes(this.Type));
            result.AddRange(this.Data);
            result.AddRange(dataCrc.ToByteArray());

            return result.ToArray();
        }

        private void DecodeType(byte[] typeBytes)
        {
            this.Type = PngChunk.GetChunkTypeString(typeBytes);

            var bitArray = new BitArray(typeBytes);

            this.Ancillary = bitArray[4];
            this.Private = bitArray[9];
            this.Reserved = bitArray[14];
            this.SafeToCopy = bitArray[19];
        }

        private bool CrcCheck()
        {
            var crcInputBytes = InputToCrcCheck();

            return (PngCrc.Calculate(crcInputBytes) == this.Crc);
        }

        private byte[] InputToCrcCheck()
        {
            byte[] chunkTypeBytes = GetChunkTypeBytes(this.Type);
            return chunkTypeBytes.Concat(this.Data).ToArray();
        }

        internal static string GetChunkTypeString(byte[] chunkTypeBytes)
        {
            return Encoding.UTF8.GetString(chunkTypeBytes, 0, chunkTypeBytes.Length);
        }

        private static byte[] GetChunkTypeBytes(string chunkTypeString)
        {
            return Encoding.UTF8.GetBytes(chunkTypeString);
        }
    }

    internal class HeaderChunk : PngChunk
    {
        private static byte[] pngSignature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        
        internal HeaderChunk()
        {
            base.Type = "IHDR";
        }

        internal uint Width
        {
            get;
            set;
        }

        internal uint Height
        {
            get;
            set;
        }

        internal byte BitDepth
        {
            get;
            set;
        }

        internal ColorType ColorType
        {
            get;
            set;
        }

        internal byte CompressionMethod
        {
            get;
            set;
        }

        internal byte FilterMethod
        {
            get;
            set;
        }

        internal byte InterlaceMethod
        {
            get;
            set;
        }

        internal static byte[] PngSignature
        {
            get { return pngSignature; }
        }

        internal override void Decode(byte[] chunkBytes)
        {
            base.Decode(chunkBytes);
            var chunkData = base.Data;

            this.Width = chunkData.Take(4).ToArray().ToUInt();
            this.Height = chunkData.Skip(4).Take(4).ToArray().ToUInt();
            this.BitDepth = chunkData.Skip(8).First();
            this.ColorType = (ColorType)chunkData.Skip(9).First();
            this.CompressionMethod = chunkData.Skip(10).First();
            this.FilterMethod = chunkData.Skip(11).First();
            this.InterlaceMethod = chunkData.Skip(12).First();

            if (this.BitDepth < 8)
            {
                throw new Exception(String.Format("Bit depth less than 8 bits per sample is unsupported.  Image bit depth is {0} bits per sample.", this.BitDepth));
            }
        }

        internal override byte[] Encode()
        {
            var chunkData = new List<byte>();

            chunkData.AddRange(this.Width.ToByteArray().ToList());
            chunkData.AddRange(this.Height.ToByteArray().ToList());
            chunkData.Add(this.BitDepth);
            chunkData.Add((byte)this.ColorType);
            chunkData.Add(this.CompressionMethod);
            chunkData.Add(this.FilterMethod);
            chunkData.Add(this.InterlaceMethod);

            base.Data = chunkData.ToArray();

            return base.Encode();
        }
    }

    internal class PaletteChunk : PngChunk
    {
        internal PaletteChunk()
        {
            base.Type = "PLTE";
            this.Palette = new Palette();
        }

        internal Palette Palette
        {
            get;
            set;
        }

        internal override void Decode(byte[] chunkBytes)
        {
            base.Decode(chunkBytes);
            var chunkData = base.Data;

            if (chunkData.Length % 3 != 0)
            {
                throw new Exception("Malformed palette chunk - length not multiple of 3.");
            }

            for (int i = 0; i < chunkData.Length / 3; i++)
            {
                byte red = chunkData.Skip(3 * i).Take(1).First();
                byte green = chunkData.Skip((3 * i) + 1).Take(1).First();
                byte blue = chunkData.Skip((3 * i) + 2).Take(1).First();

                this.Palette.AddColor(new Color(red, green, blue));
            }
        }
    }

    internal class TransparencyChunk : PngChunk
    {
        internal TransparencyChunk()
        {
            base.Type = "tRNS";
            this.PaletteTransparencies = new List<byte>();
        }

        internal IList<byte> PaletteTransparencies
        {
            get;
            set;
        }

        internal override void Decode(byte[] chunkBytes)
        {
            base.Decode(chunkBytes);
            var chunkData = base.Data;

            this.PaletteTransparencies = chunkData.ToArray();
        }

        internal override byte[] Encode()
        {
            var chunkData = new List<byte>();


            base.Data = chunkData.ToArray();

            return base.Encode();
        }
    }

    internal class DataChunk : PngChunk
    {
        internal DataChunk()
        {
            base.Type = "IDAT";
        }
    }

    internal class EndChunk : PngChunk
    {
        internal EndChunk()
        {
            base.Type = "IEND";
        }
    }

    #endregion
    
    #region Enumerations

    internal enum ColorType
    {
        Grayscale = 0,
        Rgb = 2,
        Palette = 3,
        GrayscaleWithAlpha = 4,
        RgbWithAlpha = 6
    }

    internal enum FilterType
    {
        None = 0,
        Sub = 1,
        Up = 2,
        Average = 3,
        Paeth = 4
    }

    #endregion

    #region Filters

    internal static class NoneFilter
    {
        internal static byte[] Decode(byte[] scanline)
        {
            return scanline;
        }

        internal static byte[] Encode(byte[] scanline)
        {
            var encodedScanline = new byte[scanline.Length + 1];

            encodedScanline[0] = (byte)FilterType.None;
            scanline.CopyTo(encodedScanline, 1);

            return encodedScanline;
        }
    }

    internal static class SubFilter
    {
        internal static byte[] Decode(byte[] scanline, int bytesPerPixel)
        {
            byte[] result = new byte[scanline.Length];

            for (int x = 1; x < scanline.Length; x++)
            {
                byte priorRawByte = (x - bytesPerPixel < 1) ? (byte)0 : result[x - bytesPerPixel];

                result[x] = (byte)((scanline[x] + priorRawByte) % 256);
            }

            return result;
        }

        internal static byte[] Encode(byte[] scanline, int bytesPerPixel)
        {
            var encodedScanline = new byte[scanline.Length + 1];

            encodedScanline[0] = (byte)FilterType.Sub;

            for (int x = 0; x < scanline.Length; x++)
            {
                byte priorRawByte = (x - bytesPerPixel < 0) ? (byte)0 : scanline[x - bytesPerPixel];

                encodedScanline[x + 1] = (byte)((scanline[x] - priorRawByte) % 256);
            }

            return encodedScanline;
        }
    }

    internal static class UpFilter
    {
        internal static byte[] Decode(byte[] scanline, byte[] previousScanline)
        {
            byte[] result = new byte[scanline.Length];

            for (int x = 1; x < scanline.Length; x++)
            {
                byte above = previousScanline[x];

                result[x] = (byte)((scanline[x] + above) % 256);
            }

            return result;
        }

        internal static byte[] Encode(byte[] scanline, byte[] previousScanline)
        {
            var encodedScanline = new byte[scanline.Length + 1];

            encodedScanline[0] = (byte)FilterType.Up;

            for (int x = 0; x < scanline.Length; x++)
            {
                byte above = previousScanline[x];

                encodedScanline[x + 1] = (byte)((scanline[x] - above) % 256);
            }

            return encodedScanline;
        }
    }

    internal static class AverageFilter
    {
        internal static byte[] Decode(byte[] scanline, byte[] previousScanline, int bytesPerPixel)
        {
            byte[] result = new byte[scanline.Length];

            for (int x = 1; x < scanline.Length; x++)
            {
                byte left = (x - bytesPerPixel < 1) ? (byte)0 : result[x - bytesPerPixel];
                byte above = previousScanline[x];

                result[x] = (byte)((scanline[x] + Average(left, above)) % 256);
            }

            return result;
        }

        internal static byte[] Encode(byte[] scanline, byte[] previousScanline, int bytesPerPixel)
        {
            var encodedScanline = new byte[scanline.Length + 1];

            encodedScanline[0] = (byte)FilterType.Average;

            for (int x = 0; x < scanline.Length; x++)
            {
                byte left = (x - bytesPerPixel < 0) ? (byte)0 : scanline[x - bytesPerPixel];
                byte above = previousScanline[x];

                encodedScanline[x + 1] = (byte)((scanline[x] - Average(left, above)) % 256);
            }

            return encodedScanline;
        }

        private static int Average(byte left, byte above)
        {
            return Convert.ToInt32(Math.Floor((left + above) / 2.0));
        }
    }

    internal static class PaethFilter
    {
        internal static byte[] Decode(byte[] scanline, byte[] previousScanline, int bytesPerPixel)
        {
            byte[] result = new byte[scanline.Length];

            for (int x = 1; x < scanline.Length; x++)
            {
                byte left = (x - bytesPerPixel < 1) ? (byte)0 : result[x - bytesPerPixel];
                byte above = previousScanline[x];
                byte upperLeft = (x - bytesPerPixel < 1) ? (byte)0 : previousScanline[x - bytesPerPixel];

                result[x] = (byte)((scanline[x] + PaethPredictor(left, above, upperLeft)) % 256);
            }

            return result;
        }

        internal static byte[] Encode(byte[] scanline, byte[] previousScanline, int bytesPerPixel)
        {
            var encodedScanline = new byte[scanline.Length + 1];

            encodedScanline[0] = (byte)FilterType.Paeth;

            for (int x = 0; x < scanline.Length; x++)
            {
                byte left = (x - bytesPerPixel < 0) ? (byte)0 : scanline[x - bytesPerPixel];
                byte above = previousScanline[x];
                byte upperLeft = (x - bytesPerPixel < 0) ? (byte)0 : previousScanline[x - bytesPerPixel];

                encodedScanline[x + 1] = (byte)((scanline[x] - PaethPredictor(left, above, upperLeft)) % 256);
            }

            return encodedScanline;
        }

        private static int PaethPredictor(int a, int b, int c)
        {
            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);

            if ((pa <= pb) && (pa <= pc))
            {
                return a;
            }
            else
            {
                if (pb <= pc)
                {
                    return b;
                }
                else
                {
                    return c;
                }
            }
        }
    }

    #endregion

    internal static class PngCrc
    {
        // table of CRCs of all 8-bit messages
        private static uint[] crcTable = null;

        static PngCrc()
        {
            BuildCrcTable();
        }

        /// <summary>
        /// Build CRC lookup table for performance (once-off)
        /// </summary>
        private static void BuildCrcTable()
        {
            crcTable = new uint[256];

            uint c, n, k;

            for (n = 0; n < 256; n++)
            {
                c = n;

                for (k = 0; k < 8; k++)
                {
                    if ((c & 1) > 0)
                    {
                        c = 0xedb88320 ^ (c >> 1);
                    }
                    else
                    {
                        c = c >> 1;
                    }
                }

                crcTable[n] = c;
            }
        }

        internal static uint Calculate(byte[] bytes)
        {
            uint c = 0xffffffff;

            int n;

            if (crcTable == null)
            {
                BuildCrcTable();
            }

            for (n = 0; n < bytes.Length; n++)
            {
                c = crcTable[(c ^ bytes[n]) & 0xff] ^ (c >> 8);
            }

            return c ^ 0xffffffff;
        }
    }

    internal static class Extensions
    {
        internal static uint ToUInt(this byte[] bytes)
        {
            byte[] input;

            if (BitConverter.IsLittleEndian)
            {
                input = ReverseByteArray(bytes);
            }
            else
            {
                input = bytes;
            }

            return BitConverter.ToUInt32(input, 0);
        }

        internal static byte[] ToByteArray(this uint integer)
        {
            byte[] output = BitConverter.GetBytes(integer);

            if (BitConverter.IsLittleEndian)
            {
                return ReverseByteArray(output);
            }
            else
            {
                return output;
            }
        }

        private static byte[] ReverseByteArray(byte[] byteArray)
        {
            return (byte[])byteArray.Reverse().ToArray();
        }
    }
}
