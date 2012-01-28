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
						DecompressDxt1Block(imageReader, x, y, blockCountX, imageData);
					}
                }
            }

            return imageData;
        }

        private static void DecompressDxt1Block(BinaryReader imageReader, int x, int y, int width, byte[] imageData)
        {
            ushort c0 = imageReader.ReadUInt16();
            ushort c1 = imageReader.ReadUInt16();

            byte[] color0 = ConvertRgb565ToRgb888(c0);
            byte[] color1 = ConvertRgb565ToRgb888(c1);

            uint lookupTable = imageReader.ReadUInt32();

            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    byte[] finalColor = new byte[4];
                    uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;
                    
                    if (c0 > c1)
                    {
                        switch (index)
                        {
                            case 0: finalColor = GetRgba(color0[0], color0[1], color0[2]); break;
                            case 1: finalColor = GetRgba(color1[0], color1[1], color1[2]); break;
                            case 2: finalColor = GetRgba((byte)((2 * color0[0] + color1[0]) / 3), (byte)((2 * color0[1] + color1[1]) / 3), (byte)((2 * color0[2] + color1[2]) / 3)); break;
                            case 3: finalColor = GetRgba((byte)((color0[0] + 2 * color1[0]) / 3), (byte)((color0[1] + 2 * color1[1]) / 3), (byte)((color0[2] + 2 * color1[2]) / 3)); break;
                        }
                    }
                    else
                    {
                        switch (index)
                        {
                            case 0: finalColor = GetRgba(color0[0], color0[1], color0[2]); break;
                            case 1: finalColor = GetRgba(color1[0], color1[1], color1[2]); break;
                            case 2: finalColor = GetRgba((byte)((color0[0] + color1[0]) / 2), (byte)((color0[1] + color1[1]) / 2), (byte)((color0[2] + color1[2]) / 2)); break;
                            case 3: finalColor = GetRgba(0, 0, 0); break;
                        }
                    }
					
					imageData[y * width * 64 + blockY * width * 16 + x * 16 + blockX * 4] = finalColor[0];
					imageData[y * width * 64 + blockY * width * 16 + x * 16 + blockX * 4 + 1] = finalColor[1];
					imageData[y * width * 64 + blockY * width * 16 + x * 16 + blockX * 4 + 2] = finalColor[2];
					imageData[y * width * 64 + blockY * width * 16 + x * 16 + blockX * 4 + 3] = finalColor[3];
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
                        DecompressDxt3Block(imageReader, x, y, blockCountX, imageData);
					}
                }
            }

            return imageData;
        }

        private static void DecompressDxt3Block(BinaryReader imageReader, int x, int y, int width, byte[] imageData)
        {
            byte[] alpha = imageReader.ReadBytes(8);
            
            ushort c0 = imageReader.ReadUInt16();
            ushort c1 = imageReader.ReadUInt16();

            byte[] color0 = ConvertRgb565ToRgb888(c0);
            byte[] color1 = ConvertRgb565ToRgb888(c1);

            uint lookupTable = imageReader.ReadUInt32();

            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    byte[] finalColor = new byte[4];
                    uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;

                    switch (index)
                    {
                        case 0: finalColor = GetRgba(color0[0], color0[1], color0[2], Convert8BitTo4Bit(alpha[(4 * blockY + blockX) / 2])[(4 * blockY + blockX) % 2]); break;
                        case 1: finalColor = GetRgba(color1[0], color1[1], color1[2], Convert8BitTo4Bit(alpha[(4 * blockY + blockX) / 2])[(4 * blockY + blockX) % 2]); break;
                        case 2: finalColor = GetRgba((byte)((2 * color0[0] + color1[0]) / 3), (byte)((2 * color0[1] + color1[1]) / 3), (byte)((2 * color0[2] + color1[2]) / 3), Convert8BitTo4Bit(alpha[(4 * blockY + blockX) / 2])[(4 * blockY + blockX) % 2]); break;
                        case 3: finalColor = GetRgba((byte)((color0[0] + 2 * color1[0]) / 3), (byte)((color0[1] + 2 * color1[1]) / 3), (byte)((color0[2] + 2 * color1[2]) / 3), Convert8BitTo4Bit(alpha[(4 * blockY + blockX) / 2])[(4 * blockY + blockX) % 2]); break;
                    }

					imageData[y * width * 64 + blockY * width * 16 + x * 16 + blockX * 4] = finalColor[0];
					imageData[y * width * 64 + blockY * width * 16 + x * 16 + blockX * 4 + 1] = finalColor[1];
					imageData[y * width * 64 + blockY * width * 16 + x * 16 + blockX * 4 + 2] = finalColor[2];
					imageData[y * width * 64 + blockY * width * 16 + x * 16 + blockX * 4 + 3] = finalColor[3];
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
						DecompressDxt5Block(imageReader, x, y, blockCountX, imageData);
					}
                }
            }

            return imageData;
        }
		
        private static void DecompressDxt5Block(BinaryReader imageReader, int x, int y, int width, byte[] imageData)
		{
            byte alpha0 = imageReader.ReadByte();
            byte alpha1 = imageReader.ReadByte();
			
			byte[] bits = imageReader.ReadBytes(6);
			int alphaCode1 = bits[2] | (bits[3] << 8) | (bits[4] << 16) | (bits[5] << 24);
			int alphaCode2 = bits[0] | (bits[1] << 8);
			
			ushort color0 = imageReader.ReadUInt16();
			ushort color1 = imageReader.ReadUInt16();
			
			int temp;
 
			temp = (color0 >> 11) * 255 + 16;
			byte r0 = (byte)((temp/32 + temp)/32);
			temp = ((color0 & 0x07E0) >> 5) * 255 + 32;
			byte g0 = (byte)((temp/64 + temp)/64);
			temp = (color0 & 0x001F) * 255 + 16;
			byte b0 = (byte)((temp/32 + temp)/32);
 
			temp = (color1 >> 11) * 255 + 16;
			byte r1 = (byte)((temp/32 + temp)/32);
			temp = ((color1 & 0x07E0) >> 5) * 255 + 32;
			byte g1 = (byte)((temp/64 + temp)/64);
			temp = (color1 & 0x001F) * 255 + 16;
			byte b1 = (byte)((temp/32 + temp)/32);
 
			int code = imageReader.ReadInt32();
 
			for (int blockY=0; blockY < 4; blockY++)
			{
				for (int blockX=0; blockX < 4; blockX++)
				{
					int alphaCodeIndex = 3*(4*blockY+blockX);
					int alphaCode;
		 
					if (alphaCodeIndex <= 12)
					{
						alphaCode = (alphaCode2 >> alphaCodeIndex) & 0x07;
					}
					else if (alphaCodeIndex == 15)
					{
						alphaCode = (alphaCode2 >> 15) | ((alphaCode1 << 1) & 0x06);
					}
					else
					{
						alphaCode = (alphaCode1 >> (alphaCodeIndex - 16)) & 0x07;
					}
		 
					byte finalAlpha;
					if (alphaCode == 0)
					{
						finalAlpha = alpha0;
					}
					else if (alphaCode == 1)
					{
						finalAlpha = alpha1;
					}
					else
					{
						if (alpha0 > alpha1)
						{
							finalAlpha = (byte)(((8-alphaCode)*alpha0 + (alphaCode-1)*alpha1)/7);
						}
						else
						{
							if (alphaCode == 6)
								finalAlpha = 0;
							else if (alphaCode == 7)
								finalAlpha = 255;
							else
								finalAlpha = (byte)(((6-alphaCode)*alpha0 + (alphaCode-1)*alpha1)/5);
						}
					}
		 
					byte colorCode = (byte)((code >> 2*(4*blockY+blockX)) & 0x03);
		 
					byte[] finalColor = new byte[4];
					switch (colorCode)
					{
						case 0:
							finalColor = GetRgba(r0, g0, b0, finalAlpha);
							break;
						case 1:
							finalColor = GetRgba(r1, g1, b1, finalAlpha);
							break;
						case 2:
							finalColor = GetRgba((byte)((2*r0+r1)/3), (byte)((2*g0+g1)/3), (byte)((2*b0+b1)/3), finalAlpha);
							break;
						case 3:
							finalColor = GetRgba((byte)((r0+2*r1)/3), (byte)((g0+2*g1)/3), (byte)((b0+2*b1)/3), finalAlpha);
							break;
					}
		 
					if (x + blockX < width)
					{
						imageData[y * width * 64 + blockY * width * 16 + x * 16 + blockX * 4] = finalColor[0];
						imageData[y * width * 64 + blockY * width * 16 + x * 16 + blockX * 4 + 1] = finalColor[1];
						imageData[y * width * 64 + blockY * width * 16 + x * 16 + blockX * 4 + 2] = finalColor[2];
						imageData[y * width * 64 + blockY * width * 16 + x * 16 + blockX * 4 + 3] = finalColor[3];
					}					
				}
			}
		}
		
		private static byte[] PackRGBA(byte r, byte g, byte b, byte a)
		{
			return new byte[] {r, g, b, a};
		}
		
        private static byte[] ConvertRgb565ToRgb888(ushort color)
        {
            int temp;

            temp = (color >> 11) * 255 + 16;
            byte r = (byte)((temp / 32 + temp) / 32);
            temp = ((color & 0x07E0) >> 5) * 255 + 32;
            byte g = (byte)((temp / 64 + temp) / 64);
            temp = (color & 0x001F) * 255 + 16;
            byte b = (byte)((temp / 32 + temp) / 32);

            return new byte[3] { r, g, b };
        }

        private static byte[] Convert8BitTo4Bit(byte value)
        {
            byte low = (byte)(value & 0x0F);
            byte high = (byte)(value & 0xF0);

            byte value1 = (byte)(low | (low << 4));
            byte value2 = (byte)(high | (high >> 4));

            return new byte[] { value1, value2 };
        }

        private static byte[] GetRgba(byte r, byte g, byte b)
        {
            return new byte[4] { r, g, b, 255 };
        }

        private static byte[] GetRgba(byte r, byte g, byte b, byte a)
        {
            return new byte[4] { r, g, b, a };
        }
	}
}

