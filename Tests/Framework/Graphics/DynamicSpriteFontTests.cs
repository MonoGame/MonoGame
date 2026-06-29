// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics;

[NonParallelizable]
[RunOnUiTestFixture]
internal sealed class DynamicSpriteFontTest : GraphicsDeviceTestFixtureBase
{
#if DIRECTX12 || VULKAN
    [Test]
    public void FromFile_GraphicsDeviceIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DynamicSpriteFont.FromFile(null!, Paths.Font("IBMPlexSans-Regular.ttf"), 32.0f, Array.Empty<CharacterRegion>()));
    }

    [Test]
    public void FromFile_PathIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DynamicSpriteFont.FromFile(gd, null!, 32.0f, Array.Empty<CharacterRegion>()));
    }

    [Test]
    public void FromFile_WhitespacePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => DynamicSpriteFont.FromFile(gd, " ", 32.0f, Array.Empty<CharacterRegion>()));
    }

    [Test]
    public void FromFile_EmptyPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => DynamicSpriteFont.FromFile(gd, string.Empty, 32.0f, Array.Empty<CharacterRegion>()));
    }

    [Test]
    public void FromFile_CharacterRegionsIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DynamicSpriteFont.FromFile(gd, Paths.Font("IBMPlexSans-Regular.ttf"), 32.0f, null!));
    }

    [Test]
    public void FromFile_SizeIsZero_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DynamicSpriteFont.FromFile(gd, Paths.Font("IBMPlexSans-Regular.ttf"), 0.0f, Array.Empty<CharacterRegion>()));
    }

    [Test]
    public void FromFile_EmptyCharacterRegions_ReturnsDynamicSpriteFont()
    {
        using DynamicSpriteFont font = DynamicSpriteFont.FromFile(gd, Paths.Font("IBMPlexSans-Regular.ttf"), 32.0f, Array.Empty<CharacterRegion>());

        Assert.That(font, Is.Not.Null);
    }

    [Test]
    public void FromFile_ValidArguments_SetsInitialSize()
    {
        using DynamicSpriteFont font = DynamicSpriteFont.FromFile(gd, Paths.Font("IBMPlexSans-Regular.ttf"), 32.0f, Array.Empty<CharacterRegion>());

        Assert.That(font.Size, Is.EqualTo(32.0f));
    }

    [Test]
    public void FromFile_WithoutCharacterRegions_ReturnsDynamicSpriteFont()
    {
        using DynamicSpriteFont font = DynamicSpriteFont.FromFile(gd, Paths.Font("IBMPlexSans-Regular.ttf"), 32.0f);

        Assert.That(font, Is.Not.Null);
        Assert.That(font.Size, Is.EqualTo(32.0f));
    }

    [Test]
    public void FromFile_ValidOtfArguments_SetsInitialSize()
    {
        using DynamicSpriteFont font = DynamicSpriteFont.FromFile(gd, Paths.Font("IBMPlexSans-Regular.otf"), 32.0f, Array.Empty<CharacterRegion>());

        Assert.That(font.Size, Is.EqualTo(32.0f));
    }

    [Test]
    public void FromStream_GraphicsDeviceIsNull_ThrowsArgumentNullException()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            Assert.Throws<ArgumentNullException>(() => DynamicSpriteFont.FromStream(null!, stream, 3.0f, Array.Empty<CharacterRegion>()));
        }
    }

    [Test]
    public void FromStream_StreamIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DynamicSpriteFont.FromStream(gd, null!, 32.0f, Array.Empty<CharacterRegion>()));
    }

    [Test]
    public void FromStream_SizeIsZero_ThrowsArgumentOutOfRangeException()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DynamicSpriteFont.FromStream(gd, stream, 0.0f, Array.Empty<CharacterRegion>()));
        }
    }

    [Test]
    public void FromStream_CharacterRegionsIsNull_ThrowsArgumentNullException()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            Assert.Throws<ArgumentNullException>(() => DynamicSpriteFont.FromStream(gd, stream, 32.0f, null));
        }
    }

    [Test]
    public void FromStream_EmptyCharacterRegions_ReturnsDynamicSpriteFont()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            Assert.That(font, Is.Not.Null);
        }
    }

    [Test]
    public void FromStream_WithCharacterRegions_WarmsGlyphsAtCreation()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            CharacterRegion[] characterRegions = new[] { new CharacterRegion('a', 'c') };
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, characterRegions);

            Texture2D initialTexture = font.GetTexture(0);
            font.MeasureString("abc");

            Assert.That(initialTexture.Width, Is.EqualTo(1024));
            Assert.That(initialTexture.Height, Is.EqualTo(1024));
            Assert.That(font.GetTexture(0), Is.SameAs(initialTexture));
        }
    }    

    [Test]
    public void FromStream_ValidArguments_SetsInitialSize()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            Assert.That(font.Size, Is.EqualTo(32.0f));
        }
    }

    [Test]
    public void FromStream_WithoutCharacterRegions_ReturnsDynamicSpriteFont()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f);

            Assert.That(font, Is.Not.Null);
            Assert.That(font.Size, Is.EqualTo(32.0f));
        }
    }

    [Test]
    [TestCase((char)127)]
    [TestCase((char)31)]
    public void DefaultCharacter_SetToUnavailableCharacter_ThrowsArgumentException(char character)
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f);

            Assert.Throws<ArgumentException>(() => font.DefaultCharacter = character);
        }
    }

    [Test]
    [TestCase((char)32)]
    [TestCase((char)63)]
    public void DefaultCharacter_SetToAvailableCharacter_DoesNotThrow(char character)
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f);

            Assert.DoesNotThrow(() => font.DefaultCharacter = character);
        }
    }

    [Test]
    public void MeasureString_WithDefaultCharacter_UsesFallbackGlyph()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f);
            string unresolvedText = ((char)127).ToString();

            font.DefaultCharacter = '?';

            Vector2 fallbackSize = font.MeasureString("?");
            Vector2 unresolvedSize = font.MeasureString(unresolvedText);

            Assert.That(unresolvedSize, Is.EqualTo(fallbackSize).Using(Vector2Comparer.Epsilon));
        }
    }

    [Test]
    public void MeasureString_WithDefaultCharacterAfterSizeChange_UsesFallbackGlyph()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 16.0f);
            string unresolvedText = ((char)127).ToString();

            font.DefaultCharacter = '?';

            Vector2 smallFallbackSize = font.MeasureString("?");
            Vector2 smallUnresolvedSize = font.MeasureString(unresolvedText);

            font.Size = 32.0f;

            Vector2 largeFallbackSize = font.MeasureString("?");
            Vector2 largeUnresolvedSize = font.MeasureString(unresolvedText);

            Assert.That(smallUnresolvedSize, Is.EqualTo(smallFallbackSize).Using(Vector2Comparer.Epsilon));
            Assert.That(largeUnresolvedSize, Is.EqualTo(largeFallbackSize).Using(Vector2Comparer.Epsilon));
        }
    }

    [Test]
    public void Size_SetToZero_ThrowsArgumentOutOfRangeException()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            Assert.Throws<ArgumentOutOfRangeException>(() => font.Size = 0.0f);
        }
    }

    [Test]
    public void Size_SetToNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            Assert.Throws<ArgumentOutOfRangeException>(() => font.Size = -1.0f);
        }
    }

    [Test]
    public void Size_SetToPositiveValue_UpdatesSize()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            font.Size = 16.0f;

            Assert.That(font.Size, Is.EqualTo(16.0f));
        }
    }

    [Test]
    public void Size_SetToNaN_ThrowsArgumentOutOfRangeException()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            Assert.Throws<ArgumentOutOfRangeException>(() => font.Size = float.NaN);
        }
    }

    [Test]
    public void Size_SetToPositiveInfinity_ThrowsArgumentOutOfRangeException()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            Assert.Throws<ArgumentOutOfRangeException>(() => font.Size = float.PositiveInfinity);
        }
    }

    [Test]
    public void Size_SetToNegativeInfinity_ThrowsArgumentOutOfRangeException()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            Assert.Throws<ArgumentOutOfRangeException>(() => font.Size = float.NegativeInfinity);
        }
    }

    [Test]
    public void MeasureString_EmptyString_ReturnsVector2Zero()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            Vector2 size = font.MeasureString(string.Empty);

            Assert.That(size, Is.EqualTo(Vector2.Zero).Using(Vector2Comparer.Epsilon));
        }
    }

    [Test]
    public void MeasureString_TextIsNull_ThrowsArgumentNullException()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            Assert.Throws<ArgumentNullException>(() => font.MeasureString((string)null!));
        }
    }

    [Test]
    public void MeasureString_WithoutPreloadedGlyphs_ReturnsPositiveWidth()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            Vector2 size = font.MeasureString("abc");

            Assert.That(size.X, Is.GreaterThan(0.0f));
        }
    }

    [Test]
    public void MeasureStringStringBuilder_EmptyString_ReturnsVector2Zero()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            Vector2 size = font.MeasureString(new StringBuilder());

            Assert.That(size, Is.EqualTo(Vector2.Zero).Using(Vector2Comparer.Epsilon));
        }
    }

    [Test]
    public void MeasureStringStringBuilder_TextIsNull_ThrowsArgumentNullException()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            Assert.Throws<ArgumentNullException>(() => font.MeasureString((StringBuilder)null!));
        }
    }

    [Test]
    public void MeasureStringStringBuilder_WithoutPreloadedGlyphs_ReturnsPositiveWidth()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            Vector2 size = font.MeasureString(new StringBuilder("abc"));

            Assert.That(size.X, Is.GreaterThan(0.0f));
        }
    }

    [Test]
    public void MeasureString_SizeChanges_ChangesMeasuredWidth()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 16.0f, Array.Empty<CharacterRegion>());

            Vector2 smallSize = font.MeasureString("abc");

            font.Size = 32.0f;
            Vector2 largeSize = font.MeasureString("abc");

            Assert.That(largeSize.X, Is.GreaterThan(smallSize.X));
        }
    }

    [Test]
    public void MeasureString_SizeChangesBackToPreviousSize_RestoresMeasuredWidth()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 16.0f, Array.Empty<CharacterRegion>());

            Vector2 smallSize = font.MeasureString("abc");

            font.Size = 32.0f;
            font.MeasureString("abc");

            font.Size = 16.0f;
            Vector2 restoredSize = font.MeasureString("abc");

            Assert.That(restoredSize, Is.EqualTo(smallSize).Using(Vector2Comparer.Epsilon));
        }
    }

    [Test]
    public void MeasureString_SizeChangesBackToPreviousSize_KeepsSharedAtlasTexture()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 16.0f, Array.Empty<CharacterRegion>());

            font.MeasureString("a");
            Texture2D initialTexture = font.GetTexture(0);

            font.Size = 32.0f;
            font.MeasureString("W");
            Texture2D grownTexture = font.GetTexture(0);

            font.Size = 16.0f;
            font.MeasureString("a");

            Assert.That(initialTexture, Is.Not.Null);
            Assert.That(grownTexture, Is.SameAs(font.GetTexture(0)));
        }
    }

    [Test]
    public void DrawString_SampleShapedDeferredBatch_AfterRepeatedFontRecreation_DoesNotThrow()
    {
        string visibleAscii = CreateVisibleAsciiString();

        for (int i = 0; i < 3; i++)
        {
            using (Stream stream = OpenRuntimeFontStream())
            using (SpriteBatch spriteBatch = new SpriteBatch(gd))
            {
                using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 128.0f, Array.Empty<CharacterRegion>());

                spriteBatch.Begin();

                Assert.DoesNotThrow(() => spriteBatch.DrawString(font,
                                                                 "Small texture atlas: This line will be drawn before page growth.",
                                                                 new Vector2(40.0f, 40.0f),
                                                                 Color.Yellow));

                Texture2D textureBeforeLargeDraw = font.GetTexture(0);

                Assert.DoesNotThrow(() => spriteBatch.DrawString(font,
                                                                 visibleAscii,
                                                                 new Vector2(40.0f, 280.0f),
                                                                 Color.White));

                Assert.DoesNotThrow(() => spriteBatch.DrawString(font,
                                                                 "Everything after page growth should still draw.",
                                                                 new Vector2(40.0f, 360.0f),
                                                                 Color.White));

                Texture2D textureAfterLargeDraw = font.GetTexture(0);

                Assert.DoesNotThrow(() => spriteBatch.End());
                Assert.That(textureBeforeLargeDraw, Is.Not.Null);
                Assert.That(textureAfterLargeDraw, Is.Not.Null);
                Assert.That(font.TotalPages, Is.GreaterThan(1));
            }
        }
    }

    [Test]
    public void MeasureStringStringBuilder_SizeChanges_ChangesMeasuredWidth()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 16.0f, Array.Empty<CharacterRegion>());

            Vector2 smallSize = font.MeasureString(new StringBuilder("abc"));

            font.Size = 32.0f;
            Vector2 largeSize = font.MeasureString(new StringBuilder("abc"));

            Assert.That(largeSize.X, Is.GreaterThan(smallSize.X));
        }
    }

    [Test]
    public void DrawString_WithoutPreloadedGlyphs_DoesNotThrow()
    {
        using (Stream stream = OpenRuntimeFontStream())
        using (SpriteBatch spriteBatch = new SpriteBatch(gd))
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            spriteBatch.Begin();
            Assert.DoesNotThrow(() => spriteBatch.DrawString(font, "abc", Vector2.Zero, Color.White));
            spriteBatch.End();
        }
    }

    [Test]
    public void DrawStringStringBuilder_WithoutPreloadedGlyphs_DoesNotThrow()
    {
        using (Stream stream = OpenRuntimeFontStream())
        using (SpriteBatch spriteBatch = new SpriteBatch(gd))
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            spriteBatch.Begin();
            Assert.DoesNotThrow(() => spriteBatch.DrawString(font, new StringBuilder("abc"), Vector2.Zero, Color.White));
            spriteBatch.End();
        }
    }

    [Test]
    public void DrawStringScaled_WithoutPreloadedGlyphs_DoesNotThrow()
    {
        using (Stream stream = OpenRuntimeFontStream())
        using (SpriteBatch spriteBatch = new SpriteBatch(gd))
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            spriteBatch.Begin();
            Assert.DoesNotThrow(() =>
                spriteBatch.DrawString(font, "abc", Vector2.Zero, Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f));
            spriteBatch.End();
        }
    }

    [Test]
    public void DrawStringVectorScale_WithoutPreloadedGlyphs_DoesNotThrow()
    {
        using (Stream stream = OpenRuntimeFontStream())
        using (SpriteBatch spriteBatch = new SpriteBatch(gd))
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            spriteBatch.Begin();
            Assert.DoesNotThrow(() =>
                spriteBatch.DrawString(font, "abc", Vector2.Zero, Color.White, 0.0f, Vector2.Zero, new Vector2(2.0f, 1.5f), SpriteEffects.None, 0.0f));
            spriteBatch.End();
        }
    }

    [Test]
    public void DrawStringStringBuilderScaled_WithoutPreloadedGlyphs_DoesNotThrow()
    {
        using (Stream stream = OpenRuntimeFontStream())
        using (SpriteBatch spriteBatch = new SpriteBatch(gd))
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            spriteBatch.Begin();
            Assert.DoesNotThrow(() =>
                spriteBatch.DrawString(font, new StringBuilder("abc"), Vector2.Zero, Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f));
            spriteBatch.End();
        }
    }

    [Test]
    public void DrawStringStringBuilderVectorScale_WithoutPreloadedGlyphs_DoesNotThrow()
    {
        using (Stream stream = OpenRuntimeFontStream())
        using (SpriteBatch spriteBatch = new SpriteBatch(gd))
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            spriteBatch.Begin();
            Assert.DoesNotThrow(() =>
                spriteBatch.DrawString(font, new StringBuilder("abc"), Vector2.Zero, Color.White, 0.0f, Vector2.Zero, new Vector2(2.0f, 1.5f), SpriteEffects.None, 0.0f));
            spriteBatch.End();
        }
    }

    [Test]
    public void DrawStringRtl_WithoutPreloadedGlyphs_DoesNotThrow()
    {
        using (Stream stream = OpenRuntimeFontStream())
        using (SpriteBatch spriteBatch = new SpriteBatch(gd))
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            spriteBatch.Begin();
            Assert.DoesNotThrow(() =>
                spriteBatch.DrawString(font, "abc", Vector2.Zero, Color.White, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f, true));
            spriteBatch.End();
        }
    }

    [Test]
    public void DrawStringStringBuilderRtl_WithoutPreloadedGlyphs_DoesNotThrow()
    {
        using (Stream stream = OpenRuntimeFontStream())
        using (SpriteBatch spriteBatch = new SpriteBatch(gd))
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());

            spriteBatch.Begin();
            Assert.DoesNotThrow(() =>
                spriteBatch.DrawString(font, new StringBuilder("abc"), Vector2.Zero, Color.White, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f, true));
            spriteBatch.End();
        }
    }

    [Test]
    public void MeasureString_StringAndStringBuilder_ReturnSameValue()
    {
        using (Stream stream = OpenRuntimeFontStream())
        {
            using DynamicSpriteFont font = DynamicSpriteFont.FromStream(gd, stream, 32.0f, Array.Empty<CharacterRegion>());
            string text = "abc\nxyz";

            Vector2 stringSize = font.MeasureString(text);
            Vector2 builderSize = font.MeasureString(new StringBuilder(text));

            Assert.That(builderSize, Is.EqualTo(stringSize).Using(Vector2Comparer.Epsilon));
        }
    }

    private static Stream OpenRuntimeFontStream()
    {
        return File.OpenRead(Paths.Font("IBMPlexSans-Regular.ttf"));
    }

    private static string CreateVisibleAsciiString()
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < 255; i++)
        {
            char character = (char)i;
            if (!char.IsControl(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }
#endif
}
