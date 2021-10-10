using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Moq;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    internal class EffectParametersTest : GraphicsDeviceTestFixtureBase
    {
        /// <summary>
        /// Test case to check the set value with int input
        /// </summary>
        [Test]
        public void TestSetValue()
        {
            var effect = new BasicEffect(game.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = null;
            effect.Parameters["SpecularPower"].SetValue(50);
            var val= effect.Parameters["SpecularPower"].GetValueSingle();
            Assert.AreEqual(50f, val);

            var effect2 = new BasicEffect(game.GraphicsDevice);
            effect2.TextureEnabled = true;
            effect2.Texture = null;
            effect2.Parameters["SpecularPower"].SetValue(100f);
            var val2 = effect2.Parameters["SpecularPower"].GetValueSingle();
            Assert.AreEqual(100f, val2);

            var effect3 = new BasicEffect(game.GraphicsDevice);
            effect3.TextureEnabled = true;
            effect3.Texture = null;
            effect3.Parameters["SpecularPower"].SetValue(0.99f);
            var val3 = effect3.Parameters["SpecularPower"].GetValueSingle();
            Assert.AreEqual(0.99f, val3);
        }

    }
}
