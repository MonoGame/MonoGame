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
        [Test]
        public void TestSetValue()
        {
            var effect = new BasicEffect(game.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = null;
            effect.Parameters["SpecularPower"].SetValue(50);
            var val= effect.Parameters["SpecularPower"].GetValueSingle();
            Assert.AreEqual(50f, val);
        }

    }
}
