// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline;

/// <summary>
/// Provides methods for reading font files for use in the Content Pipeline.
/// </summary>
[ContentImporter(".ttf", ".otf", DisplayName = "Sprite Font Importer - MonoGame", DefaultProcessor = "FontDescriptionProcessor")]
public class FontImporter : ContentImporter<FontDescription>
{
    /// <summary>
    /// Gets or sets the default character for the font.
    /// </summary>
    public char? DefaultCharacter { get; set; }

    /// <summary>
    /// Gets or sets the size, in points, of the font.
    /// </summary>
    public float Size { get; set; } = 12;

    /// <summary>
    /// Gets or sets the amount of space, in pixels, to insert between letters in a string.
    /// </summary>
    public float Spacing { get; set; }

    /// <summary>
    /// Gets or sets the style of the font, expressed as a combination of one or more FontDescriptionStyle flags.
    /// </summary>
    public FontDescriptionStyle Style { get; set; } = FontDescriptionStyle.Regular;

    /// <summary>
    /// Indicates if kerning information is used when drawing characters.
    /// </summary>
    public bool UseKerning { get; set; }

    /// <summary>
    /// Retrieves the set of characters to include in the processor output.
    /// </summary>
    public List<CharacterRegion> CharacterRegions { get; set; } = [new CharacterRegion((char)32, (char)126)];

    /// <summary>
    /// Allows including extra characters as specified by the resource files.
    /// </summary>
    public List<string> ResourceFiles { get; set; } = [];

    /// <summary>
    /// Called by the XNA Framework when importing a font file to be used as a game asset. This is the method called by the XNA Framework when an asset is to be imported into an object that can be recognized by the Content Pipeline.
    /// </summary>
    /// <param name="filename">Name of a game asset file.</param>
    /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
    /// <returns>Resulting game asset.</returns>
    public override FontDescription Import(string filename, ContentImporterContext context)
    {
        var characters = new HashSet<char>();

        foreach (var characterRegion in CharacterRegions)
        {
            if (characterRegion.End < characterRegion.Start)
                throw new ArgumentException("CharacterRegion.End must be greater than CharacterRegion.Start");

            for (var start = characterRegion.Start; start <= characterRegion.End; start++)
                characters.Add(start);
        }

        var fontDir = Path.GetDirectoryName(filename) ?? "";
        foreach (var resourceFile in ResourceFiles)
        {
            var absolutePath = Path.Combine(Path.Combine(fontDir, resourceFile.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar)));
            if (!File.Exists(absolutePath))
            {
                throw new InvalidContentException("Can't find " + absolutePath);
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(absolutePath);

            // Scan each string from the .resx file.
            var modeList = xmlDocument.SelectNodes("root/data/value");
            if (modeList != null)
            {
                foreach (XmlNode xmlNode in modeList)
                {
                    foreach (char usedCharacter in xmlNode.InnerText)
                        characters.Add(usedCharacter);
                }
            }

            // Mark that this font should be rebuilt if the resource file changes.
            context.AddDependency(absolutePath);
        }

        return new()
        {
            DefaultCharacter = DefaultCharacter,
            FontName = filename,
            Size = Size,
            Spacing = Spacing,
            Style = Style,
            UseKerning = UseKerning,
            Characters = characters,
            Identity = new ContentIdentity(new FileInfo(filename).FullName, "FontImporter")
        };
    }
}
