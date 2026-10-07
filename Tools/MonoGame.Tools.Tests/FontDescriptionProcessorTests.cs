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

        // When kerning is disabled, match the space glyph width emitted by XNA-built SpriteFonts.
        // See: https://github.com/monogame/monogame/issues/4027
        [Test]
        public void BuildFontFromDescription_WhenKerningDisabled_MatchesXnaSpaceWidth()
        {
            FontDescription withoutKerning = null;
            using (var input = XmlReader.Create(new StringReader(ArialFontWithoutKerning)))
                withoutKerning = IntermediateSerializer.Deserialize<FontDescription>(input, "");

            using TestProcessorContext context = new TestProcessorContext(TargetPlatform.Windows, "Arial.xnb");
            var processor = new FontDescriptionProcessor();
            SpriteFontContent withoutKerningContent = processor.Process(withoutKerning, context);

            int spaceIndex = withoutKerningContent.CharacterMap.IndexOf(' ');
            Vector3 spaceWithoutKerning = withoutKerningContent.Kerning[spaceIndex];

            Assert.That(spaceWithoutKerning.Y, Is.EqualTo(9.0f));

        }

        // See: https://github.com/MonoGame/MonoGame/issues/6545
        [Test]
        public void BuildFontFromDescription_MetricOnlyGlyph_HasNeutralCropping()
        {
            FontDescription fontDescription = null;
            using (var input = XmlReader.Create(new StringReader(ArialFont)))
                fontDescription = IntermediateSerializer.Deserialize<FontDescription>(input, "");

            using TestProcessorContext context = new TestProcessorContext(TargetPlatform.Windows, "Arial.xnb");
            FontDescriptionProcessor processor = new FontDescriptionProcessor();
            SpriteFontContent output = processor.Process(fontDescription, context);

            int spaceIndex = output.CharacterMap.IndexOf(' ');

            Assert.That(output.Cropping[spaceIndex], Is.EqualTo(new Rectangle(0, 0, 1, 1)));
        }

        // See: https://github.com/MonoGame/MonoGame/issues/6618
        [Test]
        public void BuildFontFromDescription_MetricOnlyGlyph_UsesOnePixelWidth()
        {
            FontDescription fontDescription = null;
            using (var input = XmlReader.Create(new StringReader(ArialFont)))
                fontDescription = IntermediateSerializer.Deserialize<FontDescription>(input, "");

            using TestProcessorContext context = new TestProcessorContext(TargetPlatform.Windows, "Arial.xnb");
            FontDescriptionProcessor processor = new FontDescriptionProcessor();
            SpriteFontContent output = processor.Process(fontDescription, context);

            int spaceIndex = output.CharacterMap.IndexOf(' ');

            Assert.That(output.Kerning[spaceIndex].Y, Is.EqualTo(1.0f));
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
