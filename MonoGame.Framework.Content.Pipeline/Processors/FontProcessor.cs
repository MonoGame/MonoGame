// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Sprite Font - MonoGame")]
    public class FontProcessor : FontDescriptionProcessor
    {
        public virtual CharacterRegionsDescription CharacterRegions { get; set; }

        public virtual char DefaultCharacter { get; set; }

        public virtual float Size { get; set; }

        public virtual float Spacing { get; set; }

        public virtual FontDescriptionStyle Style { get; set; }

        public virtual bool UseKerning { get; set; }

        public FontProcessor() : base()
        {
            Size = 12;
            Spacing = 0;
            UseKerning = true;
            Style = FontDescriptionStyle.Regular;
            DefaultCharacter = ' ';
            CharacterRegions = new CharacterRegionsDescription();
        }

        public override SpriteFontContent Process(FontDescription input, ContentProcessorContext context)
        {
            input.Size = Size;
            input.Spacing = Spacing;
            input.UseKerning = UseKerning;
            input.Style = Style;
            input.DefaultCharacter = DefaultCharacter;
            input.CharacterRegions = CharacterRegions.Array;

            return base.Process(input, context);
        }
    }
}

