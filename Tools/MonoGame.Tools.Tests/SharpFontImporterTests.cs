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
        const string SourceOfTruthJSON = @"[{""Character"":"" "",""SpacingA"":0,""SpacingB"":0,""SpacingC"":4},{""Character"":""!"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":4},{""Character"":""\u0022"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":6},{""Character"":""#"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":11},{""Character"":""$"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":10},{""Character"":""%"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":14},{""Character"":""\u0026"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":10},{""Character"":""\u0027"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":3},{""Character"":""("",""SpacingA"":1,""SpacingB"":0,""SpacingC"":4},{""Character"":"")"",""SpacingA"":0,""SpacingB"":-1,""SpacingC"":6},{""Character"":""*"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":7},{""Character"":""\u002B"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":9},{""Character"":"","",""SpacingA"":0,""SpacingB"":0,""SpacingC"":4},{""Character"":""-"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":5},{""Character"":""."",""SpacingA"":1,""SpacingB"":0,""SpacingC"":3},{""Character"":""/"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":6},{""Character"":""0"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":10},{""Character"":""1"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":10},{""Character"":""2"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":9},{""Character"":""3"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":10},{""Character"":""4"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":10},{""Character"":""5"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":9},{""Character"":""6"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":9},{""Character"":""7"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":9},{""Character"":""8"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":10},{""Character"":""9"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":9},{""Character"":"":"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":4},{""Character"":"";"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":5},{""Character"":""\u003C"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":9},{""Character"":""="",""SpacingA"":1,""SpacingB"":0,""SpacingC"":9},{""Character"":""\u003E"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":9},{""Character"":""?"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":8},{""Character"":""@"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":13},{""Character"":""A"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":10},{""Character"":""B"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":9},{""Character"":""C"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":10},{""Character"":""D"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":10},{""Character"":""E"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":8},{""Character"":""F"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":8},{""Character"":""G"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":11},{""Character"":""H"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":10},{""Character"":""I"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":6},{""Character"":""J"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":8},{""Character"":""K"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":9},{""Character"":""L"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":7},{""Character"":""M"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":12},{""Character"":""N"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":10},{""Character"":""O"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":11},{""Character"":""P"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":9},{""Character"":""Q"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":11},{""Character"":""R"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":9},{""Character"":""S"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":9},{""Character"":""T"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":9},{""Character"":""U"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":10},{""Character"":""V"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":10},{""Character"":""W"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":14},{""Character"":""X"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":10},{""Character"":""Y"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":9},{""Character"":""Z"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":9},{""Character"":""["",""SpacingA"":1,""SpacingB"":0,""SpacingC"":4},{""Character"":""\\"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":6},{""Character"":""]"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":5},{""Character"":""^"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":10},{""Character"":""_"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":9},{""Character"":""\u0060"",""SpacingA"":2,""SpacingB"":0,""SpacingC"":8},{""Character"":""a"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":9},{""Character"":""b"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":8},{""Character"":""c"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":8},{""Character"":""d"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":9},{""Character"":""e"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":9},{""Character"":""f"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":5},{""Character"":""g"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":8},{""Character"":""h"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":8},{""Character"":""i"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":3},{""Character"":""j"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":4},{""Character"":""k"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":7},{""Character"":""l"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":3},{""Character"":""m"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":13},{""Character"":""n"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":8},{""Character"":""o"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":9},{""Character"":""p"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":8},{""Character"":""q"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":9},{""Character"":""r"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":5},{""Character"":""s"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":8},{""Character"":""t"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":6},{""Character"":""u"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":8},{""Character"":""v"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":8},{""Character"":""w"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":12},{""Character"":""x"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":8},{""Character"":""y"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":8},{""Character"":""z"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":7},{""Character"":""{"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":5},{""Character"":""|"",""SpacingA"":1,""SpacingB"":0,""SpacingC"":4},{""Character"":""}"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":5},{""Character"":""~"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":10},{""Character"":""\u007F"",""SpacingA"":0,""SpacingB"":0,""SpacingC"":8}]";
    }
}
