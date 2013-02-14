// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{

    public static class ByteColorConverter
    {
        /*public static void GetPixel<T>(this byte[] data, int startIndex, SurfaceFormat format, out T result) where T : struct
        {
            result = new Color();
        }*/

        public static void GetPixel(this byte[] data, int startIndex, SurfaceFormat format, out Color result)
        {
            result = new Color();
        }

        public static void SetPixelData<T>(this byte[] data, PixelBitmapContent<T> bmpContent, int startIndex, SurfaceFormat format) where T : struct, IEquatable<T>
        {
        }

        public static void SetPixelData(this byte[] data, PixelBitmapContent<Color> bmpContent, SurfaceFormat format)
        {
            var formatSize = format.Size();

            for (int y = 0; y < bmpContent.Height; y++)
            {
                for (int x = 0; x < bmpContent.Width; x++)
                {
                    switch(format)
                    {
                        case SurfaceFormat.Vector4:

                            var startIdx = (y * formatSize) + (x * formatSize);
                            var vec4 = new Vector4( BitConverter.ToSingle(data, startIdx),
                                                    BitConverter.ToSingle(data, startIdx + 4),
                                                    BitConverter.ToSingle(data, startIdx + 8),
                                                    BitConverter.ToSingle(data, startIdx + 12) );

                            bmpContent._pixelData[y][x] = new Color(vec4);
                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }

    public static class GraphicsUtil
    {

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
                var vec4Tex = content.Faces[0][0] as PixelBitmapContent<Vector4>;
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
                }
            }
        }
    }
}
