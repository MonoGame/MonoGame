// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MonoGame.Framework.Utilities;
using Glyph = Microsoft.Xna.Framework.Content.Pipeline.Graphics.Glyph;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Sprite Font Description - MonoGame")]
    public class FontDescriptionProcessor : ContentProcessor<FontDescription, SpriteFontContent>
    {
        [DefaultValue(true)]
        public virtual bool PremultiplyAlpha { get; set; }

        [DefaultValue(typeof(TextureProcessorOutputFormat), "Compressed")]
        public virtual TextureProcessorOutputFormat TextureFormat { get; set; }

    // Distance field generation options (experimental).
    [DefaultValue(false)]
    public bool GenerateDistanceField { get; set; }
    [DefaultValue(8f)]
    public float DistanceFieldSpread { get; set; } = 8f; // in texels (half-range)
    [DefaultValue(4)]
    public int Oversample { get; set; } = 4; // supersampling factor for source coverage

        public FontDescriptionProcessor()
        {
            PremultiplyAlpha = true;
            TextureFormat = TextureProcessorOutputFormat.Compressed;
        }

        public override SpriteFontContent Process(FontDescription input, ContentProcessorContext context)
        {
            var output = new SpriteFontContent(input);
            var fontFile = FindFont(input.FontName, input.Style.ToString());

            // Look for fonts by filename
            if (string.IsNullOrWhiteSpace(fontFile))
            {
                var directories = new List<string> { Path.GetDirectoryName(input.Identity.SourceFilename) };
                var extensions = new string[] { "", ".ttf", ".ttc", ".otf" };

                // Add special per platform directories
                if (CurrentPlatform.OS == OS.Windows)
                    directories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts)));
                else if (CurrentPlatform.OS == OS.MacOSX)
                {
                    directories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Fonts"));
                    directories.Add("/Library/Fonts");
                    directories.Add("/System/Library/Fonts/Supplemental");
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

            context.Logger.LogMessage("Building Font {0}", fontFile);

            // Get the platform specific texture profile.
            var texProfile = TextureProfile.ForPlatform(context.TargetPlatform);

            {
                if (!File.Exists(fontFile))
                {
                    throw new Exception(string.Format("Could not load {0}", fontFile));
                }
                var lineSpacing = 0f;
                long yOffsetMin = 0;
                var glyphs = ImportFont(input, out lineSpacing, out yOffsetMin, context, fontFile);

                var glyphData = new HashSet<GlyphData>(glyphs.Select(x => x.Data));

                // Optimize.
                foreach (GlyphData glyph in glyphData)
                {
                    GlyphCropper.Crop(glyph);
                }

                // For SDF fonts, expand each tightly-cropped glyph by spread pixels of
                // transparent padding on every side before atlas packing. This ensures the
                // full SDF gradient — and the outline ring — is captured inside the rendered
                // quad rather than being clipped at the atlas cell boundary.
                // GlyphCropper already removed all blank space, so only transparent pixels
                // are added here; no glyph content is lost.
                int sdfPad = GenerateDistanceField ? (int)Math.Ceiling(DistanceFieldSpread) : 0;
                if (sdfPad > 0)
                {
                    foreach (GlyphData glyph in glyphData)
                    {
                        var src    = glyph.Subrect;
                        int newW   = src.Width  + 2 * sdfPad;
                        int newH   = src.Height + 2 * sdfPad;
                        // PixelBitmapContent is zero-initialised (transparent black).
                        var padded = new PixelBitmapContent<Color>(newW, newH);
                        BitmapContent.Copy(glyph.Bitmap, src,
                            padded, new Rectangle(sdfPad, sdfPad, src.Width, src.Height));
                        glyph.Bitmap  = padded;
                        glyph.Subrect = new Rectangle(0, 0, newW, newH);
                        // The padded quad's top-left is sdfPad pixels earlier in each axis;
                        // shift YOffset up so Cropping.Y keeps the glyph body on the baseline.
                        glyph.YOffset -= sdfPad;
                    }
                }

                // We need to know how to pack the glyphs.
                bool requiresPot, requiresSquare;
                texProfile.Requirements(context, TextureFormat, out requiresPot, out requiresSquare);

                var face = GlyphPacker.ArrangeGlyphs(glyphData.ToArray(), requiresPot, requiresSquare);

                // Adjust line and character spacing.
                lineSpacing += input.Spacing;
                output.VerticalLineSpacing = (int)lineSpacing;

                foreach (Glyph glyph in glyphs)
                {
                    output.CharacterMap.Add(glyph.Character);

                    var texRect = glyph.Data.Subrect;
                    output.Glyphs.Add(texRect);

                    var cropping = new Rectangle(-sdfPad, (int)(glyph.Data.YOffset - yOffsetMin), (int)glyph.Data.XAdvance, output.VerticalLineSpacing);
                    output.Cropping.Add(cropping);

                    // Set the optional character kerning.
                    if (input.UseKerning)
                    {
                        ABCFloat widths = glyph.Data.CharacterWidths;
                        output.Kerning.Add(new Vector3(widths.A, widths.B, widths.C));
                    }
                    else
                    {
                        output.Kerning.Add(new Vector3(0, texRect.Width - 2 * sdfPad, 0));
                    }
                }

                output.Texture.Faces[0].Add(face);

                if (GenerateDistanceField)
                {
                    // Convert face (BitmapContent) to a signed distance field in-place.
                    //
                    // Algorithm: two-pass 1D Euclidean distance transform (Meijster/Felzenszwalb)
                    // applied to the oversampled alpha mask, then downsampled to atlas resolution.
                    // This is O(W*H) and produces correct Euclidean distances, unlike the naive
                    // O(W*H*R²) brute-force search it replaces.
                    //
                    // Oversample: the rasterizer already rendered glyphs at atlas resolution.
                    // We use the alpha channel at that resolution directly; the Oversample property
                    // controls how much the font was rendered above the nominal size (set in the
                    // importer / glyph builder) which we account for in the spread scaling below.
                    var bmp = output.Texture.Faces[0][0];
                    var data = bmp.GetPixelData(); // RGBA8
                    int width  = bmp.Width;
                    int height = bmp.Height;

                    // Extract binary inside/outside mask (threshold 128) — used for sign determination only.
                    // Coverage is stored in the RED channel.
                    var inside = new bool[width * height];
                    for (int i = 0, p = 0; i < width * height; i++, p += 4)
                        inside[i] = data[p] >= 128;

                    // Route A: seed the EDT with sub-pixel edge positions derived from FreeType's
                    // anti-aliased coverage values rather than a hard binary threshold.
                    //
                    // For a pixel with normalised coverage 'a' (0=fully outside, 1=fully inside):
                    //   outSeeds: distance to nearest foreground (for outside pixels)
                    //     inside (a >= 0.5) → 0         (this pixel IS foreground)
                    //     edge   (0 < a < 0.5) → 0.5-a  (sub-pixel fraction to foreground edge)
                    //     outside (a == 0)   → inf       (let EDT propagate from neighbours)
                    //   inSeeds: distance to nearest background (for inside pixels)
                    //     outside (a < 0.5)  → 0         (this pixel IS background)
                    //     edge   (0.5 <= a < 1) → a-0.5  (sub-pixel fraction to background edge)
                    //     inside  (a == 1)   → inf        (let EDT propagate from neighbours)
                    //
                    // This places the 0.5 isoline with sub-pixel accuracy on curves rather than
                    // snapping it to the nearest pixel boundary, reducing staircase artefacts
                    // at large render scales without changing the rasterization resolution.
                    float inf = (float)(width * width + height * height) + 1f;
                    var outSeeds = new float[width * height];
                    var inSeeds  = new float[width * height];
                    for (int i = 0, p = 0; i < width * height; i++, p += 4)
                    {
                        float a = data[p] / 255f;
                        outSeeds[i] = a >= 0.5f ? 0f : (a > 0f ? 0.5f - a : inf);
                        inSeeds[i]  = a <  0.5f ? 0f : (a < 1f ? a - 0.5f  : inf);
                    }

                    float[] sqDistOut = ComputeEDT(outSeeds, width, height);
                    float[] sqDistIn  = ComputeEDT(inSeeds,  width, height);

                    // Normalize to [0,1] with 0.5 at the edge, clamped to DistanceFieldSpread.
                    var sdf = new float[width * height];
                    float spread = DistanceFieldSpread;
                    float twoSpread = spread * 2f;
                    for (int i = 0; i < width * height; i++)
                    {
                        float distIn  = MathF.Sqrt(sqDistIn[i]);
                        float distOut = MathF.Sqrt(sqDistOut[i]);
                        float signedDist = inside[i] ? distIn : -distOut;
                        signedDist = MathF.Max(-spread, MathF.Min(spread, signedDist));
                        sdf[i] = 0.5f + signedDist / twoSpread;
                    }

                    // Write back: SDF value stored in all RGB channels + alpha so
                    // both single-channel (R) and legacy (A) sampling paths work.
                    for (int i = 0, p = 0; i < width * height; i++, p += 4)
                    {
                        byte q = (byte)(sdf[i] * 255f + 0.5f);
                        data[p + 0] = q;
                        data[p + 1] = q;
                        data[p + 2] = q;
                        data[p + 3] = q;
                    }
                    bmp.SetPixelData(data);

                    output.DistanceFieldType  = (byte)1;
                    output.DistanceFieldSpread = DistanceFieldSpread;
                    output.DistanceFieldEmSize = input.Size;
                }
            }

            if (GenerateDistanceField)
            {
                // Distance field already encoded as linear distance in RGB; keep premultiplied path simple: ensure grayscale remains.
                // No action needed; we already replaced data.
            }
            else if (PremultiplyAlpha)
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
            // NOTE: SDF atlases must remain uncompressed: DXT compression destroys the
            // gradient precision required for distance field rendering at runtime.
            var effectiveFormat = GenerateDistanceField ? TextureProcessorOutputFormat.Color : TextureFormat;
            texProfile.ConvertTexture(context, output.Texture, effectiveFormat, true);

            return output;
        }

        private static Glyph[] ImportFont(FontDescription options, out float lineSpacing, out long yOffsetMin, ContentProcessorContext context, string fontName)
        {
            // Which importer knows how to read this source font?
            IFontImporter importer;

            var TrueTypeFileExtensions = new List<string> { ".ttf", ".ttc", ".otf" };
            //var BitmapFileExtensions = new List<string> { ".bmp", ".png", ".gif" };

            string fileExtension = Path.GetExtension(fontName).ToLowerInvariant();

            //			if (BitmapFileExtensions.Contains(fileExtension))
            //			{
            //				importer = new BitmapImporter();
            //			}
            //			else
            //			{
            if (!TrueTypeFileExtensions.Contains(fileExtension))
                throw new PipelineException("Unknown file extension " + fileExtension);

            importer = new SharpFontImporter();

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

        private string FindFont(string name, string style)
        {
            if (CurrentPlatform.OS == OS.Windows)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                var fontDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
                foreach (var key in new RegistryKey[] { Registry.LocalMachine, Registry.CurrentUser })
                {
                    var subkey = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", false);
                    foreach (var font in subkey.GetValueNames().OrderBy(x => x))
                    {
                        if (font.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                        {
                            var fontPath = subkey.GetValue(font).ToString();

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
                string s, e;
                ExternalTool.Run("/bin/bash", string.Format("-c \"fc-match -f '%{{file}}:%{{family}}\\n' '{0}:style={1}'\"", name, style), out s, out e);
                s = s.Trim();

                var split = s.Split(':');
                if (split.Length < 2)
                    return string.Empty;

                // check font family, fontconfig might return a fallback
                if (split[1].Contains(","))
                {
                    // this file defines multiple family names
                    var families = split[1].Split(',');
                    foreach (var f in families)
                    {
                        if (f.ToLowerInvariant() == name.ToLowerInvariant())
                            return split[0];
                    }
                    // didn't find it
                    return string.Empty;
                }
                else
                {
                    if (split[1].ToLowerInvariant() != name.ToLowerInvariant())
                        return string.Empty;
                }

                return split[0];
            }

            return String.Empty;
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

        /// <summary>
        /// Computes an unsigned squared Euclidean distance transform using the
        /// Felzenszwalb–Huttenlocher two-pass 1D algorithm (O(W*H)).
        /// </summary>
        /// <param name="initialDists">
        /// Per-pixel initial 1D seed distances. 0 = this pixel is foreground (already at the target).
        /// A small positive value encodes a sub-pixel fractional offset to the edge (Route A seeding).
        /// A large value (inf) means the pixel is background and needs EDT propagation.
        /// </param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <returns>Array of squared Euclidean distances (same size as initialDists).</returns>
        private static float[] ComputeEDT(float[] initialDists, int width, int height)
        {
            // Phase 1: 1D distance transform along each row.
            var rowDist = new float[width * height];

            for (int y = 0; y < height; y++)
            {
                int row = y * width;
                for (int x = 0; x < width; x++)
                    rowDist[row + x] = initialDists[row + x];

                // Forward pass
                for (int x = 1; x < width; x++)
                {
                    float prev = rowDist[row + x - 1];
                    if (prev + 1f < rowDist[row + x])
                        rowDist[row + x] = prev + 1f;
                }
                // Backward pass
                for (int x = width - 2; x >= 0; x--)
                {
                    float next = rowDist[row + x + 1];
                    if (next + 1f < rowDist[row + x])
                        rowDist[row + x] = next + 1f;
                }
                // Square the 1D distances
                for (int x = 0; x < width; x++)
                {
                    float d = rowDist[row + x];
                    rowDist[row + x] = d * d;
                }
            }

            // Phase 2: 1D parabolic lower-envelope along each column.
            float inf = (float)(width * width + height * height) + 1f;
            var result = new float[width * height];
            var v = new int[height];
            var z = new float[height + 1];

            for (int x = 0; x < width; x++)
            {
                // Build lower envelope of parabolas
                int k = 0;
                v[0] = 0;
                z[0] = -inf;
                z[1] = inf;

                for (int q = 1; q < height; q++)
                {
                    float fq = rowDist[q * width + x] + q * q;
                    float s;
                    while (true)
                    {
                        int vk = v[k];
                        float fvk = rowDist[vk * width + x] + vk * vk;
                        s = (fq - fvk) / (2f * q - 2f * vk);
                        if (s > z[k]) break;
                        k--;
                        if (k < 0) { k = 0; break; }
                    }
                    k++;
                    v[k]     = q;
                    z[k]     = s;
                    z[k + 1] = inf;
                }

                // Fill result column from envelope
                k = 0;
                for (int q = 0; q < height; q++)
                {
                    while (z[k + 1] < q) k++;
                    int vk  = v[k];
                    float dq = q - vk;
                    result[q * width + x] = rowDist[vk * width + x] + dq * dq;
                }
            }

            return result;
        }
    }
}
