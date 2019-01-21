// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tests.ContentPipeline;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    internal class SamplerStateTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ShouldNotBeAbleToSetNullSamplerState()
        {
            Assert.Throws<ArgumentNullException>(() => gd.SamplerStates[0] = null);
        }

        [Test]
        public void ShouldNotBeAbleToMutateStateObjectAfterBindingToGraphicsDevice()
        {
            var samplerState = new SamplerState();

            // Can mutate before binding.
            DoAsserts(samplerState, Assert.DoesNotThrow);

            // Can't mutate after binding.
            gd.SamplerStates[0] = samplerState;
            DoAsserts(samplerState, d => Assert.Throws<InvalidOperationException>(d));

            // Even after changing to different SamplerState, you still can't mutate a previously-bound object.
            gd.SamplerStates[0] = SamplerState.AnisotropicClamp;
            DoAsserts(samplerState, d => Assert.Throws<InvalidOperationException>(d));

            samplerState.Dispose();
        }

        [Test]
        public void ShouldNotBeAbleToMutateDefaultStateObjects()
        {
            DoAsserts(SamplerState.AnisotropicClamp, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(SamplerState.AnisotropicWrap, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(SamplerState.LinearClamp, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(SamplerState.LinearWrap, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(SamplerState.PointClamp, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(SamplerState.PointWrap, d => Assert.Throws<InvalidOperationException>(d));
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
            PrepareFrameCapture();

            var addressModes = new[]
            {
                TextureAddressMode.Border,
                TextureAddressMode.Clamp, 
                TextureAddressMode.Mirror, 
                TextureAddressMode.Wrap
            };


            var spriteBatch = new SpriteBatch(gd);

            var texture = content.Load<Texture2D>(Paths.Texture("MonoGameIcon"));

            var samplerStates = new SamplerState[addressModes.Length];
            for (var i = 0; i < addressModes.Length; i++)
                samplerStates[i] = new SamplerState
                {
                    AddressU = addressModes[i],
                    AddressV = addressModes[i],
                    AddressW = addressModes[i],
                    BorderColor = Color.Purple
                };

            var size = new Vector2(texture.Width * 2, texture.Height * 2);
            var offset = new Vector2(10, 10);

            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);

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

            CheckFrames();

            spriteBatch.Dispose();
            texture.Dispose();
            foreach (var state in samplerStates)
                state.Dispose();
        }
#endif

#if !XNA
        [Test]
#if DESKTOPGL
        [Ignore("Comparison samplers are ps_4_0 and up, cannot use them on DesktopGL due to MojoShader")]
#endif
        public void VisualTestComparisonFunction()
        {
            PrepareFrameCapture();

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

            var spriteBatch = new SpriteBatch(gd);

            // Texture contains a horizontal gradient [0..1].
            // In the shader, we compare samples from this texture to a hardcoded "0.5" value, 
            // and run the test once for each comparison function.
            var texture = new Texture2D(gd, 16, 1, false, SurfaceFormat.Single);
            var textureData = new float[texture.Width];
            for (var x = 0; x < texture.Width; x++)
                textureData[x] = x / (float) texture.Width;
            texture.SetData(textureData);

            var samplerStates = new SamplerState[compares.Length];
            for (var i = 0; i < compares.Length; i++)
                samplerStates[i] = new SamplerState
                {
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    ComparisonFunction = compares[i],
                    FilterMode = TextureFilterMode.Comparison
                };

            var customEffect = AssetTestUtility.LoadEffect(content, "CustomSpriteBatchEffectComparisonSampler");

            var size = new Vector2(100, 100);
            var offset = new Vector2(10, 10);

            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);

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

            CheckFrames();

            spriteBatch.Dispose();
            texture.Dispose();
            customEffect.Dispose();
            foreach (var state in samplerStates)
                state.Dispose();
        }
#endif
    }
}
