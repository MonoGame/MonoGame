// MonoGame - Distance Field SpriteFont tests
// Verifies distance field metadata defaults and that the distance field effect
// variant is exercised (parameters applied) when internal DF fields are set.

using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    [NonParallelizable]
    class SpriteFontDistanceFieldTest : GraphicsDeviceTestFixtureBase
    {
        private SpriteBatch _spriteBatch;
        private SpriteFont _defaultFont;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _spriteBatch = new SpriteBatch(gd);
            _defaultFont = content.Load<SpriteFont>(Paths.Font("Default"));
        }

        [TearDown]
        public override void TearDown()
        {
            _spriteBatch.Dispose();
            _spriteBatch = null;
            base.TearDown();
        }

        [Test]
        [RunOnUI]
        public void DefaultFont_IsNotDistanceField()
        {
            // Internal property should report false for existing assets.
            Assert.False(_defaultFont.IsDistanceField, "Expected legacy test font to have no distance field metadata.");
        }

        [Test]
        [RunOnUI]
        public void DistanceFieldFlags_TriggerEffectParameterApplication()
        {
            // Arrange: reflectively set internal DF metadata on an existing font instance
            var fontType = typeof(SpriteFont);
            var dfTypeField = fontType.GetField("_distanceFieldType", BindingFlags.NonPublic | BindingFlags.Instance);
            var dfSpreadField = fontType.GetField("_distanceFieldSpread", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(dfTypeField, "Could not find _distanceFieldType field");
            Assert.NotNull(dfSpreadField, "Could not find _distanceFieldSpread field");

            // Get internal enum type and create value for SDF (underlying byte = 1)
            var enumType = fontType.GetNestedType("DistanceFieldType", BindingFlags.NonPublic);
            Assert.NotNull(enumType, "Could not find DistanceFieldType enum");
            var sdfEnumValue = Enum.ToObject(enumType, 1); // SDF
            float spreadValue = 6f;

            dfTypeField.SetValue(_defaultFont, sdfEnumValue);
            dfSpreadField.SetValue(_defaultFont, spreadValue);

            Assert.True(_defaultFont.IsDistanceField, "Distance field flag not recognized after reflection set.");

            // Capture effect instance and reset parameters (if any) to sentinel values
            var dfEffect = DistanceFieldSpriteEffect.Instance(gd);
            var spreadParam = dfEffect.Parameters["Spread"];
            var dfTypeParam = dfEffect.Parameters["DFType"];
            // Set sentinel distinct from expected
            spreadParam?.SetValue(-1f);
            Assert.Null(dfTypeParam, "DFType parameter should not exist in the SDF-only shader path.");

            // Act: draw using SpriteBatch default path (no custom effect)
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_defaultFont, "abc", Vector2.Zero, Color.White);
            _spriteBatch.End();

            // Assert: effect parameters updated by SpriteBatcher -> DistanceFieldSpriteEffect.ApplyDistanceFieldSettings
            if (spreadParam != null)
                Assert.AreEqual(spreadValue, spreadParam.GetValueSingle(), 1e-5, "Spread parameter not applied");
        }
    }
}
