// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using StbImageSharp;
using StbImageWriteSharp;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D
    {
        private unsafe static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream, Action<byte[]> colorProcessor)
        {
            // Rewind stream if it is at end
            if (stream.CanSeek && stream.Length == stream.Position)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            ImageResult result;
            if (stream.CanSeek)
            {
                result = ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
            }
            else
            {
                // If stream doesn't provide seek functionality, use MemoryStream instead
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    result = ImageResult.FromStream(ms, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                }
            }

            if (colorProcessor != null)
            {
                colorProcessor(result.Data);
            }

            Texture2D texture = null;
            texture = new Texture2D(graphicsDevice, result.Width, result.Height);
            texture.SetData(result.Data);

            return texture;
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
            SaveAsImage(stream, width, height, ImageWriterFormat.Jpg);
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
            SaveAsImage(stream, width, height, ImageWriterFormat.Png);
        }

        private enum ImageWriterFormat
        {
            Jpg,
            Png
        }

        private unsafe void SaveAsImage(Stream stream, int width, int height, ImageWriterFormat format)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "'stream' cannot be null (Nothing in Visual Basic)");
            }
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException("width", width, "'width' cannot be less than or equal to zero");
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("height", height, "'height' cannot be less than or equal to zero");
            }
            Color[] data = null;
            try
            {
                data = GetColorData();

                // Write
                fixed (Color* ptr = &data[0])
                {
                    var writer = new ImageWriter();
                    switch (format)
                    {
                        case ImageWriterFormat.Jpg:
                            writer.WriteJpg(ptr, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream, 90);
                            break;
                        case ImageWriterFormat.Png:
                            writer.WritePng(ptr, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
                            break;
                    }
                }
            }
            finally
            {
                if (data != null)
                {
                    data = null;
                }
            }
        }
    }
}
