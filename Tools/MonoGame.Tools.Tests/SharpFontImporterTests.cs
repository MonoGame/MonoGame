using Microsoft.Win32;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MonoGame.Framework.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MonoGame.Tests.ContentPipeline
{
    [TestFixture]
    internal class SharpFontImporterTests
    {
        record ABCGlyphData(char Character, float SpacingA, float SpacingB, float SpacingC);

        [TestCase]
        public void ValidateABCSpacings()
        {
            SharpFontImporter SFImp = new SharpFontImporter();
            SFImp.Import(new FontDescription("arial", 12, 0, FontDescriptionStyle.Regular, true)
            {
                CharacterRegions = [new CharacterRegion((char)32, (char)127)]
            }, FindFont("arial", "regular"));

            IEnumerable<ABCGlyphData> sourceOfTruthGlyphs = JsonSerializer.Deserialize<IEnumerable<ABCGlyphData>>(SourceOfTruthJSON);

            foreach (var impGlyph in SFImp.Glyphs)
            {
                //Find golden glyph, so we dont just assume sorted set.
                var goldenGlyph = sourceOfTruthGlyphs.First(c => c.Character == impGlyph.Character);
                Assert.NotNull(goldenGlyph, "Source of truth did not contain glyph to test against");

                //Check your ABC's!
                Assert.That(impGlyph.Data.CharacterWidths.A, Is.EqualTo(goldenGlyph.SpacingA), $"A mismatch for '{goldenGlyph.Character}'");
                Assert.That(impGlyph.Data.CharacterWidths.B, Is.EqualTo(goldenGlyph.SpacingB), $"B mismatch for '{goldenGlyph.Character}'");
                Assert.That(impGlyph.Data.CharacterWidths.C, Is.EqualTo(goldenGlyph.SpacingC), $"C mismatch for '{goldenGlyph.Character}'");
            }
        }

        //TODO: This is taken from FontDescriptionProcessor, should we make this a helper function that is accessible?
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

        //TODO: This is taken from FontDescriptionProcessor, should we make this a helper function that is accessible?
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

        //Source of truth
        const string SourceOfTruthJSON = @"[
  {
    ""Character"": "" "",
    ""SpacingA"": 0,
    ""SpacingB"": 0,
    ""SpacingC"": 4
  },
  {
    ""Character"": ""!"",
    ""SpacingA"": 1,
    ""SpacingB"": 3,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""\u0022"",
    ""SpacingA"": 0,
    ""SpacingB"": 5,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""#"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""$"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""%"",
    ""SpacingA"": 0,
    ""SpacingB"": 14,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""\u0026"",
    ""SpacingA"": 0,
    ""SpacingB"": 11,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""\u0027"",
    ""SpacingA"": 0,
    ""SpacingB"": 3,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""("",
    ""SpacingA"": 0,
    ""SpacingB"": 5,
    ""SpacingC"": 0
  },
  {
    ""Character"": "")"",
    ""SpacingA"": 0,
    ""SpacingB"": 5,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""*"",
    ""SpacingA"": 0,
    ""SpacingB"": 6,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""\u002B"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": "","",
    ""SpacingA"": 1,
    ""SpacingB"": 3,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""-"",
    ""SpacingA"": 0,
    ""SpacingB"": 5,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""."",
    ""SpacingA"": 1,
    ""SpacingB"": 3,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""/"",
    ""SpacingA"": 0,
    ""SpacingB"": 5,
    ""SpacingC"": -1
  },
  {
    ""Character"": ""0"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""1"",
    ""SpacingA"": 1,
    ""SpacingB"": 5,
    ""SpacingC"": 3
  },
  {
    ""Character"": ""2"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""3"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""4"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""5"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""6"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""7"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""8"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""9"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": "":"",
    ""SpacingA"": 1,
    ""SpacingB"": 3,
    ""SpacingC"": 0
  },
  {
    ""Character"": "";"",
    ""SpacingA"": 1,
    ""SpacingB"": 3,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""\u003C"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""="",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""\u003E"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""?"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""@"",
    ""SpacingA"": 0,
    ""SpacingB"": 16,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""A"",
    ""SpacingA"": 0,
    ""SpacingB"": 11,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""B"",
    ""SpacingA"": 1,
    ""SpacingB"": 9,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""C"",
    ""SpacingA"": 0,
    ""SpacingB"": 11,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""D"",
    ""SpacingA"": 1,
    ""SpacingB"": 10,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""E"",
    ""SpacingA"": 1,
    ""SpacingB"": 9,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""F"",
    ""SpacingA"": 1,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""G"",
    ""SpacingA"": 0,
    ""SpacingB"": 12,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""H"",
    ""SpacingA"": 1,
    ""SpacingB"": 10,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""I"",
    ""SpacingA"": 1,
    ""SpacingB"": 3,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""J"",
    ""SpacingA"": 0,
    ""SpacingB"": 7,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""K"",
    ""SpacingA"": 1,
    ""SpacingB"": 10,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""L"",
    ""SpacingA"": 1,
    ""SpacingB"": 8,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""M"",
    ""SpacingA"": 1,
    ""SpacingB"": 12,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""N"",
    ""SpacingA"": 1,
    ""SpacingB"": 10,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""O"",
    ""SpacingA"": 0,
    ""SpacingB"": 12,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""P"",
    ""SpacingA"": 1,
    ""SpacingB"": 9,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""Q"",
    ""SpacingA"": 0,
    ""SpacingB"": 12,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""R"",
    ""SpacingA"": 1,
    ""SpacingB"": 11,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""S"",
    ""SpacingA"": 0,
    ""SpacingB"": 10,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""T"",
    ""SpacingA"": 0,
    ""SpacingB"": 10,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""U"",
    ""SpacingA"": 1,
    ""SpacingB"": 10,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""V"",
    ""SpacingA"": 0,
    ""SpacingB"": 11,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""W"",
    ""SpacingA"": 0,
    ""SpacingB"": 15,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""X"",
    ""SpacingA"": 0,
    ""SpacingB"": 11,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""Y"",
    ""SpacingA"": 0,
    ""SpacingB"": 11,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""Z"",
    ""SpacingA"": 0,
    ""SpacingB"": 10,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""["",
    ""SpacingA"": 1,
    ""SpacingB"": 4,
    ""SpacingC"": -1
  },
  {
    ""Character"": ""\\"",
    ""SpacingA"": 0,
    ""SpacingB"": 5,
    ""SpacingC"": -1
  },
  {
    ""Character"": ""]"",
    ""SpacingA"": 0,
    ""SpacingB"": 4,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""^"",
    ""SpacingA"": 0,
    ""SpacingB"": 8,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""_"",
    ""SpacingA"": 0,
    ""SpacingB"": 10,
    ""SpacingC"": -1
  },
  {
    ""Character"": ""\u0060"",
    ""SpacingA"": 0,
    ""SpacingB"": 4,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""a"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""b"",
    ""SpacingA"": 1,
    ""SpacingB"": 8,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""c"",
    ""SpacingA"": 0,
    ""SpacingB"": 8,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""d"",
    ""SpacingA"": 0,
    ""SpacingB"": 8,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""e"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""f"",
    ""SpacingA"": 0,
    ""SpacingB"": 5,
    ""SpacingC"": -1
  },
  {
    ""Character"": ""g"",
    ""SpacingA"": 0,
    ""SpacingB"": 8,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""h"",
    ""SpacingA"": 1,
    ""SpacingB"": 7,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""i"",
    ""SpacingA"": 1,
    ""SpacingB"": 2,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""j"",
    ""SpacingA"": 0,
    ""SpacingB"": 3,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""k"",
    ""SpacingA"": 1,
    ""SpacingB"": 7,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""l"",
    ""SpacingA"": 1,
    ""SpacingB"": 2,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""m"",
    ""SpacingA"": 1,
    ""SpacingB"": 12,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""n"",
    ""SpacingA"": 1,
    ""SpacingB"": 7,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""o"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""p"",
    ""SpacingA"": 1,
    ""SpacingB"": 8,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""q"",
    ""SpacingA"": 0,
    ""SpacingB"": 8,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""r"",
    ""SpacingA"": 1,
    ""SpacingB"": 5,
    ""SpacingC"": -1
  },
  {
    ""Character"": ""s"",
    ""SpacingA"": 0,
    ""SpacingB"": 8,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""t"",
    ""SpacingA"": 0,
    ""SpacingB"": 5,
    ""SpacingC"": -1
  },
  {
    ""Character"": ""u"",
    ""SpacingA"": 1,
    ""SpacingB"": 7,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""v"",
    ""SpacingA"": 0,
    ""SpacingB"": 8,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""w"",
    ""SpacingA"": 0,
    ""SpacingB"": 12,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""x"",
    ""SpacingA"": 0,
    ""SpacingB"": 8,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""y"",
    ""SpacingA"": 0,
    ""SpacingB"": 8,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""z"",
    ""SpacingA"": 0,
    ""SpacingB"": 8,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""{"",
    ""SpacingA"": 0,
    ""SpacingB"": 5,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""|"",
    ""SpacingA"": 1,
    ""SpacingB"": 2,
    ""SpacingC"": 1
  },
  {
    ""Character"": ""}"",
    ""SpacingA"": 0,
    ""SpacingB"": 5,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""~"",
    ""SpacingA"": 0,
    ""SpacingB"": 9,
    ""SpacingC"": 0
  },
  {
    ""Character"": ""\u007F"",
    ""SpacingA"": 2,
    ""SpacingB"": 8,
    ""SpacingC"": 2
  }
]";
    }
}
