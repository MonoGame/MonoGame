// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.ComponentModel;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Custom processor extends the SpriteFont build process to scan over the resource
    /// strings used by the game, automatically adding whatever characters it finds in
    /// them to the font. This makes sure the game will always have all the characters
    /// it needs, no matter what languages it is localized into, while still producing
    /// an efficient font that does not waste space on unnecessary characters. This is
    /// especially useful for languages such as Japanese and Korean, which have
    /// potentially thousands of different characters, although games typically only
    /// use a small fraction of these. Building only the characters we need is far more
    /// efficient than if we tried to include the entire CJK character region.
    /// </summary>
    [Obsolete($"Please use {nameof(FontImporter)} for importing and {nameof(FontDescriptionProcessor)} for processing instead.")]
    [ContentProcessor]
    public class LocalizedFontProcessor : ContentProcessor<LocalizedFontDescription, SpriteFontContent>
    {
        /// <summary>
        /// Gets or Sets the premultiply alpha flag.
        /// </summary>
        [DefaultValue(true)]
        public virtual bool PremultiplyAlpha { get; set; } = true;

        /// <summary>
        /// Gets or Sets the target texture output format.
        /// </summary>
        [DefaultValue(typeof(TextureProcessorOutputFormat), "Compressed")]
        public virtual TextureProcessorOutputFormat TextureFormat { get; set; } = TextureProcessorOutputFormat.Compressed;

        /// <summary>
        /// Converts a font description into SpriteFont format.
        /// </summary>
        public override SpriteFontContent Process(LocalizedFontDescription input, ContentProcessorContext context)
        {
            // Scan each .resx file in turn.
            foreach (var resourceFile in input.ResourceFiles)
            {
                var absolutePath = Path.GetFullPath(resourceFile.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar));

                // Make sure the .resx file really does exist.
                if (!File.Exists(absolutePath))
                {
                    throw new InvalidContentException("Can't find " + absolutePath);
                }

                // Load the .resx data.
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(absolutePath);

                // Scan each string from the .resx file.
                var nodes = xmlDocument.SelectNodes("root/data/value");
                if (nodes != null)
                {
                    foreach (XmlNode xmlNode in nodes)
                    {
                        var resourceString = xmlNode.InnerText;

                        // Scan each character of the string.
                        foreach (var usedCharacter in resourceString)
                        {
                            if (!input.Characters.Contains(usedCharacter))
                                input.Characters.Add(usedCharacter);
                        }
                    }
                }

                // Mark that this font should be rebuilt if the resource file changes.
                context.AddDependency(absolutePath);
            }

            // After adding the necessary characters, we can use the built in
            // FontDescriptionProcessor to do the hard work of building the font for us.
            return context.Convert<FontDescription, SpriteFontContent>(input, new FontDescriptionProcessor
            {
                PremultiplyAlpha = PremultiplyAlpha,
                TextureFormat = TextureFormat
            });
        }
    }
}
