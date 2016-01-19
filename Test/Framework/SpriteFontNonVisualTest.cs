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
        private SpriteFont _font;

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
            
            // DataFont has a non-zero Spacing property.
            _font = _game.Content.Load<SpriteFont>(Paths.Font("DataFont"));
        }

        [TearDown]
        public void TearDown()
        {
            _game.Dispose();
        }

        [TestCase("The quick brown fox jumps over the lazy dog. 1234567890", 417, 19)]
        [TestCase("The quick brown fox jumps\nover the lazy dog.\n1234567890", 196, 53)]
        public void MeasureString_returns_correct_values(string text, float width, float height)
        {
            var actualSize = _font.MeasureString(text);
            var expectedSize = new Vector2(width, height);
            Assert.That(actualSize, Is.EqualTo(expectedSize).Using(Vector2Comparer.Epsilon));
        }
    }
}
