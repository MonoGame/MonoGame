// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName="Texture - MonoGame")]
    public class SpriteFontDescriptionProcessor : ContentProcessor<SpriteFontDescriptionContent, SpriteFontDescriptionContent>
    {
        public SpriteFontDescriptionProcessor() { }

        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            if (ColorKeyEnabled)
            {
                var replaceColor = System.Drawing.Color.FromArgb(0);
                for (var x = 0; x < input._bitmap.Width; x++)
                {
                    for (var y = 0; y < input._bitmap.Height; y++)
                    {
                        var col = input._bitmap.GetPixel(x, y);

                        if (col.ColorsEqual(ColorKeyColor))
                        {
                            input._bitmap.SetPixel(x, y, replaceColor);
                        }
                    }
                }
            }

            var face = input.Faces[0][0];
            if (ResizeToPowerOfTwo)
            {
                if (!GraphicsUtil.IsPowerOfTwo(face.Width) || !GraphicsUtil.IsPowerOfTwo(face.Height))
                    input.Resize(GraphicsUtil.GetNextPowerOfTwo(face.Width), GraphicsUtil.GetNextPowerOfTwo(face.Height));
            }

            if (PremultiplyAlpha)
            {
                for (var x = 0; x < input._bitmap.Width; x++)
                {
                    for (var y = 0; y < input._bitmap.Height; y++)
                    {
                        var oldCol = input._bitmap.GetPixel(x, y);
                        var preMultipliedColor = Color.FromNonPremultiplied(oldCol.R, oldCol.G, oldCol.B, oldCol.A);
                        input._bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(preMultipliedColor.A, 
                                                                                   preMultipliedColor.R,
                                                                                   preMultipliedColor.G,
                                                                                   preMultipliedColor.B));
                    }
                }
            }

            if (GenerateMipmaps)
                throw new NotImplementedException();

            // TODO: Set all mip level data
            input.Faces[0][0].SetPixelData(input._bitmap.GetData());

            if (TextureFormat == TextureProcessorOutputFormat.NoChange)
                return input;

            if (TextureFormat != TextureProcessorOutputFormat.Color)
                throw new NotImplementedException();

            return input;
        }


    }
}
