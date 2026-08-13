using System;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using NUnit.Framework;

namespace MonoGame.Tests.ContentPipeline
{
    class FontDescriptionProcessorTests
    {
        static object[] textureFormats = new object[] {
            new object[] {
                TargetPlatform.DesktopGL,
                TextureProcessorOutputFormat.Color,
            },
            new object[] {
                TargetPlatform.DesktopGL,
                TextureProcessorOutputFormat.Color16Bit,
            },
            new object[] {
                TargetPlatform.DesktopGL,
                TextureProcessorOutputFormat.Compressed,
            },
            new object[] {
                TargetPlatform.Android,
                TextureProcessorOutputFormat.Etc1Compressed,
            },
            new object[] {
                TargetPlatform.iOS,
                TextureProcessorOutputFormat.PvrCompressed,
            },
            new object[] {
                TargetPlatform.iOS,
                TextureProcessorOutputFormat.Compressed,
            },
            new object[] {
                TargetPlatform.Android,
                TextureProcessorOutputFormat.Compressed,
            },
            new object[] {
                TargetPlatform.Windows,
                TextureProcessorOutputFormat.Compressed,
            },
        };

        [Test]
        [TestCaseSource("textureFormats")]
        public void BuildLocalizedFont (TargetPlatform platform, TextureProcessorOutputFormat format)
        {
            var context = new TestProcessorContext(platform, "Localized.xnb");
            var processor = new LocalizedFontProcessor()
            {
                TextureFormat = format,
                PremultiplyAlpha = true,
            };

            LocalizedFontDescription fontDescription = null;
            using (var fs = File.OpenRead(Path.Combine("Assets", "Fonts", "Localized.spritefont")))
            using (var input = XmlReader.Create(new StreamReader(fs)))
                fontDescription = IntermediateSerializer.Deserialize<LocalizedFontDescription>(input, "");
            fontDescription.Identity = new ContentIdentity("Localized.spritefont");

            var output = processor.Process(fontDescription, context);
            Assert.IsNotNull(output, "output should not be null");
            Assert.IsNotNull(output.Texture, "output.Texture should not be null");
            var textureType = output.Texture.Faces[0][0].GetType();
            switch (format)
            {
                case TextureProcessorOutputFormat.Color:
                    Assert.IsTrue(textureType == typeof(PixelBitmapContent<Color>));
                    break;
                case TextureProcessorOutputFormat.Color16Bit:
                    Assert.IsTrue(textureType == typeof(PixelBitmapContent<Microsoft.Xna.Framework.Graphics.PackedVector.Bgr565>));
                    break;
                case TextureProcessorOutputFormat.Compressed:
                    switch (platform)
                    {
                        case TargetPlatform.Windows:
                        case TargetPlatform.DesktopGL:
                            Assert.IsTrue(textureType == typeof(Dxt3BitmapContent));
                            break;
                        case TargetPlatform.iOS:
                            Assert.IsTrue(textureType == typeof(PixelBitmapContent<Microsoft.Xna.Framework.Graphics.PackedVector.Bgra4444>));
                            break;
                        case TargetPlatform.Android:
                            Assert.IsTrue(textureType == typeof(PixelBitmapContent<Microsoft.Xna.Framework.Graphics.PackedVector.Bgra4444>));
                            break;
                    }
                    break;
                case TextureProcessorOutputFormat.PvrCompressed:
                    // because the font is not power of 2 we should use Brga4444
                    Assert.IsTrue(textureType == typeof(PixelBitmapContent<Microsoft.Xna.Framework.Graphics.PackedVector.Bgra4444>));
                    break;
                case TextureProcessorOutputFormat.Etc1Compressed:
                    // because the font has Alpha we should use Brga4444
                    Assert.IsTrue(textureType == typeof(PixelBitmapContent<Microsoft.Xna.Framework.Graphics.PackedVector.Bgra4444>));
                    break;
                default:
                    Assert.Fail("Test not written for " + format);
                    break;
            }
        }

        [Test]
        [TestCaseSource("textureFormats")]
        public void BuildFontFromDescription (TargetPlatform platform, TextureProcessorOutputFormat format)
        {
            var context = new TestProcessorContext(platform, "Arial.xnb");
            var processor = new FontDescriptionProcessor()
            {
                TextureFormat = format,
                PremultiplyAlpha = true,
            };

            FontDescription fontDescription = null;

            using (var input = XmlReader.Create(new StringReader (ArialFont)))
                fontDescription = IntermediateSerializer.Deserialize<FontDescription>(input, "");
            fontDescription.Identity = new ContentIdentity("Arial.spritefont");

            var output = processor.Process(fontDescription, context);
            Assert.IsNotNull(output, "output should not be null");
            Assert.IsNotNull(output.Texture, "output.Texture should not be null");
            var textureType = output.Texture.Faces[0][0].GetType();
            switch (format)
            {
                case TextureProcessorOutputFormat.Color:
                    Assert.IsTrue(textureType == typeof(PixelBitmapContent<Color>));
                    break;
                case TextureProcessorOutputFormat.Color16Bit:
                    Assert.IsTrue(textureType == typeof(PixelBitmapContent<Microsoft.Xna.Framework.Graphics.PackedVector.Bgr565>));
                    break;
                case TextureProcessorOutputFormat.Compressed:
                    switch (platform)
                    {
                        case TargetPlatform.Windows:
                        case TargetPlatform.DesktopGL:
                            Assert.IsTrue(textureType == typeof(Dxt3BitmapContent));
                            break;
                        case TargetPlatform.iOS:
                            Assert.IsTrue(textureType == typeof(PixelBitmapContent<Microsoft.Xna.Framework.Graphics.PackedVector.Bgra4444>));
                            break;
                        case TargetPlatform.Android:
                            Assert.IsTrue(textureType == typeof(PixelBitmapContent<Microsoft.Xna.Framework.Graphics.PackedVector.Bgra4444>));
                            break;
                    }
                    break;
                case TextureProcessorOutputFormat.PvrCompressed:
                    // because the font is not power of 2 we should use Brga4444
                    Assert.IsTrue(textureType == typeof(PixelBitmapContent<Microsoft.Xna.Framework.Graphics.PackedVector.Bgra4444>));
                    break;
                case TextureProcessorOutputFormat.Etc1Compressed:
                    // because the font has Alpha we should use Brga4444
                    Assert.IsTrue(textureType == typeof(PixelBitmapContent<Microsoft.Xna.Framework.Graphics.PackedVector.Bgra4444>));
                    break;
                default:
                    Assert.Fail("Test not written for " + format);
                    break;
            }
        }

        // When kerning is disabled, the space character's advance width should be preserved in the kerning array.
        // See: https://github.com/monogame/monogame/issues/4027
        [Test]
        public void BuildFontFromDescription_WhenKerningDisabled_PreservesSpaceAdvance()
        {

            FontDescription withKerning = null;
            using (var input = XmlReader.Create(new StringReader(ArialFont)))
                withKerning = IntermediateSerializer.Deserialize<FontDescription>(input, "");


            FontDescription withoutKerning = null;
            using (var input = XmlReader.Create(new StringReader(ArialFontWithoutKerning)))
                withoutKerning = IntermediateSerializer.Deserialize<FontDescription>(input, "");

            using TestProcessorContext context = new TestProcessorContext(TargetPlatform.Windows, "Arial.xnb");
            var processor = new FontDescriptionProcessor();
            SpriteFontContent withKerningContnet = processor.Process(withKerning, context);
            SpriteFontContent withoutKerningContent = processor.Process(withoutKerning, context);

            int spaceIndex = withKerningContnet.CharacterMap.IndexOf(' ');

            Vector3 spaceWithKerning = withKerningContnet.Kerning[spaceIndex];
            Vector3 spaceWithoutKerning = withoutKerningContent.Kerning[spaceIndex];

            // Kerning stores the glyph advance as three parts:
            // X = left side bearing
            // Y = glyph width
            // Z = right side bearing
            //
            // With kerning enabled, space can be split across those components,
            // for example (1, 8, 0). With kerning disabled, MonoGame flattens the
            // full advance into Y, so the equivalent result becomes (0, 9, 0).
            float expectedSpaceWidth = spaceWithKerning.X + spaceWithKerning.Y + spaceWithKerning.Z;

            Assert.That(spaceWithoutKerning.Y, Is.EqualTo(expectedSpaceWidth));

        }

        static string ArialFont = @"<?xml version=""1.0"" encoding=""utf-8""?>
<XnaContent xmlns:Graphics=""Microsoft.Xna.Framework.Content.Pipeline.Graphics"">
  <Asset Type=""Graphics:FontDescription"">
    <FontName>Arial</FontName>
    <Size>20</Size>
    <Spacing>0</Spacing>
    <UseKerning>true</UseKerning>
    <Style>Bold</Style>
    <CharacterRegions>
      <CharacterRegion>
        <Start>&#32;</Start>
        <End>&#126;</End>
      </CharacterRegion>
    </CharacterRegions>
  </Asset>
</XnaContent>
";

        static string ArialFontWithoutKerning = @"<?xml version=""1.0"" encoding=""utf-8""?>
<XnaContent xmlns:Graphics=""Microsoft.Xna.Framework.Content.Pipeline.Graphics"">
  <Asset Type=""Graphics:FontDescription"">
    <FontName>Arial</FontName>
    <Size>20</Size>
    <Spacing>0</Spacing>
    <UseKerning>false</UseKerning>
    <Style>Bold</Style>
    <CharacterRegions>
      <CharacterRegion>
        <Start>&#32;</Start>
        <End>&#126;</End>
      </CharacterRegion>
    </CharacterRegions>
  </Asset>
</XnaContent>
";
    }
}
