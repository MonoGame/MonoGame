// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tests.ContentPipeline;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    internal class EffectTest : GraphicsDeviceTestFixtureBase
    {

        [Test]
        public void EffectPassShouldSetTexture()
        {
            var texture = new Texture2D(gd, 1, 1, false, SurfaceFormat.Color);
            gd.Textures[0] = null;

            var effect = new BasicEffect(gd);
            effect.TextureEnabled = true;
            effect.Texture = texture;

            Assert.That(gd.Textures[0], Is.Null);

            var effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(gd.Textures[0], Is.SameAs(texture));

            texture.Dispose();
            effect.Dispose();
        }

        [Test]
        public void EffectPassShouldSetTextureOnSubsequentCalls()
        {
            var texture = new Texture2D(gd, 1, 1, false, SurfaceFormat.Color);
            gd.Textures[0] = null;

            var effect = new BasicEffect(gd);
            effect.TextureEnabled = true;
            effect.Texture = texture;

            Assert.That(gd.Textures[0], Is.Null);

            var effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(gd.Textures[0], Is.SameAs(texture));

            gd.Textures[0] = null;

            effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(gd.Textures[0], Is.SameAs(texture));

            texture.Dispose();
            effect.Dispose();
        }

        [Test]
        public void EffectPassShouldSetTextureEvenIfNull()
        {
            var texture = new Texture2D(gd, 1, 1, false, SurfaceFormat.Color);
            gd.Textures[0] = texture;

            var effect = new BasicEffect(gd);
            effect.TextureEnabled = true;
            effect.Texture = null;

            Assert.That(gd.Textures[0], Is.SameAs(texture));

            var effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(gd.Textures[0], Is.Null);

            texture.Dispose();
            effect.Dispose();
        }

        [Test]
        public void EffectPassShouldOverrideTextureIfNotExplicitlySet()
        {
            var texture = new Texture2D(gd, 1, 1, false, SurfaceFormat.Color);
            gd.Textures[0] = texture;

            var effect = new BasicEffect(gd);
            effect.TextureEnabled = true;

            Assert.That(gd.Textures[0], Is.SameAs(texture));

            var effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(gd.Textures[0], Is.Null);

            texture.Dispose();
            effect.Dispose();
        }

        [Test]
        public void EffectPassShouldNotSetNullTextures()
        {
            var effect = AssetTestUtility.CompileEffect(gd, "SamplerRegistersEffect.fx");
            var t0 = new Texture2D(gd, 1, 1);
            var t1 = new Texture2D(gd, 1, 1);

            gd.Textures[0] = t0;
            gd.Textures[1] = t1;
            Assert.AreSame(t0, gd.Textures[0]);
            Assert.AreSame(t1, gd.Textures[1]);

            effect.CurrentTechnique = effect.Techniques["Both"];
            effect.CurrentTechnique.Passes[0].Apply();
            Assert.AreSame(t0, gd.Textures[0]);
            Assert.AreSame(t1, gd.Textures[1]);

            effect.CurrentTechnique = effect.Techniques["Zero"];
            effect.CurrentTechnique.Passes[0].Apply();
            Assert.AreSame(t0, gd.Textures[0]);
            Assert.AreSame(t1, gd.Textures[1]);

            effect.CurrentTechnique = effect.Techniques["One"];
            effect.CurrentTechnique.Passes[0].Apply();
            Assert.AreSame(t0, gd.Textures[0]);
            Assert.AreSame(t1, gd.Textures[1]);

            effect.Dispose();
            t0.Dispose();
            t1.Dispose();
        }
    }
}