// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
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
    }
}