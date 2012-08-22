using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.ComponentModel;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName="Texture Processor")]
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
            if (PremultiplyAlpha)
                GraphicsUtil.PremultiplyAlpha(input);

            if (ColorKeyEnabled)
                throw new NotImplementedException();

            if (GenerateMipmaps)
                throw new NotImplementedException();

            if (ResizeToPowerOfTwo)
                throw new NotImplementedException();

            if (TextureFormat == TextureProcessorOutputFormat.NoChange)
                return input;

            if (TextureFormat != TextureProcessorOutputFormat.Color)
                throw new NotImplementedException();

            return input;
        }


    }
}
