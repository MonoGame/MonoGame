// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    class SpriteFontNonVisualTest
    {
        private TestGameBase _game;

        [SetUp]
        public void SetUp()
        {
            _game = new TestGameBase();
            var graphicsDeviceManager = new GraphicsDeviceManager(_game);
#if XNA
            graphicsDeviceManager.ApplyChanges();
#else
            graphicsDeviceManager.CreateDevice();
#endif
        }

        [TearDown]
        public void TearDown()
        {
            _game.Dispose();
        }

        [TestCase("Default", "The quick brown fox jumps over the lazy dog. 1234567890", 605, 21)]
        [TestCase("Default", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 275, 59)]
        [TestCase("Default", "The quick brown fox jumps over the lazy dog.\r1234567890", 594, 21)]
        [TestCase("DataFont", "The quick brown fox jumps over the lazy dog. 1234567890", 417, 19)]
        [TestCase("DataFont", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 196, 53)]
        [TestCase("JingJing", "The quick brown fox jumps over the lazy dog. 1234567890", 918, 45)]
        [TestCase("JingJing", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 435, 135)]
        [TestCase("JingJing", "%", 21, 45)] // LSB=2, W=17, RSB=2
        [TestCase("JingJing", "*", 10, 45)] // LSB=0, W=10, RSB=-1
        [TestCase("Lindsey", "The quick brown fox jumps over the lazy dog. 1234567890", 1031, 49)]
        [TestCase("Lindsey", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 454, 139)]
        [TestCase("Lindsey", "B", 25, 49)] // LSB=-3, W=24, RSB=1
        [TestCase("Motorwerk", "The quick brown fox jumps over the lazy dog. 1234567890", 932, 44)]
        [TestCase("Motorwerk", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 441, 124)]
        [TestCase("Motorwerk", " ", 12, 44)] // LSB=0, W=1, RSB=11
        [TestCase("Motorwerk", "(", 18, 44)] // LSB=3, W=15, RSB=-6
        [TestCase("Motorwerk", ")", 14, 44)] // LSB=-1, W=14, RSB=-1
        [TestCase("Motorwerk", "_", 15, 44)] // LSB=-2, W=15, RSB=0
        [TestCase("QuartzMS", "The quick brown fox jumps over the lazy dog. 1234567890", 947, 39)]
        [TestCase("QuartzMS", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 440, 111)]
        [TestCase("QuartzMS", "#", 20, 39)] // LSB=0, W=20, RSB=0
        [TestCase("SegoeKeycaps", "The quick brown fox jumps over the lazy dog. 1234567890", 988, 20)]
        [TestCase("SegoeKeycaps", "The quick brown fox jumps\nover the lazy dog.\n1234567890", 448, 58)]
        [TestCase("SegoeKeycaps", "!", 16, 20)] // LSB=1, W=15, RSB=0
        public void MeasureString_returns_correct_values(string fontName, string text, float width, float height)
        {
            var font = _game.Content.Load<SpriteFont>(Paths.Font(fontName));
            var actualSize = font.MeasureString(text);
            var expectedSize = new Vector2(width, height);
            Assert.That(actualSize, Is.EqualTo(expectedSize).Using(Vector2Comparer.Epsilon));
        }
    }
}
