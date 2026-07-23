using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGame.Tools.Tests
{
    [TestFixture]
    internal class CharacterRegionTypeConverterTests
    {
        private CharacterRegionTypeConverter converter;

        [SetUp]
        public void Setup()
        {
            converter = new CharacterRegionTypeConverter();
        }

        [Test]
        public void ConvertFrom_SingleAsciiCharacter_ReturnsSingleCharacterRegion()
        {
            var result = (CharacterRegion)converter.ConvertFrom(null, null, "A");
            Assert.AreEqual('A', result.Start);
            Assert.AreEqual('A', result.End);
        }

        [Test]
        public void ConvertFrom_AsciiRange_ReturnsCharacterRegion()
        {
            var result = (CharacterRegion)converter.ConvertFrom(null, null, "A-Z");
            Assert.AreEqual('A', result.Start);
            Assert.AreEqual('Z', result.End);
        }

        [Test]
        public void ConvertFrom_SingleUnicodeCharacter_ReturnsSingleCharacterRegion()
        {
            var result = (CharacterRegion)converter.ConvertFrom(null, null, "あ");
            Assert.AreEqual('あ', result.Start);
            Assert.AreEqual('あ', result.End);
        }

        [Test]
        public void ConvertFrom_UnicodeRange_ReturnsCharacterRegion()
        {
            var result = (CharacterRegion)converter.ConvertFrom(null, null, "あ-ん");
            Assert.AreEqual('あ', result.Start);
            Assert.AreEqual('ん', result.End);
        }

        [Test]
        public void ConvertFrom_CyrillicRange_ReturnsCharacterRegion()
        {
            var result = (CharacterRegion)converter.ConvertFrom(null, null, "А-Я");
            Assert.AreEqual('А', result.Start);
            Assert.AreEqual('Я', result.End);
        }

        [Test]
        public void ConvertFrom_HexadecimalCodePoint_ReturnsSingleCharacterRegion()
        {
            var result = (CharacterRegion)converter.ConvertFrom(null, null, "0x3042");
            Assert.AreEqual('あ', result.Start);
            Assert.AreEqual('あ', result.End);
        }

        [Test]
        public void ConvertFrom_DecimalEntityCodePoint_ReturnsSingleCharacterRegion()
        {
            var result = (CharacterRegion)converter.ConvertFrom(null, null, "&#12354;");
            Assert.AreEqual('あ', result.Start);
            Assert.AreEqual('あ', result.End);
        }

        [Test]
        public void ConvertFrom_IntegerCodePoint_ReturnsSingleCharacterRegion()
        {
            var result = (CharacterRegion)converter.ConvertFrom(null, null, "12354"); // 'あ' in decimal
            Assert.AreEqual('あ', result.Start);
            Assert.AreEqual('あ', result.End);
        }

        [Test]
        public void ConvertFrom_InvalidInput_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => converter.ConvertFrom(null, null, ""));
            Assert.Throws<ArgumentException>(() => converter.ConvertFrom(null, null, "A--Z"));
            Assert.Throws<ArgumentException>(() => converter.ConvertFrom(null, null, "Invalid"));
            Assert.Throws<ArgumentException>(() => converter.ConvertFrom(null, null, "0xZZ"));
        }

        [Test]
        public void ConvertFrom_InvalidRangeOrder_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => converter.ConvertFrom(null, null, "Z-A"));
            Assert.Throws<ArgumentException>(() => converter.ConvertFrom(null, null, "ん-あ"));
        }
    }
}
