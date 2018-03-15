// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
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
            var texture = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            game.GraphicsDevice.Textures[0] = null;

            var effect = new BasicEffect(game.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = texture;

            Assert.That(game.GraphicsDevice.Textures[0], Is.Null);

            var effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(game.GraphicsDevice.Textures[0], Is.SameAs(texture));

            texture.Dispose();
            effect.Dispose();
        }

        [Test]
        public void EffectPassShouldSetTextureOnSubsequentCalls()
        {
            var texture = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            game.GraphicsDevice.Textures[0] = null;

            var effect = new BasicEffect(game.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = texture;

            Assert.That(game.GraphicsDevice.Textures[0], Is.Null);

            var effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(game.GraphicsDevice.Textures[0], Is.SameAs(texture));

            game.GraphicsDevice.Textures[0] = null;

            effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(game.GraphicsDevice.Textures[0], Is.SameAs(texture));

            texture.Dispose();
            effect.Dispose();
        }

        [Test]
        public void EffectPassShouldSetTextureEvenIfNull()
        {
            var texture = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            game.GraphicsDevice.Textures[0] = texture;

            var effect = new BasicEffect(game.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = null;

            Assert.That(game.GraphicsDevice.Textures[0], Is.SameAs(texture));

            var effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(game.GraphicsDevice.Textures[0], Is.Null);

            texture.Dispose();
            effect.Dispose();
        }

        [Test]
        public void EffectPassShouldOverrideTextureIfNotExplicitlySet()
        {
            var texture = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            game.GraphicsDevice.Textures[0] = texture;

            var effect = new BasicEffect(game.GraphicsDevice);
            effect.TextureEnabled = true;

            Assert.That(game.GraphicsDevice.Textures[0], Is.SameAs(texture));

            var effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(game.GraphicsDevice.Textures[0], Is.Null);

            texture.Dispose();
            effect.Dispose();
        }

        [Test]
#if DESKTOPGL
        [Ignore("Fails under OpenGL!")]
#endif
        public void EffectParameterShouldBeSetIfSetByNameAndGetByIndex()
        {
            // This relies on the parameters permanently being on the same index.
            // Should be no problem except when adding parameters.
            var texture = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            game.GraphicsDevice.Textures[0] = texture;

            var effect = new BasicEffect(game.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = null;
            effect.Parameters["DiffuseColor"].SetValue(Color.HotPink.ToVector3());
            effect.Parameters["FogColor"].SetValue(Color.Honeydew.ToVector3());

            Assert.That(effect.Parameters[0].GetValueVector3().Equals(Color.HotPink.ToVector3()));
            Assert.That(effect.Parameters[14].GetValueVector3().Equals(Color.Honeydew.ToVector3()));

            texture.Dispose();
            effect.Dispose();
        }
    }
}
