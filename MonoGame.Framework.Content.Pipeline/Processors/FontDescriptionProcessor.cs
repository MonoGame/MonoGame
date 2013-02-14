// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Sprite Font Processor - MonoGame")]
    class FontDescriptionProcessor : ContentProcessor<SpriteFontContent, SpriteFontContent>
    {
        public override SpriteFontContent Process(SpriteFontContent input,
            ContentProcessorContext context)
        {
            
            var font = new Font(input.FontName, input.FontSize);

            // Make sure that this font is installed on the system.
            // Creating a font object with a font that's not contained will default to MS Sans Serif:
            // http://msdn.microsoft.com/en-us/library/zet4c3fa.aspx
            if (font.FontFamily.Name == "Microsoft Sans Serif" && input.FontName != "Microsoft Sans Serif")
                throw new Exception(string.Format("Font {0} is not installed on this computer.", input.FontName));

            var estimatedSurfaceArea = 0;
            var largestHeight = 0;
            var widthsAndHeights = new List<Point>();
            // Calculate the bounds of each rect
            var bmp = new Bitmap((int)(font.Size * 1.5), (int)(font.Size * 1.5));
            using (var temp = System.Drawing.Graphics.FromImage(bmp))
            {
                // Calculate and save the size of each character
                foreach (var ch in input.CharacterMap)
                {
                    var charSize = temp.MeasureString(ch.ToString(), font);
                    var width = (int)charSize.Width;
                    var height = (int)charSize.Height;

                    estimatedSurfaceArea += width;
                    largestHeight = Math.Max(largestHeight, height);


                    widthsAndHeights.Add(new Point(width, height));
                }

                // TODO: Using the largest height will give us some empty space
                // This can be optimized to pack a smaller texture if necessary
                estimatedSurfaceArea *= largestHeight;
            }

            // calculate the best height and width for our output texture.
            // TODO: GetMonoGamePlatform()
            var texBounds = calculateOutputTextureBounds(estimatedSurfaceArea, context.BuildConfiguration.ToUpper().Contains("IOS"));

            // Create our texture
            var outputBitmap = new Bitmap(texBounds.X, texBounds.Y);
            using (var g = System.Drawing.Graphics.FromImage(outputBitmap))
            {

                g.FillRectangle(Brushes.Magenta, new System.Drawing.Rectangle(0, 0, outputBitmap.Width, outputBitmap.Height));

                int x = 0;
                int y = 0;
                // Draw each glyph into the image.
                for (int i = 0; i < input.CharacterMap.Count; i++)
                {

                    input.Glyphs.Add(new Microsoft.Xna.Framework.Rectangle(x, y, input.Glyphs[x].Width, input.Glyphs[x].Height));
                    g.DrawString(input.CharacterMap[x].ToString(), font, Brushes.White, new PointF(x, y));

                    x += input.Glyphs[x].Width;

                    if (x >= texBounds.X)
                    {
                        x = 0;
                        y += largestHeight;
                    }
                }

                using (var ms = new MemoryStream())
                {
                    outputBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.MemoryBmp);

                    var texData = new byte[ms.Length];
                    ms.Read(texData, 0, (int)ms.Length);

                    var bitmapContent = (BitmapContent)Activator.CreateInstance(typeof(PixelBitmapContent<Microsoft.Xna.Framework.Color>), 
                                                                                new object[] { outputBitmap.Width, outputBitmap.Height });
                    bitmapContent.SetPixelData(texData);

                    input.Texture.Faces[0].Add(bitmapContent);
                    var tp = new TextureProcessor();
                    tp.Process(input.Texture, context);
                }
            }

            return input;
        }

        private Point calculateOutputTextureBounds(int estArea, bool forceSquare)
        {
            // always generate textures with PoT bounds.

            // Some texture compression requires square PoT textures
            if (forceSquare)
            {
                // get a single dimention of the square
                var dimention = (int)Math.Ceiling(Math.Sqrt(estArea));

                // get thenext power of two of this dimention
                dimention = getPowerOfTwo(dimention);

                // return the height and width of our new
                return new Point(dimention, dimention);
            }
            else
                throw new NotSupportedException("Non Square Textures are not yet supported");

            /*// We don't require a square texture. Get the smallest PoT bounds
            // that can contain the entire sheet.

            return new Point();*/
        }

        private int getPowerOfTwo(int num)
        {
            num--;
            num = (num >> 1) | num;
            num = (num >> 2) | num;
            num = (num >> 4) | num;
            num = (num >> 8) | num;
            num = (num >> 16) | num;
            return ++num;
        }
    }
}
