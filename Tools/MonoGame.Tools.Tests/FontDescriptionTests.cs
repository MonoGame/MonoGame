using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.ContentPipeline
{
    class FontDescriptionTests
    {
        [Test]
        public void ValidateMembers()
        {
            {
                var font = new FontDescription("FontName", 12.34f, 5.67f);
                Assert.NotNull(font.Characters);
                Assert.AreEqual(0, font.Characters.Count);
                Assert.IsNull(font.DefaultCharacter);
                Assert.AreEqual("FontName", font.FontName);
                Assert.AreEqual(12.34f, font.Size);
                Assert.AreEqual(5.67f, font.Spacing);
                Assert.AreEqual(FontDescriptionStyle.Regular, font.Style);
                Assert.AreEqual(false, font.UseKerning);
                Assert.IsNull(font.Identity);
                Assert.IsNull(font.Name);
                Assert.NotNull(font.OpaqueData);
                Assert.AreEqual(0, font.OpaqueData.Count);
            }

            {
                var font = new FontDescription("FontName", 12.34f, 5.67f, FontDescriptionStyle.Italic);
                Assert.NotNull(font.Characters);
                Assert.AreEqual(0, font.Characters.Count);
                Assert.IsNull(font.DefaultCharacter);
                Assert.AreEqual("FontName", font.FontName);
                Assert.AreEqual(12.34f, font.Size);
                Assert.AreEqual(5.67f, font.Spacing);
                Assert.AreEqual(FontDescriptionStyle.Italic, font.Style);
                Assert.AreEqual(false, font.UseKerning);
                Assert.IsNull(font.Identity);
                Assert.IsNull(font.Name);
                Assert.NotNull(font.OpaqueData);
                Assert.AreEqual(0, font.OpaqueData.Count);
            }

            {
                var font = new FontDescription("FontName", 12.34f, 5.67f, FontDescriptionStyle.Bold, true);
                Assert.NotNull(font.Characters);
                Assert.AreEqual(0, font.Characters.Count);
                Assert.IsNull(font.DefaultCharacter);
                Assert.AreEqual("FontName", font.FontName);
                Assert.AreEqual(12.34f, font.Size);
                Assert.AreEqual(5.67f, font.Spacing);
                Assert.AreEqual(FontDescriptionStyle.Bold, font.Style);
                Assert.AreEqual(true, font.UseKerning);
                Assert.IsNull(font.Identity);
                Assert.IsNull(font.Name);
                Assert.NotNull(font.OpaqueData);
                Assert.AreEqual(0, font.OpaqueData.Count);
            }

            Assert.Throws<ArgumentNullException>(() => new FontDescription(null, 1, 1));
            Assert.Throws<ArgumentNullException>(() => new FontDescription("", 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FontDescription("Aye", 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FontDescription("Aye", -1, 1));

            {
                var font = new FontDescription("Bee", 1, 1);
                
                font.DefaultCharacter = 'A';
                Assert.AreEqual('A', font.DefaultCharacter);
                font.DefaultCharacter = null;
                Assert.IsNull(font.DefaultCharacter);

                font.FontName = "See";
                Assert.AreEqual("See", font.FontName);
                Assert.Throws<ArgumentNullException>(() => font.FontName = null);
                Assert.Throws<ArgumentNullException>(() => font.FontName = "");

                font.Size = 2;
                Assert.AreEqual(2, font.Size);
                Assert.Throws<ArgumentOutOfRangeException>(() => font.Size = 0);
                Assert.Throws<ArgumentOutOfRangeException>(() => font.Size = -1);

                font.Spacing = 2;
                Assert.AreEqual(2, font.Spacing);
                font.Spacing = 0;
                Assert.AreEqual(0, font.Spacing);
                font.Spacing = -2;
                Assert.AreEqual(-2, font.Spacing);

                font.Style = FontDescriptionStyle.Italic;
                Assert.AreEqual(FontDescriptionStyle.Italic, font.Style);

                font.UseKerning = true;
                Assert.AreEqual(true, font.UseKerning);
            }

        }
    }
}
