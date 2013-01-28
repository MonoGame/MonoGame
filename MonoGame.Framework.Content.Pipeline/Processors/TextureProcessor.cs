// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName="Texture - MonoGame")]
    public class TextureProcessor : ContentProcessor<TextureContent, TextureContent>
    {
        public TextureProcessor() { }

        public virtual Color ColorKeyColor { get; set; }

        public virtual bool ColorKeyEnabled { get; set; }

        public virtual bool GenerateMipmaps { get; set; }

        [DefaultValueAttribute(true)]
        public virtual bool PremultiplyAlpha { get; set; }

        public virtual bool ResizeToPowerOfTwo { get; set; }

        public virtual TextureProcessorOutputFormat TextureFormat { get; set; }

        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            if (ColorKeyEnabled)
            {
                ColorKeyColor = Color.Red;
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

                input.Faces[0][0].SetPixelData(GraphicsUtil.ConvertBitmap(input._bitmap));
            }

            var face = input.Faces[0][0];
            if (ResizeToPowerOfTwo)
            {
                if (!GraphicsUtil.IsPowerOfTwo(face.Width) || !GraphicsUtil.IsPowerOfTwo(face.Height))
                    input.Resize(GraphicsUtil.GetNextPowerOfTwo(face.Width), GraphicsUtil.GetNextPowerOfTwo(face.Height));
            }

            if (PremultiplyAlpha)
                GraphicsUtil.PremultiplyAlpha(input);

            if (GenerateMipmaps)
                throw new NotImplementedException();

            if (TextureFormat == TextureProcessorOutputFormat.NoChange)
                return input;

            if (TextureFormat != TextureProcessorOutputFormat.Color)
                throw new NotImplementedException();

            return input;
        }


    }
}
