// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tests.ContentPipeline;
using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    internal class SamplerStateTest : VisualTestFixtureBase
    {
        [Test]
        public void ShouldNotBeAbleToSetNullSamplerState()
        {
            Game.DrawWith += (sender, e) =>
            {
                Assert.Throws<ArgumentNullException>(() => Game.GraphicsDevice.SamplerStates[0] = null);
            };
            Game.Run();
        }

        [Test]
        public void ShouldNotBeAbleToMutateStateObjectAfterBindingToGraphicsDevice()
        {
            Game.DrawWith += (sender, e) =>
            {
                var samplerState = new SamplerState();

                // Can mutate before binding.
                DoAsserts(samplerState, Assert.DoesNotThrow);

                // Can't mutate after binding.
                Game.GraphicsDevice.SamplerStates[0] = samplerState;
                DoAsserts(samplerState, d => Assert.Throws<InvalidOperationException>(d));

                // Even after changing to different SamplerState, you still can't mutate a previously-bound object.
                Game.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
                DoAsserts(samplerState, d => Assert.Throws<InvalidOperationException>(d));
            };
            Game.Run();
        }

        [Test]
        public void ShouldNotBeAbleToMutateDefaultStateObjects()
        {
            Game.DrawWith += (sender, e) =>
            {
                DoAsserts(SamplerState.AnisotropicClamp, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(SamplerState.AnisotropicWrap, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(SamplerState.LinearClamp, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(SamplerState.LinearWrap, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(SamplerState.PointClamp, d => Assert.Throws<InvalidOperationException>(d));
                DoAsserts(SamplerState.PointWrap, d => Assert.Throws<InvalidOperationException>(d));
            };
            Game.Run();
        }

        private static void DoAsserts(SamplerState samplerState, Action<TestDelegate> assertMethod)
        {
            assertMethod(() => samplerState.AddressU = TextureAddressMode.Clamp);
            assertMethod(() => samplerState.AddressV = TextureAddressMode.Clamp);
            assertMethod(() => samplerState.AddressW = TextureAddressMode.Clamp);
#if !XNA
            assertMethod(() => samplerState.BorderColor = Color.Red);
#endif
            assertMethod(() => samplerState.Filter = TextureFilter.Anisotropic);
            assertMethod(() => samplerState.MaxAnisotropy = 0);
            assertMethod(() => samplerState.MaxMipLevel = 0);
            assertMethod(() => samplerState.MipMapLevelOfDetailBias = 0);
#if !XNA
            assertMethod(() => samplerState.ComparisonFunction = CompareFunction.Always);
#endif
        }

#if !XNA
        [Test]
        public void VisualTestAddressModes()
        {
            var addressModes = new[]
            {
                TextureAddressMode.Border,
                TextureAddressMode.Clamp, 
                TextureAddressMode.Mirror, 
                TextureAddressMode.Wrap
            };

            SpriteBatch spriteBatch = null;
            Texture2D texture = null;
            SamplerState[] samplerStates = null;
            Effect effect = null;

            Game.LoadContentWith += (sender, e) =>
            {
                spriteBatch = new SpriteBatch(Game.GraphicsDevice);

                texture = Game.Content.Load<Texture2D>(Paths.Texture("MonoGameIcon"));

                samplerStates = new SamplerState[addressModes.Length];
                for (var i = 0; i < addressModes.Length; i++)
                    samplerStates[i] = new SamplerState
                    {
                        AddressU = addressModes[i],
                        AddressV = addressModes[i],
                        AddressW = addressModes[i],
                        BorderColor = Color.Purple
                    };
            };

            Game.DrawWith += (sender, e) =>
            {
                var size = new Vector2(texture.Width * 2, texture.Height * 2);
                var offset = new Vector2(10, 10);

                Game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);

                for (var i = 0; i < addressModes.Length; i++)
                {
                    var x = i % 4;
                    var pos = offset + new Vector2(x * size.X, 0);

                    spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: samplerStates[i]);
                    spriteBatch.Draw(texture, 
                        new Rectangle((int) pos.X, (int) pos.Y, (int) size.X, (int) size.Y),
                        new Rectangle(-20, -20, texture.Width + 40, texture.Height + 40),
                        Color.White);
                    spriteBatch.End();
                }
            };

            RunSingleFrameTest();
        }
#endif

#if !XNA
        [Test]
        public void VisualTestComparisonFunction()
        {
            var compares = new[]
            {
                CompareFunction.Always,
                CompareFunction.Equal, 
                CompareFunction.Greater, 
                CompareFunction.GreaterEqual,
                CompareFunction.Less, 
                CompareFunction.LessEqual, 
                CompareFunction.Never, 
                CompareFunction.NotEqual
            };

            SpriteBatch spriteBatch = null;
            Texture2D texture = null;
            SamplerState[] samplerStates = null;
            Effect customEffect = null;

            Game.LoadContentWith += (sender, e) =>
            {
                spriteBatch = new SpriteBatch(Game.GraphicsDevice);

                // Texture contains a horizontal gradient [0..1].
                // In the shader, we compare samples from this texture to a hardcoded "0.5" value, 
                // and run the test once for each comparison function.
                texture = new Texture2D(Game.GraphicsDevice, 16, 1, false, SurfaceFormat.Single);
                var textureData = new float[texture.Width];
                for (var x = 0; x < texture.Width; x++)
                    textureData[x] = x / (float) texture.Width;
                texture.SetData(textureData);

                samplerStates = new SamplerState[compares.Length];
                for (var i = 0; i < compares.Length; i++)
                    samplerStates[i] = new SamplerState
                    {
                        AddressU = TextureAddressMode.Clamp,
                        AddressV = TextureAddressMode.Clamp,
                        AddressW = TextureAddressMode.Clamp,
                        ComparisonFunction = compares[i]
                    };

                customEffect = AssetTestUtility.CompileEffect(Game.GraphicsDevice, 
                    "CustomSpriteBatchEffectComparisonSampler.fx");
            };

            Game.DrawWith += (sender, e) =>
            {
                var size = new Vector2(100, 100);
                var offset = new Vector2(10, 10);

                Game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);

                for (var i = 0; i < compares.Length; i++)
                {
                    var x = i % 4;
                    var y = (i > 3) ? 1 : 0;
                    var pos = offset + new Vector2(x * size.X, y * size.Y);

                    spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: samplerStates[i], effect: customEffect);
                    spriteBatch.Draw(texture, 
                        new Rectangle((int) pos.X, (int) pos.Y, (int) size.X, (int) size.Y),
                        Color.White);
                    spriteBatch.End();
                }
            };

            RunSingleFrameTest();
        }
#endif
    }
}
