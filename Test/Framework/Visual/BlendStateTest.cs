// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    internal class BlendStateTest : VisualTestFixtureBase
    {
        [Test]
        public void ShouldNotBeAbleToSetNullBlendState()
        {
            Game.DrawWith += (sender, e) =>
            {
                Assert.Throws<ArgumentNullException>(() => Game.GraphicsDevice.BlendState = null);
            };
            Game.Run();
        }

        [Test]
        public void ShouldNotBeAbleToMutateStateObjectAfterBindingToGraphicsDevice()
        {
            Game.DrawWith += (sender, e) =>
            {
                var blendState = new BlendState();

                // Can mutate before binding.
                DoAsserts(blendState, Assert.DoesNotThrow);

                // Can't mutate after binding.
                Game.GraphicsDevice.BlendState = blendState;
                DoAsserts(blendState, d => Assert.Throws<InvalidOperationException>(d));

                // Even after changing to different BlendState, you still can't mutate a previously-bound object.
                Game.GraphicsDevice.BlendState = BlendState.Opaque;
                DoAsserts(blendState, d => Assert.Throws<InvalidOperationException>(d));
            };
            Game.Run();
        }

        [Test]
        public void ShouldNotBeAbleToMutateDefaultStateObjects()
        {
            Game.DrawWith += (sender, e) =>
            {
                DoAsserts(BlendState.Additive, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(BlendState.AlphaBlend, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(BlendState.NonPremultiplied, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(BlendState.Opaque, d => Assert.Throws<InvalidOperationException>(d));
            };
            Game.Run();
        }

        private static void DoAsserts(BlendState blendState, Action<TestDelegate> assertMethod)
        {
            assertMethod(() => blendState.AlphaBlendFunction = BlendFunction.Add);
            assertMethod(() => blendState.AlphaDestinationBlend = Blend.BlendFactor);
            assertMethod(() => blendState.AlphaSourceBlend = Blend.BlendFactor);
            assertMethod(() => blendState.BlendFactor = Color.White);
            assertMethod(() => blendState.ColorBlendFunction = BlendFunction.Add);
            assertMethod(() => blendState.ColorDestinationBlend = Blend.BlendFactor);
            assertMethod(() => blendState.ColorSourceBlend = Blend.BlendFactor);
            assertMethod(() => blendState.ColorWriteChannels = ColorWriteChannels.All);
            assertMethod(() => blendState.ColorWriteChannels1 = ColorWriteChannels.All);
            assertMethod(() => blendState.ColorWriteChannels2 = ColorWriteChannels.All);
            assertMethod(() => blendState.ColorWriteChannels3 = ColorWriteChannels.All);
#if !XNA
            assertMethod(() => blendState.IndependentBlendEnable = true);
#endif
            assertMethod(() => blendState.MultiSampleMask = 0);

#if !XNA
            for (var i = 0; i < 4; i++)
            {
                assertMethod(() => blendState[0].AlphaBlendFunction = BlendFunction.Add);
                assertMethod(() => blendState[0].AlphaDestinationBlend = Blend.BlendFactor);
                assertMethod(() => blendState[0].AlphaSourceBlend = Blend.BlendFactor);
                assertMethod(() => blendState[0].ColorBlendFunction = BlendFunction.Add);
                assertMethod(() => blendState[0].ColorDestinationBlend = Blend.BlendFactor);
                assertMethod(() => blendState[0].ColorSourceBlend = Blend.BlendFactor);
                assertMethod(() => blendState[0].ColorWriteChannels = ColorWriteChannels.All);
            }
#endif
        }

        [Test]
        public void VisualTests()
        {
            var blends = new[]
            {
                Blend.One,
                Blend.Zero,
                Blend.SourceColor,
                Blend.InverseSourceColor,
                Blend.SourceAlpha,
                Blend.InverseSourceAlpha,
                Blend.DestinationColor,
                Blend.InverseDestinationColor,
                Blend.DestinationAlpha,
                Blend.InverseDestinationAlpha,
                Blend.BlendFactor,
                Blend.InverseBlendFactor,
                Blend.SourceAlphaSaturation,
            };

            SpriteBatch spriteBatch = null;
            Texture2D texture = null;
            BlendState[] blendStates = null;

            Game.LoadContentWith += (sender, e) =>
            {
                spriteBatch = new SpriteBatch(Game.GraphicsDevice);
                texture = Game.Content.Load<Texture2D>(Paths.Texture("MonoGameIcon"));

                blendStates = new BlendState[blends.Length * blends.Length];
                for (var y = 0; y < blends.Length; y++)
                    for (var x = 0; x < blends.Length; x++)
                    {
                        blendStates[(y * blends.Length) + x] = new BlendState
                        {
                            ColorSourceBlend = blends[y],
                            AlphaSourceBlend = blends[y],
                            ColorDestinationBlend = blends[x],
                            AlphaDestinationBlend = blends[x],
                            BlendFactor = new Color(0.3f, 0.5f, 0.7f)
                        };
                    }
            };

            Game.DrawWith += (sender, e) =>
            {
                var size = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
                var offset = new Vector2(10, 10);

                Game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);

                for (var y = 0; y < blends.Length; y++)
                    for (var x = 0; x < blends.Length; x++)
                    {
                        var pos = offset + new Vector2(x * size.X, y * size.Y);
                        spriteBatch.Begin(SpriteSortMode.Deferred, blendStates[(y * blends.Length) + x]);
                        spriteBatch.Draw(texture, new Rectangle((int) pos.X, (int) pos.Y, (int) size.X, (int) size.Y),
                            Color.White);
                        spriteBatch.End();
                    }
            };

            Game.UnloadContentWith += (sender, e) =>
            {
                spriteBatch.Dispose();
                spriteBatch = null;

                texture.Dispose();
                texture = null;

                foreach (BlendState blendState in blendStates)
                    blendState.Dispose();
            };

            RunSingleFrameTest();
        }
    }
}