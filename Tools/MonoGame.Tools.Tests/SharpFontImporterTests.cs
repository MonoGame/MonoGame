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

            string fontName = "IBMPlexSans-Regular";
            string fontNameWithExtension = $"{fontName}.ttf";
            var fontpath = Path.Combine("Assets", "Fonts", fontNameWithExtension);
            Assert.True(File.Exists(fontpath), $"Font: \"{fontName}\" does not exist at {fontpath}.");

            SFImp.Import(new FontDescription(fontName, 12, 0, FontDescriptionStyle.Regular, true)
            {
                CharacterRegions = [new CharacterRegion((char)32, (char)127)] //32 to 127 are all printable ascii chars
            }, Path.GetFullPath(fontpath));

            //This is what I use to generate the source of truth, if there are ever any fixes or changes and a new source of truth is needed, its as shrimple 🍤 as this.
            //var newTruth = JsonSerializer.Serialize(SFImp.Glyphs.Select(o => new ABCGlyphData(o.Character, o.Data.CharacterWidths.A, o.Data.CharacterWidths.B, o.Data.CharacterWidths.C)));

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

        //Source of truth
        const string SourceOfTruthJSON = @"[{""Character"":"" "",""SpacingA"":0,""SpacingB"":0,""SpacingC"":4},{""Character"":""!"",""SpacingA"":1,""SpacingB"":3,""SpacingC"":1},{""Character"":""\u0022"",""SpacingA"":1,""SpacingB"":5,""SpacingC"":1},{""Character"":""#"",""SpacingA"":0,""SpacingB"":11,""SpacingC"":0},{""Character"":""$"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":1},{""Character"":""%"",""SpacingA"":1,""SpacingB"":13,""SpacingC"":1},{""Character"":""\u0026"",""SpacingA"":1,""SpacingB"":10,""SpacingC"":0},{""Character"":""\u0027"",""SpacingA"":1,""SpacingB"":2,""SpacingC"":1},{""Character"":""("",""SpacingA"":1,""SpacingB"":5,""SpacingC"":-1},{""Character"":"")"",""SpacingA"":0,""SpacingB"":5,""SpacingC"":0},{""Character"":""*"",""SpacingA"":0,""SpacingB"":7,""SpacingC"":0},{""Character"":""\u002B"",""SpacingA"":1,""SpacingB"":8,""SpacingC"":1},{""Character"":"","",""SpacingA"":0,""SpacingB"":4,""SpacingC"":0},{""Character"":""-"",""SpacingA"":1,""SpacingB"":5,""SpacingC"":0},{""Character"":""."",""SpacingA"":1,""SpacingB"":3,""SpacingC"":0},{""Character"":""/"",""SpacingA"":0,""SpacingB"":6,""SpacingC"":0},{""Character"":""0"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":1},{""Character"":""1"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":1},{""Character"":""2"",""SpacingA"":1,""SpacingB"":8,""SpacingC"":1},{""Character"":""3"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":1},{""Character"":""4"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":1},{""Character"":""5"",""SpacingA"":1,""SpacingB"":8,""SpacingC"":1},{""Character"":""6"",""SpacingA"":1,""SpacingB"":8,""SpacingC"":1},{""Character"":""7"",""SpacingA"":1,""SpacingB"":8,""SpacingC"":1},{""Character"":""8"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":1},{""Character"":""9"",""SpacingA"":1,""SpacingB"":8,""SpacingC"":1},{""Character"":"":"",""SpacingA"":1,""SpacingB"":3,""SpacingC"":1},{""Character"":"";"",""SpacingA"":0,""SpacingB"":4,""SpacingC"":1},{""Character"":""\u003C"",""SpacingA"":1,""SpacingB"":8,""SpacingC"":1},{""Character"":""="",""SpacingA"":1,""SpacingB"":8,""SpacingC"":1},{""Character"":""\u003E"",""SpacingA"":1,""SpacingB"":8,""SpacingC"":1},{""Character"":""?"",""SpacingA"":0,""SpacingB"":8,""SpacingC"":0},{""Character"":""@"",""SpacingA"":1,""SpacingB"":13,""SpacingC"":0},{""Character"":""A"",""SpacingA"":0,""SpacingB"":10,""SpacingC"":0},{""Character"":""B"",""SpacingA"":1,""SpacingB"":9,""SpacingC"":0},{""Character"":""C"",""SpacingA"":0,""SpacingB"":10,""SpacingC"":0},{""Character"":""D"",""SpacingA"":1,""SpacingB"":9,""SpacingC"":1},{""Character"":""E"",""SpacingA"":1,""SpacingB"":8,""SpacingC"":0},{""Character"":""F"",""SpacingA"":1,""SpacingB"":8,""SpacingC"":0},{""Character"":""G"",""SpacingA"":0,""SpacingB"":10,""SpacingC"":1},{""Character"":""H"",""SpacingA"":1,""SpacingB"":9,""SpacingC"":1},{""Character"":""I"",""SpacingA"":0,""SpacingB"":6,""SpacingC"":0},{""Character"":""J"",""SpacingA"":0,""SpacingB"":7,""SpacingC"":1},{""Character"":""K"",""SpacingA"":1,""SpacingB"":9,""SpacingC"":0},{""Character"":""L"",""SpacingA"":1,""SpacingB"":7,""SpacingC"":0},{""Character"":""M"",""SpacingA"":1,""SpacingB"":11,""SpacingC"":1},{""Character"":""N"",""SpacingA"":1,""SpacingB"":9,""SpacingC"":1},{""Character"":""O"",""SpacingA"":0,""SpacingB"":11,""SpacingC"":0},{""Character"":""P"",""SpacingA"":1,""SpacingB"":9,""SpacingC"":0},{""Character"":""Q"",""SpacingA"":0,""SpacingB"":11,""SpacingC"":0},{""Character"":""R"",""SpacingA"":1,""SpacingB"":9,""SpacingC"":0},{""Character"":""S"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":0},{""Character"":""T"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":0},{""Character"":""U"",""SpacingA"":1,""SpacingB"":9,""SpacingC"":1},{""Character"":""V"",""SpacingA"":0,""SpacingB"":10,""SpacingC"":0},{""Character"":""W"",""SpacingA"":0,""SpacingB"":14,""SpacingC"":0},{""Character"":""X"",""SpacingA"":0,""SpacingB"":10,""SpacingC"":0},{""Character"":""Y"",""SpacingA"":0,""SpacingB"":10,""SpacingC"":-1},{""Character"":""Z"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":0},{""Character"":""["",""SpacingA"":1,""SpacingB"":4,""SpacingC"":0},{""Character"":""\\"",""SpacingA"":0,""SpacingB"":6,""SpacingC"":0},{""Character"":""]"",""SpacingA"":0,""SpacingB"":4,""SpacingC"":1},{""Character"":""^"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":1},{""Character"":""_"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":0},{""Character"":""\u0060"",""SpacingA"":2,""SpacingB"":4,""SpacingC"":4},{""Character"":""a"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":0},{""Character"":""b"",""SpacingA"":1,""SpacingB"":8,""SpacingC"":0},{""Character"":""c"",""SpacingA"":0,""SpacingB"":8,""SpacingC"":0},{""Character"":""d"",""SpacingA"":0,""SpacingB"":8,""SpacingC"":1},{""Character"":""e"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":0},{""Character"":""f"",""SpacingA"":0,""SpacingB"":5,""SpacingC"":0},{""Character"":""g"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":-1},{""Character"":""h"",""SpacingA"":1,""SpacingB"":7,""SpacingC"":1},{""Character"":""i"",""SpacingA"":1,""SpacingB"":2,""SpacingC"":1},{""Character"":""j"",""SpacingA"":0,""SpacingB"":3,""SpacingC"":1},{""Character"":""k"",""SpacingA"":1,""SpacingB"":8,""SpacingC"":-1},{""Character"":""l"",""SpacingA"":1,""SpacingB"":3,""SpacingC"":0},{""Character"":""m"",""SpacingA"":1,""SpacingB"":12,""SpacingC"":1},{""Character"":""n"",""SpacingA"":1,""SpacingB"":7,""SpacingC"":1},{""Character"":""o"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":0},{""Character"":""p"",""SpacingA"":1,""SpacingB"":8,""SpacingC"":0},{""Character"":""q"",""SpacingA"":0,""SpacingB"":8,""SpacingC"":1},{""Character"":""r"",""SpacingA"":1,""SpacingB"":5,""SpacingC"":0},{""Character"":""s"",""SpacingA"":0,""SpacingB"":7,""SpacingC"":1},{""Character"":""t"",""SpacingA"":0,""SpacingB"":5,""SpacingC"":1},{""Character"":""u"",""SpacingA"":1,""SpacingB"":7,""SpacingC"":1},{""Character"":""v"",""SpacingA"":0,""SpacingB"":8,""SpacingC"":0},{""Character"":""w"",""SpacingA"":0,""SpacingB"":12,""SpacingC"":0},{""Character"":""x"",""SpacingA"":0,""SpacingB"":8,""SpacingC"":0},{""Character"":""y"",""SpacingA"":0,""SpacingB"":8,""SpacingC"":0},{""Character"":""z"",""SpacingA"":0,""SpacingB"":7,""SpacingC"":0},{""Character"":""{"",""SpacingA"":0,""SpacingB"":5,""SpacingC"":0},{""Character"":""|"",""SpacingA"":1,""SpacingB"":3,""SpacingC"":1},{""Character"":""}"",""SpacingA"":0,""SpacingB"":6,""SpacingC"":-1},{""Character"":""~"",""SpacingA"":0,""SpacingB"":9,""SpacingC"":1},{""Character"":""\u007F"",""SpacingA"":0,""SpacingB"":8,""SpacingC"":0}]";
    }
}
