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
                throw new PipelineException(string.Format("Font {0} is not installed on this computer.", input.FontName));

            var estimatedSurfaceArea = 0;
            var largestHeight = 0;
            var widthsAndHeights = new List<Point>();
            
            // Estimate the bounds of each rect to calculate the
            // final texture size
            var sf = StringFormat.GenericTypographic;
            sf.Trimming = StringTrimming.None;
            sf.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
            
            using (var bmp = new Bitmap((int)(font.Size), (int)(font.Size)))
            using (var temp = System.Drawing.Graphics.FromImage(bmp))
            {
                // Calculate and save the size of each character
                foreach (var ch in input.Characters)
                {
                    var charSize = temp.MeasureString(ch.ToString(), font, new PointF(0, 0), sf);
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

            output.VerticalLineSpacing = largestHeight;

            // calculate the best height and width for our output texture.
            // TODO: GetMonoGamePlatform()
            var texBounds = calculateOutputTextureBounds(estimatedSurfaceArea, true);

            // Create our texture
            var outputBitmap = new Bitmap(texBounds.X, texBounds.Y);
            using (var g = System.Drawing.Graphics.FromImage(outputBitmap))
            {
                g.FillRectangle(Brushes.Transparent, new System.Drawing.Rectangle(0, 0, outputBitmap.Width, outputBitmap.Height));

                int x = 0;
                int y = 0;
                // Draw each glyph into the image.
                for (int i = 0; i < input.Characters.Count; i++)
                {
                    var kernData = FontHelper.GetCharWidthABC(input.Characters[i], font, g);
                    int charWidth = (int)(Math.Abs(kernData.abcA) + kernData.abcB + kernData.abcC);

                    if (!input.UseKerning)
                        charWidth = (int)kernData.abcB;

                    if (x + charWidth >= texBounds.X)
                    {
                        x = 0;
                        y += largestHeight;
                    }

                    Rectangle rect = new Microsoft.Xna.Framework.Rectangle(x, y, charWidth, widthsAndHeights[i].Y);
                    output.Glyphs.Add(rect);

                    // Characters with a negative a kerning value (like j) need to be adjusted,
                    // so (in the case of j) the bottom curve doesn't render outside our source
                    // rect.
                    var renderPoint = new PointF(x, y);
                    if (kernData.abcA < 0)
                        renderPoint.X += Math.Abs(kernData.abcA);

                    g.DrawString(input.Characters[i].ToString(), font, Brushes.White, renderPoint, sf);
                    output.Cropping.Add(new Rectangle(0, 0, charWidth, output.Glyphs[i].Height));

                    if (!input.UseKerning)
                    {
                        kernData.abcA = 0;
                        kernData.abcC = 0;

                    }
                    output.Kerning.Add(new Vector3(kernData.abcA, kernData.abcB, kernData.abcC));

                    // Add a 2 pixel spacing between characters
                    x += charWidth + 2;
                }

                // Drawing against a transparent black background blends
                // the 'alpha' pixels against black, leaving a black outline.
                // Interpolate between black and white
                // based on it's intensity to covert this 'outline' to
                // it's grayscale equivalent.
                var transBlack = Color.TransparentBlack;
                for (var i = 0; i < outputBitmap.Width; i++)
                {
                    for (var j = 0; j < outputBitmap.Height; j++)
                    {
                        var px = outputBitmap.GetPixel(i, j);

                        if (px.ColorsEqual(transBlack))
                            continue;

                        var val = (px.R + px.B + px.G) / (255.0f * 3.0f);
                        var col = Color.Lerp(Color.Transparent, Color.White, val);
                        px = System.Drawing.Color.FromArgb(col.A, col.R, col.G, col.B);
                        outputBitmap.SetPixel(i, j, px);
                    }
                }

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
