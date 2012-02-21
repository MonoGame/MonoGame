// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 
using System;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	internal static class DxtUtil
	{
		internal static byte[] DecompressDxt1(byte[] imageData, int width, int height)
        {
            using (MemoryStream imageStream = new MemoryStream(imageData))
                return DecompressDxt1(imageStream, width, height);
        }

        internal static byte[] DecompressDxt1(Stream imageStream, int width, int height)
        {
            byte[] imageData = new byte[width * height * 4];

            using (BinaryReader imageReader = new BinaryReader(imageStream))
            {
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;
                
                for (int y = 0; y < blockCountY; y++)
                {
                    for (int x = 0; x < blockCountX; x++)
                    {
						DecompressDxt1Block(imageReader, x, y, blockCountX, width, height, imageData);
					}
                }
            }

            return imageData;
        }

        private static void DecompressDxt1Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
            ushort c0 = imageReader.ReadUInt16();
            ushort c1 = imageReader.ReadUInt16();

			byte r0, g0, b0;
			byte r1, g1, b1;
            ConvertRgb565ToRgb888(c0, out r0, out g0, out b0);
			ConvertRgb565ToRgb888(c1, out r1, out g1, out b1);

            uint lookupTable = imageReader.ReadUInt32();

            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
					byte r = 0, g = 0, b = 0, a = 255;
                    uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;
                    
                    if (c0 > c1)
                    {
                        switch (index)
                        {
							case 0:
								r = r0;
								g = g0;
								b = b0;
								break;
							case 1:
								r = r1;
								g = g1;
								b = b1;
								break;
                            case 2:
								r = (byte)((2 * r0 + r1) / 3);
								g = (byte)((2 * g0 + g1) / 3);
								b = (byte)((2 * b0 + b1) / 3);
								break;
                            case 3:
								r = (byte)((r0 + 2 * r1) / 3);
								g = (byte)((g0 + 2 * g1) / 3);
								b = (byte)((b0 + 2 * b1) / 3);
								break;
                        }
                    }
                    else
                    {
                        switch (index)
                        {
							case 0:
								r = r0;
								g = g0;
								b = b0;
								break;
							case 1:
								r = r1;
								g = g1;
								b = b1;
								break;
							case 2:
								r = (byte)((r0 + r1) / 2);
								g = (byte)((g0 + g1) / 2);
								b = (byte)((b0 + b1) / 2);
								break;
							case 3:
								r = 0;
								g = 0;
								b = 0;
								a = 0;
								break;
                        }
                    }

					int px = (x << 2) + blockX;
					int py = (y << 2) + blockY;
					if ((px < width) && (py < height))
					{
						int offset = ((py * width) + px) << 2;
						imageData[offset] = r;
						imageData[offset + 1] = g;
						imageData[offset + 2] = b;
						imageData[offset + 3] = a;
					}
                }
			}
        }
        
        internal static byte[] DecompressDxt3(byte[] imageData, int width, int height)
        {
            using (MemoryStream imageStream = new MemoryStream(imageData))
                return DecompressDxt3(imageStream, width, height);
        }

        internal static byte[] DecompressDxt3(Stream imageStream, int width, int height)
        {
            byte[] imageData = new byte[width * height * 4];

            using (BinaryReader imageReader = new BinaryReader(imageStream))
            {
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;

                for (int y = 0; y < blockCountY; y++)
                {
                    for (int x = 0; x < blockCountX; x++)
                    {
                        DecompressDxt3Block(imageReader, x, y, blockCountX, width, height, imageData);
					}
                }
            }

            return imageData;
        }

        private static void DecompressDxt3Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
            byte a0 = imageReader.ReadByte();
			byte a1 = imageReader.ReadByte();
			byte a2 = imageReader.ReadByte();
			byte a3 = imageReader.ReadByte();
			byte a4 = imageReader.ReadByte();
			byte a5 = imageReader.ReadByte();
			byte a6 = imageReader.ReadByte();
			byte a7 = imageReader.ReadByte();
            
            ushort c0 = imageReader.ReadUInt16();
            ushort c1 = imageReader.ReadUInt16();

			byte r0, g0, b0;
			byte r1, g1, b1;
			ConvertRgb565ToRgb888(c0, out r0, out g0, out b0);
			ConvertRgb565ToRgb888(c1, out r1, out g1, out b1);

            uint lookupTable = imageReader.ReadUInt32();

			int alphaIndex = 0;
            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
					byte r = 0, g = 0, b = 0, a = 0;

                    uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;
					
					switch (alphaIndex)
					{
						case 0:
							a = (byte)((a0 & 0xF0) | ((a0 & 0xF0) >> 4));
							break;
						case 1:
							a = (byte)((a0 & 0x0F) | ((a0 & 0x0F) << 4));
							break;
						case 2:
							a = (byte)((a1 & 0xF0) | ((a1 & 0xF0) >> 4));
							break;
						case 3:
							a = (byte)((a1 & 0x0F) | ((a1 & 0x0F) << 4));
							break;
						case 4:
							a = (byte)((a2 & 0xF0) | ((a2 & 0xF0) >> 4));
							break;
						case 5:
							a = (byte)((a2 & 0x0F) | ((a2 & 0x0F) << 4));
							break;
						case 6:
							a = (byte)((a3 & 0xF0) | ((a3 & 0xF0) >> 4));
							break;
						case 7:
							a = (byte)((a3 & 0x0F) | ((a3 & 0x0F) << 4));
							break;
						case 8:
							a = (byte)((a4 & 0xF0) | ((a4 & 0xF0) >> 4));
							break;
						case 9:
							a = (byte)((a4 & 0x0F) | ((a4 & 0x0F) << 4));
							break;
						case 10:
							a = (byte)((a5 & 0xF0) | ((a5 & 0xF0) >> 4));
							break;
						case 11:
							a = (byte)((a5 & 0x0F) | ((a5 & 0x0F) << 4));
							break;
						case 12:
							a = (byte)((a6 & 0xF0) | ((a6 & 0xF0) >> 4));
							break;
						case 13:
							a = (byte)((a6 & 0x0F) | ((a6 & 0x0F) << 4));
							break;
						case 14:
							a = (byte)((a7 & 0xF0) | ((a7 & 0xF0) >> 4));
							break;
						case 15:
							a = (byte)((a7 & 0x0F) | ((a7 & 0x0F) << 4));
							break;
					}
					++alphaIndex;

                    switch (index)
                    {
						case 0:
							r = r0;
							g = g0;
							b = b0;
							break;
						case 1:
							r = r1;
							g = g1;
							b = b1;
							break;
						case 2:
							r = (byte)((2 * r0 + r1) / 3);
							g = (byte)((2 * g0 + g1) / 3);
							b = (byte)((2 * b0 + b1) / 3);
							break;
						case 3:
							r = (byte)((r0 + 2 * r1) / 3);
							g = (byte)((g0 + 2 * g1) / 3);
							b = (byte)((b0 + 2 * b1) / 3);
							break;
					}

					int px = (x << 2) + blockX;
					int py = (y << 2) + blockY;
					if ((px < width) && (py < height))
					{
						int offset = ((py * width) + px) << 2;
						imageData[offset] = r;
						imageData[offset + 1] = g;
						imageData[offset + 2] = b;
						imageData[offset + 3] = a;
					}
				}
            }
        }
		
        internal static byte[] DecompressDxt5(byte[] imageData, int width, int height)
        {
            using (MemoryStream imageStream = new MemoryStream(imageData))
                return DecompressDxt5(imageStream, width, height);
        }
        
        internal static byte[] DecompressDxt5(Stream imageStream, int width, int height)
		{
            byte[] imageData = new byte[width * height * 4];

            using (BinaryReader imageReader = new BinaryReader(imageStream))
            {
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;
                
                for (int y = 0; y < blockCountY; y++)
                {
                    for (int x = 0; x < blockCountX; x++)
                    {
                        DecompressDxt5Block(imageReader, x, y, blockCountX, width, height, imageData);
                    }
                }
            }

            return imageData;
        }

        private static void DecompressDxt5Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
            byte alpha0 = imageReader.ReadByte();
            byte alpha1 = imageReader.ReadByte();
            
            ulong alphaMask = imageReader.ReadUInt32 ();
            alphaMask <<= 16;
            alphaMask += imageReader.ReadUInt16 ();
            
            ushort c0 = imageReader.ReadUInt16();
            ushort c1 = imageReader.ReadUInt16();

			byte r0, g0, b0;
			byte r1, g1, b1;
			ConvertRgb565ToRgb888(c0, out r0, out g0, out b0);
			ConvertRgb565ToRgb888(c1, out r1, out g1, out b1);

            uint lookupTable = imageReader.ReadUInt32();

            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
					byte r = 0, g = 0, b = 0, a = 255;
                    uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;
                    
                    uint alphaIndex = (uint)((alphaMask >> 3 * (4 * blockY + blockX)) & 0x07);
                    if (alphaIndex == 0)
					{
                        a = alpha0;
                    }
					else if (alphaIndex == 1)
					{
                        a = alpha1;
                    }
					else if (alpha0 > alpha1)
					{
                        a = (byte)(((8 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 7);
                    }
					else if (alphaIndex == 6)
					{
                        a = 0;
                    }
					else if (alphaIndex == 7)
					{
                        a = 0xff;
                    }
					else
					{
                        a = (byte)(((6 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 5);
                    }

					switch (index)
					{
						case 0:
							r = r0;
							g = g0;
							b = b0;
							break;
						case 1:
							r = r1;
							g = g1;
							b = b1;
							break;
						case 2:
							r = (byte)((2 * r0 + r1) / 3);
							g = (byte)((2 * g0 + g1) / 3);
							b = (byte)((2 * b0 + b1) / 3);
							break;
						case 3:
							r = (byte)((r0 + 2 * r1) / 3);
							g = (byte)((g0 + 2 * g1) / 3);
							b = (byte)((b0 + 2 * b1) / 3);
							break;
					}

					int px = (x << 2) + blockX;
					int py = (y << 2) + blockY;
					if ((px < width) && (py < height))
					{
						int offset = ((py * width) + px) << 2;
						imageData[offset] = r;
						imageData[offset + 1] = g;
						imageData[offset + 2] = b;
						imageData[offset + 3] = a;
					}
				}
            }
        }
        		
		private static void ConvertRgb565ToRgb888(ushort color, out byte r, out byte g, out byte b)
		{
			int temp;

			temp = (color >> 11) * 255 + 16;
			r = (byte)((temp / 32 + temp) / 32);
			temp = ((color & 0x07E0) >> 5) * 255 + 32;
			g = (byte)((temp / 64 + temp) / 64);
			temp = (color & 0x001F) * 255 + 16;
			b = (byte)((temp / 32 + temp) / 32);
		}
	}
}

