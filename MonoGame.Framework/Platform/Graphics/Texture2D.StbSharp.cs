// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using MonoGame.Framework.Graphics;
using StbImageWriteSharp;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D
    {
        private unsafe static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            // Rewind stream if it is at end
            if (stream.CanSeek && stream.Length == stream.Position)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            
            using (Image2D image = Image2D.FromStream(stream))
            {
                // XNA blacks out any pixels with an alpha of zero.
                fixed (byte* b = image.PixelData)
                {
                    int length = image.PixelData.Length;
                    for (int i = 0; i < length; i += 4)
                    {
                        if (b[i + 3] == 0)
                        {
                            b[i + 0] = 0;
                            b[i + 1] = 0;
                            b[i + 2] = 0;
                        }
                    }
                }

                Texture2D texture = new Texture2D(graphicsDevice, image.Width, image.Height);
                texture.SetData(image.PixelData);

                return texture;
            }
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
            SaveAsImage(stream, 0, 0, new Rectangle(0, 0, width, height), ImageWriterFormat.Jpg);
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
            SaveAsImage(stream, 0, 0, new Rectangle(0, 0, width, height), ImageWriterFormat.Png);
        }

        private enum ImageWriterFormat
        {
            Jpg,
            Png
        }

        private unsafe void SaveAsImage(Stream stream, int level, int arraySlice, Rectangle? rectangle, ImageWriterFormat format)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "'stream' may not be null (Nothing in Visual Basic)");

            int tSize;
            int dataByteSize;
            Rectangle checkedRect;
            ValidateParams<Color>(level, arraySlice, rectangle, out tSize, out dataByteSize, out checkedRect);

            Color[] data = new Color[checkedRect.Width * checkedRect.Height];
            GetColorData(level, arraySlice, data, checkedRect);

            ColorComponents colorComp = ColorComponents.RedGreenBlueAlpha;

            // Write
            fixed (Color* ptr = data)
            {
                var writer = new ImageWriter();
                switch (format)
                {
                    case ImageWriterFormat.Jpg:
                        writer.WriteJpg(ptr, checkedRect.Width, checkedRect.Height, colorComp, stream, 90);
                        break;

                    case ImageWriterFormat.Png:
                        writer.WritePng(ptr, checkedRect.Height, checkedRect.Height, colorComp, stream);
                        break;
                }
            }
        }
    }
}
