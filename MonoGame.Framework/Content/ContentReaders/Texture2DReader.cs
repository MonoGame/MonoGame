#region License
/*
MIT License
Copyright ? 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class Texture2DReader : ContentTypeReader<Texture2D>
    {
		internal Texture2DReader()
		{
			// Do nothing
		}

#if ANDROID
        static string[] supportedExtensions = new string[] { ".jpg", ".bmp", ".jpeg", ".png", ".gif" };
#else
        static string[] supportedExtensions = new string[] { ".jpg", ".bmp", ".jpeg", ".png", ".gif", ".pict", ".tga" };
#endif

        internal static string Normalize(string fileName)
        {
            return Normalize(fileName, supportedExtensions);
        }

        protected internal override Texture2D Read(ContentReader reader, Texture2D existingInstance)
		{
			Texture2D texture = null;

            var surfaceFormat = (SurfaceFormat)reader.ReadInt32();
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            int levelCount = reader.ReadInt32();
            int levelCountOutput = levelCount;

            // If the system does not fully support Power of Two textures,
            // skip any mip maps supplied with any non PoT textures.
            if (levelCount > 1 && !reader.GraphicsDevice.GraphicsCapabilities.SupportsNonPowerOfTwo &&
                (!MathHelper.IsPowerOfTwo(width) || !MathHelper.IsPowerOfTwo(height)))
            {
                levelCountOutput = 1;
                System.Diagnostics.Debug.WriteLine(
                    "Device does not support non Power of Two textures. Skipping mipmaps.");
            }

			SurfaceFormat convertedFormat = surfaceFormat;
			switch (surfaceFormat)
			{
#if !IOS && !ANDROID
				case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1a:
                    if (!reader.GraphicsDevice.GraphicsCapabilities.SupportsDxt1)
                        convertedFormat = SurfaceFormat.Color;
                    break;
				case SurfaceFormat.Dxt3:
				case SurfaceFormat.Dxt5:
                    if (!reader.GraphicsDevice.GraphicsCapabilities.SupportsS3tc)
					    convertedFormat = SurfaceFormat.Color;
					break;
#endif
				case SurfaceFormat.NormalizedByte4:
					convertedFormat = SurfaceFormat.Color;
					break;
			}
			
            texture = existingInstance ?? new Texture2D(reader.GraphicsDevice, width, height, levelCountOutput > 1, convertedFormat);
			
			for (int level = 0; level < levelCount; level++)
			{
				var levelDataSizeInBytes = reader.ReadInt32();
				var levelData = reader.ReadBytes(levelDataSizeInBytes);
                var levelWidth = width >> level;
                var levelHeight = height >> level;

                if (level >= levelCountOutput)
                    continue;

				//Convert the image data if required
				switch (surfaceFormat)
				{
					case SurfaceFormat.Dxt1:
                    case SurfaceFormat.Dxt1a:
                        if (!reader.GraphicsDevice.GraphicsCapabilities.SupportsDxt1 && convertedFormat == SurfaceFormat.Color)
						    levelData = DxtUtil.DecompressDxt1(levelData, levelWidth, levelHeight);
						break;
					case SurfaceFormat.Dxt3:
                        if (!reader.GraphicsDevice.GraphicsCapabilities.SupportsS3tc && convertedFormat == SurfaceFormat.Color)
						    levelData = DxtUtil.DecompressDxt3(levelData, levelWidth, levelHeight);
						break;
					case SurfaceFormat.Dxt5:
                        if (!reader.GraphicsDevice.GraphicsCapabilities.SupportsS3tc && convertedFormat == SurfaceFormat.Color)
    						levelData = DxtUtil.DecompressDxt5(levelData, levelWidth, levelHeight);
						break;
                    case SurfaceFormat.Bgra5551:
                        {
#if OPENGL
                            // Shift the channels to suit OpenGL
                            int offset = 0;
                            for (int y = 0; y < levelHeight; y++)
                            {
                                for (int x = 0; x < levelWidth; x++)
                                {
                                    ushort pixel = BitConverter.ToUInt16(levelData, offset);
                                    pixel = (ushort)(((pixel & 0x7FFF) << 1) | ((pixel & 0x8000) >> 15));
                                    levelData[offset] = (byte)(pixel);
                                    levelData[offset + 1] = (byte)(pixel >> 8);
                                    offset += 2;
                                }
                            }
#endif
                        }
                        break;
					case SurfaceFormat.Bgra4444:
						{
#if OPENGL
                            // Shift the channels to suit OpenGL
							int offset = 0;
							for (int y = 0; y < levelHeight; y++)
							{
								for (int x = 0; x < levelWidth; x++)
								{
									ushort pixel = BitConverter.ToUInt16(levelData, offset);
									pixel = (ushort)(((pixel & 0x0FFF) << 4) | ((pixel & 0xF000) >> 12));
									levelData[offset] = (byte)(pixel);
									levelData[offset + 1] = (byte)(pixel >> 8);
									offset += 2;
								}
							}
#endif
						}
						break;
					case SurfaceFormat.NormalizedByte4:
						{
							int bytesPerPixel = surfaceFormat.GetSize();
							int pitch = levelWidth * bytesPerPixel;
							for (int y = 0; y < levelHeight; y++)
							{
								for (int x = 0; x < levelWidth; x++)
								{
									int color = BitConverter.ToInt32(levelData, y * pitch + x * bytesPerPixel);
									levelData[y * pitch + x * 4] = (byte)(((color >> 16) & 0xff)); //R:=W
									levelData[y * pitch + x * 4 + 1] = (byte)(((color >> 8) & 0xff)); //G:=V
									levelData[y * pitch + x * 4 + 2] = (byte)(((color) & 0xff)); //B:=U
									levelData[y * pitch + x * 4 + 3] = (byte)(((color >> 24) & 0xff)); //A:=Q
								}
							}
						}
						break;
				}
				
				texture.SetData(level, null, levelData, 0, levelData.Length);	
			}
			
			return texture;
		}
    }
}
