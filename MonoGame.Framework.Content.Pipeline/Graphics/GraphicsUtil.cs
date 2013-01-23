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

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public static class GraphicsUtil
    {
        public static void ConvertBitmap(Bitmap bmp, out byte[] output)
        {
            var bitmapData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                                    ImageLockMode.ReadOnly,
                                    bmp.PixelFormat);

            var length = bitmapData.Stride * bitmapData.Height;

            if (bitmapData.PixelFormat != PixelFormat.Format32bppArgb)
                throw new NotSupportedException("Unsupported pixel format.");

            output = new byte[length];

            // Copy bitmap to byte[]
            // NOTE: According to http://msdn.microsoft.com/en-us/library/dd183449%28VS.85%29.aspx
            // and http://stackoverflow.com/questions/8104461/pixelformat-format32bppargb-seems-to-have-wrong-byte-order
            // Image data from any GDI based function will always come in BGR/BGRA even if the format comes in as RGBA
            Marshal.Copy(bitmapData.Scan0, output, 0, length);
            bmp.UnlockBits(bitmapData);
        }


        public static void PremultiplyAlpha(TextureContent content)
        {
            var colorTex = content.Faces[0][0] as PixelBitmapContent<Color>;
            if (colorTex != null)
            {
                for (int x = 0; x < colorTex.Height; x++)
                {
                    var row = colorTex.GetRow(x);
                    for (int y = 0; y < row.Length; y++)
                    {
                        if (row[y].A < 0xff)
                            row[y] = Color.FromNonPremultiplied(row[y].R, row[y].G, row[y].B, row[y].A);
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
