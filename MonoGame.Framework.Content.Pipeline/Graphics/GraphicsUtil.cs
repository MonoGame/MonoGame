// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public static class GraphicsUtil
    {
        public static byte[] ConvertBitmap(Bitmap bmp)
        {
            var bitmapData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                                    ImageLockMode.ReadOnly,
                                    bmp.PixelFormat);

            var length = bitmapData.Stride * bitmapData.Height;
            var output = new byte[length];

            // Copy bitmap to byte[]
            Marshal.Copy(bitmapData.Scan0, output, 0, length);
            bmp.UnlockBits(bitmapData);

            // NOTE: According to http://msdn.microsoft.com/en-us/library/dd183449%28VS.85%29.aspx
            // and http://stackoverflow.com/questions/8104461/pixelformat-format32bppargb-seems-to-have-wrong-byte-order
            // Image data from any GDI based function are stored in memory as BGRA/BGR, even if the format says RBGA.

            switch (bitmapData.PixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    BGRAtoRGBA(output);
                    break;

                case PixelFormat.Format32bppRgb:
                    BGRtoRGBA(output);
                    break;
                    
                default:
                    throw new NotSupportedException("Unsupported pixel format.");
            }
            return output;
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

        public static void BGRtoRGBA(byte[] data)
        {
            var output = new byte[(int)(data.LongLength * 4.0f / 3.0f)];
            var counter = 0;
            for (var x = 0; x < data.Length;)
            {
                output[counter++] = data[x + 2];
                output[counter++] = data[x + 1];
                output[counter++] = data[x];
                output[counter++] = (byte)255;

                x += 3;
            }
        }


        public static void PremultiplyAlpha(TextureContent content)
        {
            var colorTex = content.Faces[0][0] as PixelBitmapContent<Color>;
            if (colorTex != null)
            {
                for (var y = 0; y < colorTex.Height; y++)
                {
                    for (var x = 0; x < colorTex.Width; x++)
                    {
                        colorTex._pixelData[y][x] = Color.FromNonPremultiplied(colorTex._pixelData[y][x].R,
                                                                            colorTex._pixelData[y][x].G,
                                                                            colorTex._pixelData[y][x].B,
                                                                            colorTex._pixelData[y][x].A);
                    }

                }
            }
            else
            {
                /*var vec4Tex = content.Faces[0][0] as PixelBitmapContent<Vector4>;
                if (vec4Tex == null)
                    throw new NotSupportedException();

                for (int x = 0; x < vec4Tex.Height; x++)
                {
                    var row = vec4Tex.GetRow(x);
                    for (int y = 0; y < row.Length; y++)
                    {
                        if (row[y].W < 1.0f)
                        {
                            row[y].X *= row[y].W;
                            row[y].Y *= row[y].W;
                            row[y].Z *= row[y].W;
                        }
                    }
                }*/
            }
        }
    }
}
