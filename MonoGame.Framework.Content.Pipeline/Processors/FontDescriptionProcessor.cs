// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.ComponentModel;
using Microsoft.Win32;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MonoGame.Framework.Utilities;
using Glyph = Microsoft.Xna.Framework.Content.Pipeline.Graphics.Glyph;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Sprite Font Description - MonoGame")]
    public class FontDescriptionProcessor : ContentProcessor<FontDescription, SpriteFontContent>
    {
        [DefaultValue(true)] public virtual bool PremultiplyAlpha { get; set; } = true;

        [DefaultValue(typeof(TextureProcessorOutputFormat), "Compressed")]
        public virtual TextureProcessorOutputFormat TextureFormat { get; set; } = TextureProcessorOutputFormat.Compressed;

        public override SpriteFontContent Process(FontDescription input, ContentProcessorContext context)
        {
            var output = new SpriteFontContent(input);
            var fontFile = FindFont(input.FontName, input.Style.ToString());

            // Look for fonts by filename
            if (string.IsNullOrWhiteSpace(fontFile))
            {
                var directories = new List<string>();
                if (input.Identity != null &&
                    Path.GetDirectoryName(input.Identity.SourceFilename) is string spath &&
                    !string.IsNullOrWhiteSpace(spath))
                {
                    directories.Add(spath);
                }
                var extensions = new string[] { "", ".ttf", ".ttc", ".otf" };

                switch (CurrentPlatform.OS)
                {
                    // Add special per platform directories
                    case OS.Windows:
                        directories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts)));
                        break;
                    case OS.MacOSX:
                        directories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Fonts"));
                        directories.Add("/Library/Fonts");
                        directories.Add("/System/Library/Fonts/Supplemental");
                        break;
                }

                foreach (var dir in directories)
                {
                    foreach (var ext in extensions)
                    {
                        fontFile = Path.Combine(dir, input.FontName + ext);
                        if (File.Exists(fontFile))
                            break;
                    }
                    if (File.Exists(fontFile))
                        break;
                }
            }

            if (!File.Exists(fontFile))
                throw new FileNotFoundException("Could not find \"" + input.FontName + "\" font file at \"" + fontFile + "\".");

            context.Logger.Log(LogLevel.Info, $"Building Font {fontFile}");

            // Get the platform specific texture profile.
            var texProfile = TextureProfile.ForPlatform(context.TargetPlatform);

            {
                if (!File.Exists(fontFile))
                {
                    throw new Exception($"Could not load {fontFile}");
                }
                var glyphs = ImportFont(input, out float lineSpacing, out var yOffsetMin, fontFile);
                var glyphData = new HashSet<GlyphData>(glyphs.Select(x => x.Data));

                // Optimize.
                foreach (GlyphData glyph in glyphData)
                {
                    GlyphCropper.Crop(glyph);
                }

                // We need to know how to pack the glyphs.
                texProfile.Requirements(context, TextureFormat, out var requiresPot, out var requiresSquare);

                var face = GlyphPacker.ArrangeGlyphs([.. glyphData], requiresPot, requiresSquare);

                // Adjust line and character spacing.
                lineSpacing += input.Spacing;
                output.VerticalLineSpacing = (int)lineSpacing;

                foreach (Glyph glyph in glyphs)
                {
                    output.CharacterMap.Add(glyph.Character);

                    var texRect = glyph.Data.SubRect;
                    output.Glyphs.Add(texRect);

                    var cropping = new Rectangle(0, (int)(glyph.Data.YOffset - yOffsetMin), (int)glyph.Data.XAdvance, output.VerticalLineSpacing);
                    output.Cropping.Add(cropping);

                    // Set the optional character kerning.
                    if (input.UseKerning)
                    {
                        ABCFloat widths = glyph.Data.CharacterWidths;
                        output.Kerning.Add(new Vector3(widths.A, widths.B, widths.C));
                    }
                    else
                    {
                        float width = texRect.Width;

                        // Whitespace glyphs can carry advance entirely in their metrics.
                        // Preserve that advance when kerning is disabled so spacing matches XNA.
                        if (glyph.Data.CharacterWidths.B <= 0 && glyph.Data.XAdvance > width)
                        {
                            width = glyph.Data.XAdvance;
                        }

                        output.Kerning.Add(new Vector3(0, width, 0));
                    }
                }

                output.Texture.Faces[0].Add(face);
            }

            if (PremultiplyAlpha)
            {
                var bmp = output.Texture.Faces[0][0];
                var data = bmp.GetPixelData();
                var idx = 0;
                for (; idx < data.Length;)
                {
                    var r = data[idx];

                    // Special case of simply copying the R component into the A, since R is the value of white alpha we want
                    data[idx + 0] = r;
                    data[idx + 1] = r;
                    data[idx + 2] = r;
                    data[idx + 3] = r;

                    idx += 4;
                }

                bmp.SetPixelData(data);
            }
            else
            {
                var bmp = output.Texture.Faces[0][0];
                var data = bmp.GetPixelData();
                var idx = 0;
                for (; idx < data.Length;)
                {
                    var r = data[idx];

                    // Special case of simply moving the R component into the A and setting RGB to solid white, since R is the value of white alpha we want
                    data[idx + 0] = 255;
                    data[idx + 1] = 255;
                    data[idx + 2] = 255;
                    data[idx + 3] = r;

                    idx += 4;
                }

                bmp.SetPixelData(data);
            }

            // Perform the final texture conversion.
            texProfile.ConvertTexture(context, output.Texture, TextureFormat, true);

            return output;
        }

        private static Glyph[] ImportFont(FontDescription options, out float lineSpacing, out long yOffsetMin, string fontName)
        {
            // Which importer knows how to read this source font?

            var trueTypeFileExtensions = new List<string> { ".ttf", ".ttc", ".otf" };
            //var BitmapFileExtensions = new List<string> { ".bmp", ".png", ".gif" };

            var fileExtension = Path.GetExtension(fontName).ToLowerInvariant();

            //			if (BitmapFileExtensions.Contains(fileExtension))
            //			{
            //				importer = new BitmapImporter();
            //			}
            //			else
            //			{
            if (!trueTypeFileExtensions.Contains(fileExtension))
                throw new PipelineException("Unknown file extension " + fileExtension);

            var importer = new SharpFontImporter();

            // Import the source font data.
            importer.Import(options, fontName);

            lineSpacing = importer.LineSpacing;
            yOffsetMin = importer.YOffsetMin;

            // Get all glyphs
            var glyphs = new List<Glyph>(importer.Glyphs);

            // Validate.
            if (glyphs.Count == 0)
            {
                throw new Exception("Font does not contain any glyphs.");
            }

            // Sort the glyphs
            glyphs.Sort((left, right) => left.Character.CompareTo(right.Character));


            // Check that the default character is part of the glyphs
            if (options.DefaultCharacter != null)
            {
                bool defaultCharacterFound = false;
                foreach (var glyph in glyphs)
                {
                    if (glyph.Character == options.DefaultCharacter)
                    {
                        defaultCharacterFound = true;
                        break;
                    }
                }
                if (!defaultCharacterFound)
                {
                    throw new InvalidOperationException("The specified DefaultCharacter is not part of this font.");
                }
            }

            return glyphs.ToArray();
        }

        private static string? FindFont(string name, string style)
        {
            if (CurrentPlatform.OS == OS.Windows)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                var fontDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
                foreach (var key in new[] { Registry.LocalMachine, Registry.CurrentUser })
                {
                    var subkey = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", false);
                    if (subkey == null)
                    {
                        return null;
                    }

                    foreach (var font in subkey.GetValueNames().OrderBy(x => x))
                    {
                        if (font.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                        {
                            var fontPath = subkey.GetValue(font)?.ToString();
                            if (string.IsNullOrWhiteSpace(fontPath))
                            {
                                return null;
                            }

                            // The registry value might have trailing NUL characters
                            // See https://github.com/MonoGame/MonoGame/issues/4061
                            var nulIndex = fontPath.IndexOf('\0');
                            if (nulIndex != -1)
                                fontPath = fontPath.Substring(0, nulIndex);

                            fontPath = Path.IsPathRooted(fontPath) ? fontPath : Path.Combine(fontDirectory, fontPath);
                            if (MatchFont(fontPath, name, style))
                            {
                                return fontPath;
                            }
                        }
                    }
                }
            }
            else if (CurrentPlatform.OS == OS.Linux)
            {
                ExternalTool.Run("/bin/bash", $"-c \"fc-match -f '%{{file}}:%{{family}}\\n' '{name}:style={style}'\"", out var s, out var e);
                s = s.Trim();

                var split = s.Split(':');
                if (split.Length < 2)
                    return null;

                // check font family, fontconfig might return a fallback
                if (split[1].Contains(','))
                {
                    // this file defines multiple family names
                    var families = split[1].Split(',');
                    foreach (var f in families)
                    {
                        if (f.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                            return split[0];
                    }
                    // didn't find it
                    return null;
                }
                else
                {
                    if (!split[1].Equals(name, StringComparison.InvariantCultureIgnoreCase))
                        return null;
                }

                return split[0];
            }

            return null;
        }

         private static bool MatchFont(string fontPath, string fontName, string fontStyle)
         {
            // TODO: Implement this with FreeType lib
            /*try
            {
                var font = fontPath.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase)
                    ? TrueTypeFont.FromCollectionFile(fontPath)[0]
                    : TrueTypeFont.FromFile(fontPath);

                var usCulture = CultureInfo.GetCultureInfo("en-US");
                var family = NameHelper.GetName(NameId.FontFamilyName, usCulture, font);
                var subfamily = NameHelper.GetName(NameId.FontSubfamilyName, usCulture, font);
                return family == fontName && subfamily == fontStyle;
            }
            catch (Exception)
            {
                // Let's not crash when a font cannot be parsed
                return false;
            }*/

            return true;
         }
    }
}
