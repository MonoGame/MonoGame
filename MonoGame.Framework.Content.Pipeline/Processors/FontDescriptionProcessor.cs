// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Linq;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Sprite Font Description - MonoGame")]
    public class FontDescriptionProcessor : ContentProcessor<FontDescription, SpriteFontContent>
    {
        public override SpriteFontContent Process(FontDescription input,
            ContentProcessorContext context)
        {
            var output = new SpriteFontContent(input);

            var font = new Font(input.FontName, input.Size);

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
                foreach (var ch in input.Characters)
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
            var texBounds = calculateOutputTextureBounds(estimatedSurfaceArea, true);

            // Create our texture
            var outputBitmap = new Bitmap(texBounds.X, texBounds.Y);
            using (var g = System.Drawing.Graphics.FromImage(outputBitmap))
            {
                g.FillRectangle(Brushes.Magenta, new System.Drawing.Rectangle(0, 0, outputBitmap.Width, outputBitmap.Height));

                int x = 0;
                int y = 0;
                // Draw each glyph into the image.

                for (int i = 0; i < input.Characters.Count; i++)
                {
                    var glyphWidth = widthsAndHeights[i].X;
                    
                    if (x + glyphWidth >= texBounds.X)
                    {
                        x = 0;
                        y += largestHeight;
                    }

                    output.Glyphs.Add(new Microsoft.Xna.Framework.Rectangle(x, y, widthsAndHeights[i].X, widthsAndHeights[i].Y));
                    g.DrawString(input.Characters[i].ToString(), font, Brushes.White, new PointF(x, y));

                    x += glyphWidth;
                }

                //outputBitmap.Save(@"C:\Projects\AdRotator\AdRotatorSharedModel\Model\Test.bmp", System.Drawing.Imaging.ImageFormat.Png);

                var bitmapContent = new PixelBitmapContent<Color>(texBounds.X, texBounds.Y);
                bitmapContent.SetPixelData(outputBitmap.GetData());
                output.Texture.Faces.Add(new MipmapChain(bitmapContent));
            }

            return output;
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
                dimention = GraphicsUtil.GetNextPowerOfTwo(dimention);

                // return the height and width of our new
                return new Point(dimention, dimention);
            }
            else
                throw new NotSupportedException("Non Square Textures are not yet supported");

            /*// We don't require a square texture. Get the smallest PoT bounds
            // that can contain the entire sheet.

            return new Point();*/
        }
    }
}
