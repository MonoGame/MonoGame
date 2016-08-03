// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    internal class EffectTest
    {
        private TestGameBase Game;

        [SetUp]
        public void Setup()
        {
            Game = new TestGameBase();
            Game.InitializeOnly();
        }

        [TearDown]
        public void TearDown()
        {
            Game.Dispose();
        }

        [Test]
        public void EffectPassShouldSetTexture()
        {
            var texture = new Texture2D(Game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Game.GraphicsDevice.Textures[0] = null;

            var effect = new BasicEffect(Game.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = texture;

            Assert.That(Game.GraphicsDevice.Textures[0], Is.Null);

            var effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(Game.GraphicsDevice.Textures[0], Is.SameAs(texture));
        }

        [Test]
        public void EffectPassShouldSetTextureOnSubsequentCalls()
        {
            var texture = new Texture2D(Game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Game.GraphicsDevice.Textures[0] = null;

            var effect = new BasicEffect(Game.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = texture;

            Assert.That(Game.GraphicsDevice.Textures[0], Is.Null);

            var effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(Game.GraphicsDevice.Textures[0], Is.SameAs(texture));

            Game.GraphicsDevice.Textures[0] = null;

            effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(Game.GraphicsDevice.Textures[0], Is.SameAs(texture));
        }

        [Test]
        public void EffectPassShouldSetTextureEvenIfNull()
        {
            var texture = new Texture2D(Game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Game.GraphicsDevice.Textures[0] = texture;

            var effect = new BasicEffect(Game.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = null;

            Assert.That(Game.GraphicsDevice.Textures[0], Is.SameAs(texture));

            var effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(Game.GraphicsDevice.Textures[0], Is.Null);
        }

        [Test]
        public void EffectPassShouldOverrideTextureIfNotExplicitlySet()
        {
            Game.DrawWith += (sender, e) =>
            {
                var texture = new Texture2D(Game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                Game.GraphicsDevice.Textures[0] = texture;

                var effect = new BasicEffect(Game.GraphicsDevice);
                effect.TextureEnabled = true;

                Assert.That(Game.GraphicsDevice.Textures[0], Is.SameAs(texture));

                var effectPass = effect.CurrentTechnique.Passes[0];
                effectPass.Apply();

                Assert.That(Game.GraphicsDevice.Textures[0], Is.Null);
            };
            Game.Run();
        }
    }
}